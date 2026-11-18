pipeline {
    agent any

    environment {
        PROJECT     = 'Booking.csproj'
        IIS_PATH    = 'E:\\Hosting\\MedicoTest'
        PUBLISH_DIR = "${WORKSPACE}_publish"
    }

    stages {
        stage('Check .NET SDK') {
            steps {
                bat 'dotnet --info'
                bat 'dotnet --list-sdks'
            }
        }

        stage('Clean Previous Publish') {
            steps {
                bat '''
                    if exist "%PUBLISH_DIR%" rmdir /s /q "%PUBLISH_DIR%"
                '''
            }
        }

        stage('Restore') {
            steps {
                bat 'dotnet restore "%PROJECT%"'
            }
        }

        stage('Build') {
            steps {
                bat 'dotnet build "%PROJECT%" --configuration Release --no-restore'
            }
        }

        stage('Publish') {
            steps {
                bat 'dotnet publish "%PROJECT%" --configuration Release --no-build --output "%PUBLISH_DIR%"'
            }
        }

        stage('Deploy to IIS') {
            steps {
                bat '''
                    echo Updating... > "%IIS_PATH%\\app_offline.htm"
                    ping -n 4 127.0.0.1 >nul

                    robocopy "%PUBLISH_DIR%" "%IIS_PATH%" /E /XF app_offline.htm appsettings.Production.json web.config /XD logs uploads

                    if %ERRORLEVEL% GEQ 8 exit /b 1
                    exit /b 0
                '''
            }
        }
    }

    post {
        always {
            bat 'if exist "%IIS_PATH%\\app_offline.htm" del "%IIS_PATH%\\app_offline.htm"'
        }
    }
}
