pipeline {

    agent any

    environment {
        IMAGE = "todo-backend:${BUILD_NUMBER}"
        NETWORK = "app-net"
        MYSQL_CONT = "todo-mysql"
        API_CONT = "todo-backend"
        MYSQL_PWD = "root"
        MYSQL_DB = "tododb"
    }

    stages {

        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Build Docker Image') {
            steps {
                bat "docker build -t %IMAGE% ."
            }
        }

        stage('Start MySQL') {
            steps {
                bat """
                docker network create %NETWORK% 2>nul

                docker rm -f %MYSQL_CONT% 2>nul

                docker run -d --name %MYSQL_CONT% --network %NETWORK% --label com.docker.compose.project=todoapp ^
                    -e MYSQL_ROOT_PASSWORD=%MYSQL_PWD% ^
                    -e MYSQL_DATABASE=%MYSQL_DB% ^
                    -p 3306:3306 ^
                    mysql:8.0
                """
            }
        }

        stage('Wait for MySQL') {
            steps {
                bat """
                echo Waiting for MySQL to be ready...

                for /L %%i in (1,1,30) do (
                    docker exec %MYSQL_CONT% mysqladmin ping -h "localhost" -u root -p%MYSQL_PWD% --silent
                    if not errorlevel 1 (
                        echo MySQL is ready!
                        goto :ready
                    )
                    ping 127.0.0.1 -n 2 > nul
                )

                echo MySQL not ready in time!
                exit /b 1

                :ready
                """
            }
        }

        stage('Run API') {
            steps {
                bat """
                docker rm -f %API_CONT% 2>nul

                docker run -d --name %API_CONT% --network %NETWORK% --label com.docker.compose.project=todoapp ^
                    -e "ConnectionStrings__DefaultConnection=Server=%MYSQL_CONT%;Database=%MYSQL_DB%;User=root;Password=%MYSQL_PWD%;" ^
                    -e "Jwt__Key=SuperSecretKeyForTodoApp123456!!!PleaseChangeMeInProduction" ^
                    -e "Jwt__Issuer=todoapp-backend" ^
                    -e "Jwt__Audience=todoapp-frontend" ^
                    -p 5076:8080 ^
                    %IMAGE%
                """
            }
        }
    }
}
