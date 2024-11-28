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
        string(name: 'PR_NUMBER', defaultValue: '', description: 'Pull Request Number (Optional)')
    }

    stages {
        stage('Clean Workspace') {
            steps {
                cleanWs()
            }
        }

        stage('Get PR Number') {
            when {
                expression { !params.PR_NUMBER } // Run this stage only if PR_NUMBER is not provided
            }
            steps {
                script {
                    echo "Fetching PR number from GitHub..."
                    def response = httpRequest(
                        url: "https://api.github.com/repos/${env.REPO_OWNER}/${env.REPO_NAME}/pulls?head=${env.REPO_OWNER}:${env.GIT_BRANCH}",
                        httpMode: 'GET',
                        customHeaders: [[name: 'Authorization', value: "Bearer ${env.GITHUB_TOKEN}"]],
                        validResponseCodes: '200'
                    )
                    def jsonResponse = readJSON text: response.content
                    if (jsonResponse && jsonResponse.size() > 0) {
                        env.PR_NUMBER = jsonResponse[0].number.toString()
                        echo "PR number is: ${env.PR_NUMBER}"
                    } else {
                        error "No PR found for branch ${env.GIT_BRANCH}."
                    }
                }
            }
        }

        stage('Checkout Code') {
            steps {
                script {
                    if (!env.PR_NUMBER) {
                        error "PR_NUMBER is not set. Please ensure the PR number is provided or fetched correctly."
                    }
                    checkout([
                        $class: 'GitSCM',
                        branches: [[name: "refs/pull/${env.PR_NUMBER}/head"]],
                        userRemoteConfigs: [[
                            url: "git@github.com:${env.REPO_OWNER}/${env.REPO_NAME}.git",
                            credentialsId: 'jenkins-master-key'
                        ]]
                    ])
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
                sendDiscordNotification("Build Success: PR #${env.PR_NUMBER}")
            }
        }
        failure {
            script {
                sendDiscordNotification("Build Failed: PR #${env.PR_NUMBER}")
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
