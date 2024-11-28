pipeline {
    agent { label 'App' }

    options {
        buildDiscarder(logRotator(numToKeepStr: '10'))
    }

    environment {
        JENKINS_SERVER = 'http://139.162.132.174:8080'
        DOTNET_PROJECT_PATH = 'Rise.Server/Rise.Server.csproj'
        DOTNET_TEST_PATH = 'Rise.Domain.Tests/Rise.Domain.Tests.csproj'
        PUBLISH_OUTPUT = 'publish'
        DISCORD_WEBHOOK_URL = "https://discord.com/api/webhooks/1301160382307766292/kROxjtgZ-XVOibckTMri2fy5-nNOEjzjPLbT9jEpr_R0UH9JG0ZXb2XzUsYGE0d3yk6I"
        REPO_NAME = "git@github.com:Brahim-Mahfoudhi/dev-repo.git"
        PR_NUMBER = "${params.PR_NUMBER}"
    }

    parameters {
        string(name: 'PR_NUMBER', defaultValue: '', description: 'Pull Request Number')
    }

    stages {
        stage('Clean Workspace') {
            steps {
                cleanWs()
            }
        }

        stage('Find PR Number') {
            steps {
                script {
                    checkout scm
                    
                    def commitSHA = sh(script: 'git rev-parse HEAD', returnStdout: true).trim()
                    echo "Current commit SHA: ${commitSHA}"
        
                    def apiUrl = "https://api.github.com/repos/Brahim-Mahfoudhi/dev-repo/commits/${commitSHA}/pulls"
                    
                    withCredentials([string(credentialsId: "GitHub-Personal-Access-Token-for-Jenkins", variable: 'GITHUB_TOKEN')]) {
                        def response = httpRequest(
                            url: apiUrl,
                            httpMode: 'GET',
                            customHeaders: [[name: 'Authorization', value: "Bearer ${GITHUB_TOKEN}"]],
                            validResponseCodes: '200:299',
                            contentType: 'APPLICATION_JSON'
                        )
        
                        def prNumber = ""
                        if (response) {
                            def jsonResponse = readJSON text: response.content
                            if (jsonResponse.size() > 0) {
                                prNumber = jsonResponse[0].number
                            }
                        }
                        env.PR_NUMBER = prNumber
                        echo "PR Number: ${env.PR_NUMBER}"
                    }
                }
            }
        }

        stage('Checkout Code') {
            steps {
                script {
                    if (!env.PR_NUMBER) {
                        error "PR_NUMBER is not set. Please ensure the PR number is provided."
                    }
                    checkout([$class: 'GitSCM', 
                        branches: [[name: "refs/pull/${env.PR_NUMBER}/head"]], 
                        userRemoteConfigs: [[url: 'git@github.com:Brahim-Mahfoudhi/dev-repo.git', credentialsId: 'jenkins-master-key']]])

                    def gitInfo = sh(script: 'git show -s HEAD --pretty=format:"%an%n%ae%n%s%n%H%n%h" 2>/dev/null', returnStdout: true).trim().split("\n")
                    env.GIT_AUTHOR_NAME = gitInfo[0]
                    env.GIT_AUTHOR_EMAIL = gitInfo[1]
                    env.GIT_COMMIT_MESSAGE = gitInfo[2]
                    env.GIT_COMMIT = gitInfo[3]
                    env.GIT_BRANCH = gitInfo[4]
                }
            }
        }

        stage('Restore Dependencies') {
            steps {
                sh "dotnet restore ${DOTNET_PROJECT_PATH}"
                sh "dotnet restore ${DOTNET_TEST_PATH}"
            }
        }

        stage('Build Application') {
            steps {
                sh "dotnet build ${DOTNET_PROJECT_PATH}"
            }
        }

        stage('Running Unit Tests') {
            steps {
                echo 'Running unit tests and collecting Clover coverage data...'
                sh """
                    dotnet test ${DOTNET_TEST_PATH} --collect:"XPlat Code Coverage" --logger 'trx;LogFileName=test-results.trx' \
                    /p:CollectCoverage=true /p:CoverletOutput='/var/lib/jenkins/agent/workspace/dotnet_pipeline/coverage/coverage.xml' \
                    /p:CoverletOutputFormat=cobertura
                """
            }
        }

        stage('Coverage Report') {
            steps {
                script {
                    def testOutput = sh(script: "dotnet test ${DOTNET_TEST_PATH} --collect \"XPlat Code Coverage\"", returnStdout: true).trim()
                    def coverageFiles = testOutput.split('\n').findAll { it.contains('coverage.cobertura.xml') }.join(';')
                    echo "Coverage files: ${coverageFiles}"

                    if (coverageFiles) {
                        sh """
                            mkdir -p /var/lib/jenkins/agent/workspace/dotnet_pipeline/coverage-report/
                            mkdir -p /var/lib/jenkins/agent/workspace/dotnet_pipeline/coverage/
                            cp ${coverageFiles} /var/lib/jenkins/agent/workspace/dotnet_pipeline/coverage/
                            /home/jenkins/.dotnet/tools/reportgenerator -reports:/var/lib/jenkins/agent/workspace/dotnet_pipeline/coverage/coverage.cobertura.xml -targetdir:/var/lib/jenkins/agent/workspace/dotnet_pipeline/coverage-report/ -reporttype:Html
                        """
                    } else {
                        error 'No coverage files found'
                    }
                }
                echo 'Publishing coverage report...'
                publishHTML([ 
                    allowMissing: false,
                    alwaysLinkToLastBuild: true,
                    keepAll: true,
                    reportDir: '/var/lib/jenkins/agent/workspace/dotnet_pipeline/coverage-report',
                    reportFiles: 'index.html',
                    reportName: 'Clover Coverage Report'
                ])
            }
        }

        stage('Update GitHub Status') {
            steps {
                script {
                    checkout scm  

                    def status = currentBuild.result == 'SUCCESS' ? 'success' : 'error'
                    def commitSHA = sh(script: 'git rev-parse HEAD', returnStdout: true).trim()

                    def requestBody = """
                    {
                        "status": "${status}",
                        "target_url": "${env.JENKINS_SERVER}/job/${env.JOB_NAME}/${env.BUILD_NUMBER}/",
                        "description": "Jenkins build ${status}",
                        "context": "Jenkins Build"
                    }
                    """

                    withCredentials([string(credentialsId: "GitHub-Personal-Access-Token-for-Jenkins", variable: 'GITHUB_TOKEN')]) {
                        def response = httpRequest(
                            url: "https://api.github.com/repos/${REPO_NAME}/statuses/${commitSHA}",
                            httpMode: 'POST',
                            customHeaders: [[name: 'Authorization', value: "Bearer ${GITHUB_TOKEN}"]],
                            contentType: 'APPLICATION_JSON',
                            requestBody: requestBody
                        )
                        echo "GitHub Status update response: ${response}"
                    }
                }
            }
        }

        stage('Merge Pull Request') {
            when {
                expression { currentBuild.result == null || currentBuild.result == 'SUCCESS' }
            }
            steps {
                script {
                    withCredentials([string(credentialsId: "GitHub-Personal-Access-Token-for-Jenkins", variable: 'GITHUB_TOKEN')]) {
                        def prNumber = "${env.PR_NUMBER}"

                        if (prNumber) {
                            try {
                                def mergeStatus = sh(script: "curl -s -H 'Authorization: Bearer ${GITHUB_TOKEN}' 'https://api.github.com/repos/Brahim-Mahfoudhi/dev-repo/pulls/${prNumber}'", returnStdout: true).trim()
                                def mergeable = readJSON text: mergeStatus
                                if (mergeable.mergeable == true) {
                                    def requestBody = """
                                    {
                                        "commit_title": "Auto-merged by Jenkins",
                                        "commit_message": "This pull request was merged automatically by Jenkins after successful tests.",
                                        "merge_method": "merge"
                                    }
                                    """
                                    def response = httpRequest(
                                        url: "https://api.github.com/repos/Brahim-Mahfoudhi/dev-repo/pulls/${prNumber}/merge",
                                        httpMode: 'PUT',
                                        customHeaders: [
                                            [name: 'Authorization', value: "Bearer ${GITHUB_TOKEN}"],
                                            [name: 'Accept', value: 'application/vnd.github.v3+json']
                                        ],
                                        validResponseCodes: '200:299',
                                        contentType: 'APPLICATION_JSON',
                                        requestBody: requestBody
                                    )

                                    echo "Pull Request Merge Response: ${response.content}"
                                } else {
                                    error "Pull request is not mergeable."
                                }
                            } catch (Exception e) {
                                error "Failed to merge PR: ${e.message}"
                            }
                        } else {
                            error 'PR_NUMBER is not set. Cannot merge PR.'
                        }
                    }
                }
            }
        }
    }

    post {
        success {
            script {
                sendDiscordNotification("Build Success")
                withCredentials([string(credentialsId: "GitHub-Personal-Access-Token-for-Jenkins", variable: 'GITHUB_TOKEN')]) {
                    githubNotify(
                        context: 'Jenkins Build',
                        status: 'SUCCESS',
                        description: 'Build successful',
                        account: 'Brahim-Mahfoudhi',
                        credentialsId: "${GITHUB_TOKEN}",
                        repo: 'Brahim-Mahfoudhi/dev-repo',
                        sha: sh(script: 'git rev-parse HEAD', returnStdout: true).trim(),
                        targetUrl: currentBuild.absoluteUrl
                    )
                }
            }
        }
        failure {
            script {
                sendDiscordNotification("Build Failed")
                withCredentials([string(credentialsId: "GitHub-Personal-Access-Token-for-Jenkins", variable: 'GITHUB_TOKEN')]) {
                    githubNotify(
                        context: 'Jenkins Build',
                        status: 'ERROR',
                        description: 'Build failed',
                        account: 'Brahim-Mahfoudhi',
                        credentialsId: "${GITHUB_TOKEN}",
                        repo: 'Brahim-Mahfoudhi/dev-repo',
                        sha: sh(script: 'git rev-parse HEAD', returnStdout: true).trim(),
                        targetUrl: currentBuild.absoluteUrl
                    )
                }
            }
        }
    }
}

def sendDiscordNotification(message) {
    def payload = [
        content: message,
        username: "Jenkins",
        avatar_url: "https://www.jenkins.io/images/logos/jenkins/jenkins.png"
    ]
    httpRequest(
        url: DISCORD_WEBHOOK_URL,
        httpMode: 'POST',
        requestBody: groovy.json.JsonOutput.toJson(payload),
        contentType: 'APPLICATION_JSON'
    )
}
