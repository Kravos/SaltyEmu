docker run -p 1433:1433 -e ACCEPT_EULA=Y -e SA_PASSWORD=strong_pass2018 --name saltyemu-database -d microsoft/mssql-server-linux:latest