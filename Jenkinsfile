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
        JENKINS_CREDENTIALS_ID = "jenkins-master-key"
        REPO_NAME = "Brahim-Mahfoudhi/dev-repo"
        PR_NUMBER = "${ghprbPullId}"
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
                    // Ensure the repository is checked out
                    checkout scm  // This ensures that the repository is properly checked out before using git
        
                    // Set the status to success or error
                    def status = currentBuild.result == 'SUCCESS' ? 'success' : 'error'
        
                    // Get the commit SHA
                    def commitSHA = sh(script: 'git rev-parse HEAD', returnStdout: true).trim()
        
                    // Prepare the GitHub API request body
                    def requestBody = """
                    {
                        "state": "${status}",
                        "target_url": "${env.JENKINS_SERVER}/job/${env.JOB_NAME}/${env.BUILD_NUMBER}/",
                        "description": "Jenkins build ${status}",
                        "context": "Jenkins Build"
                    }
                    """
        
                    // Send a request to update the commit status using GitHub API
                    httpRequest(
                        url: "https://api.github.com/repos/${REPO_NAME}/statuses/${commitSHA}",
                        httpMode: 'POST',
                        authentication: "jenkins-master-key",
                        contentType: 'APPLICATION_JSON',
                        requestBody: requestBody
                    )
                }
            }
        }


        stage('Merge to Main') {
            when {
                branch 'main'
                expression { currentBuild.result == 'SUCCESS' && isTestsSuccessful() }
            }
            steps {
                script {
                    def prNumber = env.PR_NUMBER
                    if (prNumber) {
                        try {
                            def response = httpRequest(
                                url: "https://api.github.com/repos/Brahim-Mahfoudhi/dev-repo/pulls/${prNumber}/merge",
                                httpMode: 'PUT',
                                authentication: "${JENKINS_CREDENTIALS_ID}",
                                customHeaders: [[name: 'Accept', value: 'application/vnd.github.v3+json']],
                                validResponseCodes: '200:299',
                                contentType: 'APPLICATION_JSON',
                                requestBody: '{"commit_title": "Auto-merged by Jenkins"}'
                            )
                            echo "Merge response: ${response.content}"
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

    post {
        success {
            script {
                githubNotify context: 'continuous-integration/jenkins', state: 'success', description: 'Tests passed'
                sendDiscordNotification("Build Success")
            }
        }
        failure {
            script {
                sendDiscordNotification("Build Failed")
                githubNotify context: 'continuous-integration/jenkins', state: 'failure', description: 'Tests failed'
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
