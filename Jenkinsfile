pipeline {
    agent { label 'App' }
    options {
        buildDiscarder(logRotator(numToKeepStr: '10'))
    }

    environment {
        JENKINS_SERVER = 'http://172.16.128.100:8080' //NEEDS TO BE CHANGED
        DOTNET_PROJECT_PATH = 'Rise.Server/Rise.Server.csproj'
        DOTNET_TEST_PATH = 'Rise.Domain.Tests/Rise.Domain.Tests.csproj'
        PUBLISH_OUTPUT = 'publish'
        DOTNET_ENVIRONMENT = 'Production'
        DISCORD_WEBHOOK_URL = "https://discord.com/api/webhooks/1301160382307766292/kROxjtgZ-XVOibckTMri2fy5-nNOEjzjPLbT9jEpr_R0UH9JG0ZXb2XzUsYGE0d3yk6I"
        JENKINS_CREDENTIALS_ID = "jenkins-master-key"
        SSH_KEY_FILE = '/var/lib/jenkins/.ssh/id_rsa'
        REMOTE_HOST = 'jenkins@172.16.128.101' // NEEDS TO BE CHANGED
        TRX_FILE_PATH = 'Rise.Domain.Tests/TestResults/test-results.trx'
        TEST_RESULT_PATH = 'Rise.Domain.Tests/TestResults'
        TRX_TO_XML_PATH = 'Rise.Domain.Tests/TestResults/test-results.xml'
        PUBLISH_DIR_PATH = '/var/lib/jenkins/artifacts/'
       // M2MCLIENTID = credentials('M2MClientId') 
       // M2MCLIENTSECRET = credentials('M2MClientSecret')
       // BLAZORCLIENTID = credentials('BlazorClientId')
       // BLAZORCLIENTSECRET = credentials('BlazorClientSecret')
       // SQL_CONNECTION_STRING = credentials('SQLConnectionString')
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

    
        stage('Publish Application') {
            steps {
                sh "dotnet publish ${DOTNET_PROJECT_PATH} -c Release -o ${PUBLISH_OUTPUT}"
            }
        }

        stage('Deploy to Remote Server') {
            steps {
                withCredentials([
                    string(credentialsId: 'M2MClientId', variable: 'M2MCLIENTID'),
                    string(credentialsId: 'M2MClientSecret', variable: 'M2MCLIENTSECRET'),
                    string(credentialsId: 'BlazorClientId', variable: 'BLAZORCLIENTID'),
                    string(credentialsId: 'BlazorClientSecret', variable: 'BLAZORCLIENTSECRET'),
                    string(credentialsId: 'SQLConnectionString', variable: 'SQL_CONNECTION_STRING')
                ]) {
                    sshagent([JENKINS_CREDENTIALS_ID]) {
                        script {
                            def remoteScript = "/tmp/deploy_script.sh"
                            def publishDir = "${PUBLISH_DIR_PATH}"                            
                            sh """
                                echo '#!/bin/bash
                                export M2MCLIENTID="${M2MCLIENTID}"
                                export M2MCLIENTSECRET="${M2MCLIENTSECRET}"
                                export BLAZORCLIENTID="${BLAZORCLIENTID}"
                                export BLAZORCLIENTSECRET="${BLAZORCLIENTSECRET}"
                                export SQL_CONNECTION_STRING="${SQL_CONNECTION_STRING}"
        
                                sed -i "s|\\\\"ConnectionStrings\\": {}|\\\\"ConnectionStrings\\": {\\\\"SqlServer\\": \\\\"Server=\${SQL_CONNECTION_STRING};TrustServerCertificate=True;\\\\"}|g" ${publishDir}/appsettings.json
                                sed -i "s|\\\\"ConnectionStrings\\": {}|\\\\"ConnectionStrings\\": {\\\\"SqlServer\\": \\\\"Server=\${SQL_CONNECTION_STRING};TrustServerCertificate=True;\\\\"}|g" ${publishDir}/appsettings.Development.json
                                sed -i "s|\\\\"Auth0\\": {}|\\\\"Auth0\\": {\\\\"Authority\\": \\\\"https://dev-6yunsksn11owe71c.us.auth0.com/\\\\", \\\\"Audience\\": \\\\"https://api.rise.buut.com/\\\\", \\\\"M2MClientId\\": \\\\"\${M2MCLIENTID}\\", \\\\"M2MClientSecret\\": \\\\"\${M2MCLIENTSECRET}\\", \\\\"BlazorClientId\\": \\\\"\${BLAZORCLIENTID}\\", \\\\"BlazorClientSecret\\": \\\\"\${BLAZORCLIENTSECRET}\\\\"}|g" ${publishDir}/appsettings.json
                                sed -i "s|\\\\"Logging\\": {}|\\\\"Logging\\": {\\\\"LogLevel\\": {\\\\"Default\\": \\\\"Information\\\\", \\\\"Microsoft.AspNetCore\\": \\\\"Warning\\\\"}}|g" ${publishDir}/appsettings.json
                                ' > ${remoteScript}
                            """
                            
                            sh """
                                scp -i ${SSH_KEY_FILE} -o StrictHostKeyChecking=no ${remoteScript} ${REMOTE_HOST}:${remoteScript}
                            """
                            
                            sh """
                                ssh -i ${SSH_KEY_FILE} -o StrictHostKeyChecking=no ${REMOTE_HOST} "bash ${remoteScript} && rm ${remoteScript}"
                            """
                        }
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
                [**Test result**](${JENKINS_SERVER}/job/${env.JOB_NAME}/lastBuild/testReport/)
                [**Coverage report**](${JENKINS_SERVER}/job/${env.JOB_NAME}/lastBuild/Coverage_20Report/)
                [**History**](${JENKINS_SERVER}/job/${env.JOB_NAME}/${env.BUILD_NUMBER}/testReport/history/)
            """,
            footer: "Build Duration: ${currentBuild.durationString.replace(' and counting', '')}",
            webhookURL: DISCORD_WEBHOOK_URL,
            result: status == "Build Success" ? 'SUCCESS' : 'FAILURE'
        )
    }
}
