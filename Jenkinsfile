pipeline {
    agent { label 'App' }
    
    options {
        buildDiscarder(logRotator(numToKeepStr: '10'))
    }

    environment {
        JENKINS_SERVER = 'http://139.162.132.174:8080'
        DOTNET_PROJECT_PATH = 'Rise.Server/Rise.Server.csproj'
        REPO_OWNER = "Brahim-Mahfoudhi"
        REPO_NAME = "dev-repo"
        DISCORD_WEBHOOK_URL = "https://discord.com/api/webhooks/1301160382307766292/kROxjtgZ-XVOibckTMri2fy5-nNOEjzjPLbT9jEpr_R0UH9JG0ZXb2XzUsYGE0d3yk6I"
    }

    parameters {
        string(name: 'sha1', defaultValue: '', description: 'Commit sha1')
    }

    stages {
        stage('Clean Workspace') {
            steps {
                echo "Cleaning Git repository"
                sh 'git clean -fdx'
                echo "Fetching latest code"
                cleanWs(deleteDirs: true)
                sshagent(credentials: ['jenkins-master-key']) {
                    sh '''
                        ssh -i /var/lib/jenkins/.ssh/control_node -o StrictHostKeyChecking=no root@139.162.132.174 "rm -rf /var/lib/jenkins/workspace/merge-pipeline@script"
                    '''
                }
                checkout scm
            }
        }

        stage('Checkout Code') {
            steps {
                script {
                    if (params.sha1) {
                        echo "Checking out commit ${params.sha1}."
                        if (params.sha1.startsWith("origin/pr/")) {
                            echo "Fetching and checking out pull request ${params.sha1}."
                            def prNumber = params.sha1.split('/')[2]
                            
                            sshagent(credentials: ['jenkins-master-key']) {
                                sh "git fetch origin +refs/pull/${prNumber}/head:refs/remotes/origin/pr-${prNumber}-head"
                                sh "git fetch origin +refs/pull/${prNumber}/merge:refs/remotes/origin/pr-${prNumber}-merge"
                            }

                            sh "git checkout refs/remotes/origin/pr-${prNumber}-merge || git checkout refs/remotes/origin/pr-${prNumber}-head"
                        } else {
                            sh "git fetch origin ${params.sha1}"
                            sh "git checkout ${params.sha1}"
                        }
                    } else {
                        echo "No sha1 provided. Checking the main branch"
                        git credentialsId: 'jenkins-master-key', url: "git@github.com:${REPO_OWNER}/${REPO_NAME}.git", branch: 'main'
                    }
        
                    echo 'Gathering GitHub info!'
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
                echo "Restoring dependencies..."
                sh "dotnet restore ${DOTNET_PROJECT_PATH}"
                script {
                    def testPaths = [
                        domain: 'Rise.Domain.Tests/Rise.Domain.Tests.csproj',
                        client: 'Rise.Client.Tests/Rise.Client.Tests.csproj',
                        server: 'Rise.Server.Tests/Rise.Server.Tests.csproj',
                        service: 'Rise.Services.Tests/Rise.Services.Tests.csproj'
                    ]
                    
                    testPaths.each { name, path ->
                        echo "Restoring unit tests for ${name} located at ${path}..."
                        sh "dotnet restore ${path}"
                    }
                }
            }
        }

        stage('Build Application') {
            steps {
                echo "Building application..."
                sh "dotnet build ${DOTNET_PROJECT_PATH} --no-restore"
            }
        }

        stage('Running Unit Tests') {
            steps {
                script {
                    def testPaths = [
                        Domain: 'Rise.Domain.Tests/Rise.Domain.Tests.csproj',
                        Client: 'Rise.Client.Tests/Rise.Client.Tests.csproj',
                        Server: 'Rise.Server.Tests/Rise.Server.Tests.csproj',
                        Service: 'Rise.Services.Tests/Rise.Services.Tests.csproj'
                    ]
                    
                    testPaths.each { name, path ->
                        echo "Running unit tests for ${name} located at ${path}..."
                        
                        sh """
                             dotnet test ${path} --collect:"XPlat Code Coverage" --logger 'trx;LogFileName=${name}.trx' \
                             /p:CollectCoverage=true /p:CoverletOutput='/var/lib/jenkins/agent/workspace/dotnet_pipeline/coverage/coverage.xml' \
                             /p:CoverletOutputFormat=cobertura
                        """
                        
                        echo "Generating coverage report for ${name}..."
                        sh """
                            mkdir -p /var/lib/jenkins/agent/workspace/coverage-report/${name}
                            /home/jenkins/.dotnet/tools/reportgenerator \
                                -reports:/var/lib/jenkins/agent/workspace/merge-pipeline/Rise.${name}.Tests/TestResults/${name}.xml \
                                -targetdir:/var/lib/jenkins/agent/workspace/coverage-report/ \
                                -reporttype:Html
                        """
                        
                        publishHTML([
                            allowMissing: false,
                            alwaysLinkToLastBuild: true,
                            keepAll: true,
                            reportDir: "/var/lib/jenkins/agent/workspace/coverage-report/${name}",
                            reportFiles: 'index.html',
                            reportName: "Coverage Report for ${name}"
                        ])
                    }
                }
            }
        }

/*
        stage('Coverage Report') {
            steps {
                script {
                    def testOutput = sh(script: "dotnet test ${DOTNET_PROJECT_PATH} --collect \"XPlat Code Coverage\"", returnStdout: true).trim()
                    def coverageFiles = testOutput.split('\n').findAll { it.contains('coverage.cobertura.xml') }.join(';')
                    echo "Coverage files: ${coverageFiles}"

                    if (coverageFiles) {
                        sh """
                            mkdir -p /var/lib/jenkins/agent/workspace/dotnet_pipeline/coverage-report/
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
                    reportName: 'Coverage Report'
                ])
            }
        }
        */
    }

    post {
        success {
            echo 'Merge Tests completed successfully!'
            script {
                sendDiscordNotification("Build Success")
            }
        }
        failure {
            echo 'Merge Tests have failed.'
            script {
                sendDiscordNotification("Build Failed")
            }
        }
    }
}

def sendDiscordNotification(status) {
    script {
        discordSend(
            title: "${env.JOB_NAME} - ${status}",
            description: """
                Build #${env.BUILD_NUMBER} ${status == "Build Success" ? 'completed successfully!' : 'has failed!'}
                **Commit**: ${env.GIT_COMMIT}
                **Author**: ${env.GIT_AUTHOR_NAME} <${env.GIT_AUTHOR_EMAIL}>
                **Branch**: ${env.GIT_BRANCH}
                **Message**: ${env.GIT_COMMIT_MESSAGE}
                
                [**Build output**](${env.JENKINS_SERVER}/job/${env.JOB_NAME}/${env.BUILD_NUMBER}/console)
                [**Test result**](${env.JENKINS_SERVER}/job/${env.JOB_NAME}/lastBuild/testReport/)
                [**Coverage report**](${env.JENKINS_SERVER}/job/${env.JOB_NAME}/lastBuild/Coverage_20Report/)
            """,
            footer: "Merge test Duration: ${currentBuild.durationString.replace(' and counting', '')}",
            webhookURL: env.DISCORD_WEBHOOK_URL,
            result: status == "Build Success" ? 'SUCCESS' : 'FAILURE'
        )
    }
}
