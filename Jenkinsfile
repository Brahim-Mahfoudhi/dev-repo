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
        REPO_NAME = "Brahim-Mahfoudhi/dev-repo"
        PR_NUMBER = ""
    }

    stages {
        stage('Clean Workspace') {
            steps {
                cleanWs()
            }
        }

        stage('Checkout Code') {
            steps {
                script {
                    checkout([$class: 'GitSCM', 
                        branches: [[name: 'refs/pull/*/merge']], 
                        userRemoteConfigs: [[url: 'git@github.com:Brahim-Mahfoudhi/dev-repo.git', credentialsId: 'jenkins-master-key']]
                    ])
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
                    // Ensure the repository is checked out before using git
                    checkout scm  
        
                    def status = currentBuild.result == 'SUCCESS' ? 'success' : 'error'
                    def commitSHA = sh(script: 'git rev-parse HEAD', returnStdout: true).trim()
        
                    def requestBody = """
                    {
                        "state": "${status}",
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

        stage('Debug All Environment Variables') {
            steps {
                script {
                    echo "Listing all environment variables..."
                    sh 'env | sort'
                }
            }
        }

        stage('Set PR_NUMBER') {
            steps {
                script {
                    // Debug: Print environment variables to verify correct ones are set
                    echo "ghprbPullId: ${env.ghprbPullId}"
                    echo "CHANGE_ID: ${env.CHANGE_ID}"

                    // Attempt to use Jenkins-provided variables
                    if (env.ghprbPullId) {
                        env.PR_NUMBER = env.ghprbPullId
                    } else if (env.CHANGE_ID) {
                        env.PR_NUMBER = env.CHANGE_ID
                    } else {
                        // Fallback: Try extracting manually
                        def branchName = sh(script: "git rev-parse --abbrev-ref HEAD", returnStdout: true).trim()
                        echo "Branch Name: ${branchName}"
                        if (branchName =~ /^PR-(\\d+)$/) {
                            env.PR_NUMBER = branchName.replaceAll("PR-", "")
                        } else {
                            error "PR_NUMBER could not be determined. Please verify your pipeline setup."
                        }
                    }
                    echo "Resolved PR_NUMBER: ${env.PR_NUMBER}"
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
                        def prNumber = "${PR_NUMBER}"
        
                        // Debug: Ensure PR_NUMBER is set correctly
                        echo "PR_NUMBER: ${prNumber}"

                        if (prNumber) {
                            try {
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
                                        [name: 'Authorization', value: "Bearer ${GITHUB_TOKEN}"], // Pass the token in header
                                        [name: 'Accept', value: 'application/vnd.github.v3+json'] // Specify GitHub API version
                                    ],
                                    validResponseCodes: '200:299', // Allow success responses
                                    contentType: 'APPLICATION_JSON', // Specify JSON content
                                    requestBody: requestBody // JSON payload for the merge
                                )
        
                                echo "Pull Request Merge Response: ${response.content}"
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
                githubNotify context: 'Jenkins Build', status: 'SUCCESS', description: 'Tests passed'
                sendDiscordNotification("Build Success")
            }
        }
        failure {
            script {
                sendDiscordNotification("Build Failed")
                githubNotify context: 'Jenkins Build ', state: 'FAILURE', description: 'Tests failed'
            }
        }
    }
}

def isTestsSuccessful() {
    return currentBuild.result == 'SUCCESS'
}

def sendDiscordNotification(status) {
    discordSend(
        title: "${env.JOB_NAME} - ${status}",
        description: "Build #${env.BUILD_NUMBER} - ${status}\nSee details: ${env.JENKINS_SERVER}/job/${env.JOB_NAME}/${env.BUILD_NUMBER}/",
        webhookURL: "${DISCORD_WEBHOOK_URL}",
        result: status == "Build Success" ? 'SUCCESS' : 'FAILURE'
    )
}
