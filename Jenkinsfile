pipeline {
    agent any

    stages {
        stage('Checkout') {
            steps {
                echo 'Checking out source code...'
                checkout scm
            }
        }

        stage('Build DB Service') {
            steps {
                echo 'Building MySQL Database service...'
                sh 'docker compose build todo-mysql'
            }
        }

        stage('Build Backend Service') {
            steps {
                echo 'Building C# .NET Core Backend service...'
                sh 'docker compose build backend'
            }
        }

        stage('Build Frontend Service') {
            steps {
                echo 'Building Angular Frontend service...'
                sh 'docker compose build frontend'
            }
        }

        stage('Orchestrate Stack') {
            steps {
                echo 'Starting all orchestrated containers...'
                sh 'docker compose up -d'
                sh 'docker compose ps'
            }
        }
    }

    post {
        always {
            echo 'CI/CD pipeline completed.'
        }
        success {
            echo 'Deployment successful!'
        }
        failure {
            echo 'Pipeline execution failed. Please verify build logs.'
        }
    }
}
