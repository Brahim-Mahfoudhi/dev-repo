pipeline {
    agent { label 'App' }
    options {
        buildDiscarder(logRotator(numToKeepStr: '10'))
    }

    environment {
        JENKINS_SERVER = 'http://139.162.132.174:8080' //NEEDS TO BE CHANGED
        DOTNET_PROJECT_PATH = 'Rise.Server/Rise.Server.csproj'
        DOTNET_TEST_PATH = 'Rise.Domain.Tests/Rise.Domain.Tests.csproj'
        PUBLISH_OUTPUT = 'publish'
        // DISCORD_WEBHOOK_URL = "https://discord.com/api/webhooks/1305826859665063936/xP1yD9MIf9vEwehqBE01c3AdIh-_62ZDrOzD0Zak5ti3Gm15gE8l3iWHBWMu_VzCmT_j" // NEEDS TO BE CHANGED
        JENKINS_CREDENTIALS_ID = "jenkins-master-key"
        SSH_KEY_FILE = '/var/lib/jenkins/.ssh/id_rsa'
        REMOTE_HOST = 'jenkins@139.162.148.79' // NEEDS TO BE CHANGED
        COVERAGE_REPORT_PATH = '/var/lib/jenkins/agent/workspace/dotnet_pipeline/coverage/coverage.cobertura.xml'
        COVERAGE_REPORT_DIR = '/var/lib/jenkins/agent/workspace/dotnet_pipeline/coverage-report/'
        TRX_FILE_PATH = 'dotnet-2425-tiao1/Rise.Domain.Tests/TestResults/test-results.trx'
        TEST_RESULT_PATH = 'Rise.Domain.Tests/TestResults'
        TRX_TO_XML_PATH = 'Rise.Domain.Tests/TestResults/test-results.xml'
        PUBLISH_DIR_PATH = '/var/lib/jenkins/artifacts/'
        M2MCLIENTID = credentials('M2MClientId') 
        M2MCLIENTSECRET = credentials('M2MClientSecret')
        BLAZORCLIENTID = credentials('BlazorClientId')
        BLAZORCLIENTSECRET = credentials('BlazorClientSecret')
        SQL_CONNECTION_STRING = credentials('SQLConnectionString')
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
                    git credentialsId: 'jenkins-master-key', url: 'git@github.com:HOGENT-RISE/dotnet-2425-tiao1.git', branch:'main'
                    echo 'Gather GitHub info!'
                    def gitInfo = sh(script: 'git show -s HEAD --pretty=format:"%an%n%ae%n%s%n%H%n%h" 2>/dev/null', returnStdout: true).trim().split("\n")
                    env.GIT_AUTHOR_NAME = gitInfo[0]
                    env.GIT_AUTHOR_EMAIL = gitInfo[1]
                    env.GIT_COMMIT_MESSAGE = gitInfo[2]
                    env.GIT_COMMIT = gitInfo[3]
                    env.GIT_BRANCH = gitInfo[4]
                }
            }
        }

 
       /*
        stage('Linting and Code Analysis') {
            steps {
                //TODO
            }
        }
        */

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
                sh 'mkdir -p /var/lib/jenkins/agent/workspace/dotnet_pipeline/coverage'
                sh "dotnet test ${DOTNET_TEST_PATH} --logger 'trx;LogFileName=test-results.trx' /p:CollectCoverage=true /p:CoverletOutput=${COVERAGE_REPORT_PATH} /p:CoverletOutputFormat=cobertura"
            }
        }
  
        stage('Coverage Report') {
            steps {
                echo 'Generating code coverage report...'
                script {
                    sh "/home/jenkins/.dotnet/tools/reportgenerator -reports:${COVERAGE_REPORT_PATH} -targetdir:${COVERAGE_REPORT_DIR} -reporttypes:Html"
                    publishHTML([allowMissing: false, alwaysLinkToLastBuild: false, keepAll: true, reportDir: COVERAGE_REPORT_DIR, reportFiles: 'index.html', reportName: 'Coverage Report'])
                }
            }
        }
    
        stage('Publish Application') {
            steps {
                sh "dotnet publish ${DOTNET_PROJECT_PATH} -c Release -o ${PUBLISH_OUTPUT}"
            }
        }

        stage('Deploy to Remote Server') {
            steps {
                sshagent([JENKINS_CREDENTIALS_ID]) {
                    script {
                        sh """
                            scp -i ${SSH_KEY_FILE} -o StrictHostKeyChecking=no -r ${PUBLISH_OUTPUT}/* ${REMOTE_HOST}:${PUBLISH_DIR_PATH}
                        """
                    }
                }
            }
        }
    }

    post {
        success {
            echo 'Build and deployment completed successfully!'
            archiveArtifacts artifacts: '**/*.dll', fingerprint: true
            archiveArtifacts artifacts: "${TRX_FILE_PATH}", fingerprint: true
            script {
                sendDiscordNotification("Build Success")
            }
        }
        failure {
            echo 'Build or deployment has failed.'
            script {
                sendDiscordNotification("Build Failed")
            }
        }
        always {
            echo 'Build process has completed.'
            echo 'Generate Test report...'
            sh "/home/jenkins/.dotnet/tools/trx2junit --output ${TEST_RESULT_PATH} ${TRX_FILE_PATH}"
            junit "${TRX_TO_XML_PATH}"
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
                
                [**Build output**](${JENKINS_SERVER}/job/${env.JOB_NAME}/${env.BUILD_NUMBER}/console)
                [**Test result**](${JENKINS_SERVER}/job/dotnet_pipeline/lastBuild/testReport/)
                [**Coverage report**](${JENKINS_SERVER}/job/dotnet_pipeline/lastBuild/Coverage_20Report/)
                [**History**](${JENKINS_SERVER}/job/dotnet_pipeline/${env.BUILD_NUMBER}/testReport/history/)
            """,
            footer: "Build Duration: ${currentBuild.durationString.replace(' and counting', '')}",
            webhookURL: DISCORD_WEBHOOK_URL,
            result: status == "Build Success" ? 'SUCCESS' : 'FAILURE'
        )
    }
}
