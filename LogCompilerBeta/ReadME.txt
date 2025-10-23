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
	File Reading Approaches
	=============================================================
	1. Single Read Operation
	Suitable for: Files up to 200MB
	Method: Read entire file at once
	Use Case: Small to medium-sized files
		-----------------------------------
	2. Sequential Batch Processing
	Suitable for: Files up to 1GB
	Method: Read file in batches using asynchronous sequential processing
	Use Case: Large files requiring memory efficiency
		-----------------------------------
	3. Parallel Batch Processing
	Suitable for: Very large files
	Method: Read file in batches with synchronized parallel processing
	Advantage: Improved performance for big files
		-----------------------------------
	4. Multi-Channel Async Processing
	Suitable for: Extremely large files
	Method: Concurrent file reading and processing using multiple channels
	Advantage: Maximum throughput for massive files
	=============================================================
	=============================================================
	I have documented the performance benchmark results for small size log files in the benchmark.txt file.
