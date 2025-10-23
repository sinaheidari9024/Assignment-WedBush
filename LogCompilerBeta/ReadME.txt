=============================================================
Data Storage Implementation
=============================================================
***I have implemented two data storage approaches:
SQL Database: Primary implementation with persistent storage (as you need in assignment)
In-Memory: Development-friendly alternative (no setup required)

***Default Configuration:
Development: Uses In-Memory Repository (no prerequisites)
Production: Uses SQL Repository

***How can switch to SQL in Development:
To use SQL database locally:
Edit appsettings.Development.json
Set "UseInMemoryRepository": false in RepositorySettings
Create database: FixMessagesDb
Run update-database in LogCompilerBeta project


=============================================================
Application Startup Guide
=============================================================
***Prerequisites
Copy a log file named AVATAR3.messages.log to the path: C:\Assignment\

***Backend Setup
Set LogCompilerBeta as the startup project
Run the project in Visual Studio or your preferred code editor

***Frontend Setup
Open command line and navigate to: logcompiler.client folder
Execute the command: npm run dev
Open your browser and navigate to: http://localhost:5173/


=============================================================
Read File with different approach
=============================================================