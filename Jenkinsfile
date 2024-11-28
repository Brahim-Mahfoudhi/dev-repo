pipeline {
    agent { label 'App' }

    options {
        buildDiscarder(logRotator(numToKeepStr: '10'))
    }

    environment {
        JENKINS_SERVER = 'http://139.162.132.174:8080'
        DOTNET_PROJECT_PATH = 'Rise.Server/Rise.Server.csproj'
        DOTNET_TEST_PATH = 'Rise.Domain.Tests/Rise.Domain.Tests.csproj'
        REPO_OWNER = "Brahim-Mahfoudhi"
        REPO_NAME = "dev-repo"
        GIT_BRANCH = "main"
        DISCORD_WEBHOOK_URL = "https://discord.com/api/webhooks/1301160382307766292/kROxjtgZ-XVOibckTMri2fy5-nNOEjzjPLbT9jEpr_R0UH9JG0ZXb2XzUsYGE0d3yk6I"
    }

    parameters {
        string(name: 'PR_NUMBER', defaultValue: '', description: 'Pull Request Number (Optional, leave empty to test a branch)')
        string(name: 'BRANCH_NAME', defaultValue: 'main', description: 'Branch to build and test if no PR_NUMBER is provided')
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
                    if (params.PR_NUMBER) {
                        echo "Checking out PR #${params.PR_NUMBER}"
                        // Checkout the PR using refs/pull
                        checkout([
                            $class: 'GitSCM',
                            branches: [[name: "refs/pull/${params.PR_NUMBER}/head"]],
                            userRemoteConfigs: [[
                                url: "git@github.com:${env.REPO_OWNER}/${env.REPO_NAME}.git",
                                credentialsId: 'jenkins-master-key'
                            ]]
                        ])
                    } else {
                        echo "Checking out branch ${params.BRANCH_NAME}"
                        // Checkout the branch directly
                        checkout([
                            $class: 'GitSCM',
                            branches: [[name: "*/${params.BRANCH_NAME}"]],
                            userRemoteConfigs: [[
                                url: "git@github.com:${env.REPO_OWNER}/${env.REPO_NAME}.git",
                                credentialsId: 'jenkins-master-key'
                            ]]
                        ])
                    }
                }
            }
        }

        stage('Restore Dependencies') {
            steps {
                echo "Restoring dependencies..."
                sh "dotnet restore ${DOTNET_PROJECT_PATH}"
                sh "dotnet restore ${DOTNET_TEST_PATH}"
            }
        }

        stage('Build Application') {
            steps {
                echo "Building application..."
                sh "dotnet build ${DOTNET_PROJECT_PATH} --no-restore"
            }
        }

        stage('Run Unit Tests') {
            steps {
                echo "Running unit tests..."
                sh "dotnet test ${DOTNET_TEST_PATH} --no-restore --verbosity normal"
            }
        }
    }

    post {
        success {
            script {
                sendDiscordNotification("✅ Build Success: ${params.PR_NUMBER ? "PR #${params.PR_NUMBER}" : "Branch ${params.BRANCH_NAME}"}")
            }
        }
        failure {
            script {
                sendDiscordNotification("❌ Build Failed: ${params.PR_NUMBER ? "PR #${params.PR_NUMBER}" : "Branch ${params.BRANCH_NAME}"}")
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
