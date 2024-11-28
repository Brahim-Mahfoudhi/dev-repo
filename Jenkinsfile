stage('Find PR Number') {
    steps {
        script {
            // Only attempt to get PR number if not passed manually
            if (!env.PR_NUMBER) {
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
                    
                    echo "GitHub API Response: ${response}"
                    def prNumber = ""
                    if (response) {
                        def jsonResponse = readJSON text: response.content
                        if (jsonResponse.size() > 0) {
                            prNumber = jsonResponse[0].number
                            echo "Found PR Number: ${prNumber}"
                        } else {
                            echo "No PR found for commit ${commitSHA}."
                        }
                    }
                    env.PR_NUMBER = prNumber
                }
            }
            
            if (!env.PR_NUMBER) {
                error "PR_NUMBER is not set. Please ensure the PR number is provided."
            }
        }
    }
}
