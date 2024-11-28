pipeline {
    agent { label 'App' }

    options {
        buildDiscarder(logRotator(numToKeepStr: '10'))
    }

    environment {
        JENKINS_SERVER = 'http://139.162.132.174:8080'
        DOTNET_PROJECT_PATH = 'Rise.Server/Rise.Server.csproj'
        DOTNET_TEST_PATH = 'Rise.Domain.Tests/Rise.Domain.Tests.csproj'
        REPO_NAME = "git@github.com:Brahim-Mahfoudhi/dev-repo.git"
        PR_NUMBER = "${params.PR_NUMBER}"
        DISCORD_WEBHOOK_URL = "https://discord.com/api/webhooks/1301160382307766292/kROxjtgZ-XVOibckTMri2fy5-nNOEjzjPLbT9jEpr_R0UH9JG0ZXb2XzUsYGE0d3yk6I"
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

    stage('Get PR Number') {
            steps {
                script {
                    def prNumber = env.CHANGE_ID
                    echo "The PR number is ${prNumber}"
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

        stage('Run Unit Tests') {
            steps {
                echo 'Running unit tests...'
                sh "dotnet test ${DOTNET_TEST_PATH}"
            }
        }
    }

    post {
        success {
            script {
                sendDiscordNotification("Build Success")
            }
        }
        failure {
            script {
                sendDiscordNotification("Build Failed")
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
