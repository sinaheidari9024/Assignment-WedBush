# Log Compiler Beta

## 🗄️ Data Storage Implementation

### **Storage Options**

I have implemented two data storage approaches:

- **SQL Database** 🗃️
  - Primary implementation with persistent storage (as required in the assignment)
  - Production-ready with full data persistence

- **In-Memory Storage** 🧠
  - Development-friendly alternative (no setup required)
  - Perfect for testing and rapid development

### **Default Configuration**

| Environment | Storage Type | Prerequisites |
|-------------|--------------|---------------|
| **Development** | In-Memory Repository | No setup required |
| **Production** | SQL Repository | SQL Server required |

### **Switching to SQL in Development**

To use SQL database locally:

1. **Edit the development configuration file**
2. **Locate the RepositorySettings section** in the configuration file
3. **Set the UseInMemoryRepository value to false** to disable in-memory storage
4. **Create the application database** with the specified name
5. **Run the database migration command** within the main project

---

## 🚀 Application Startup Guide

### **Prerequisites**

- Copy a log file named **AVATAR3.messages.log** to the path: C:\Assignment\ 
- The file is located in **solution item folder**
- 
### **Backend Setup**

1. Set the main project as the startup project in your solution
2. Run the project in Visual Studio or your preferred code editor

### **Frontend Setup**

1. Open command line and navigate to the **logcompiler.client folder** under the **logcompiler.client project**
2. Execute the development server command to start the frontend: **npm run dev**
3. Open your browser and navigate to theis URL:  **http://localhost:5173/**

---

## 📁 File Reading Approaches

### **1. Single Read Operation** ⚡
- **Suitable for:** Files up to **200MB**
- **Method:** Read entire file at once
- **Use Case:** Small to medium-sized files

---

### **2. Sequential Batch Processing** 🔄
- **Suitable for:** Files up to **1GB**
- **Method:** Read file in batches using asynchronous sequential processing
- **Use Case:** Large files requiring memory efficiency

---

### **3. Parallel Batch Processing** 🚀
- **Suitable for:** Very large files
- **Method:** Read file in batches with synchronized parallel processing
- **Advantage:** Improved performance for big files

---

### **4. Multi-Channel Async Processing** 🌊
- **Suitable for:** Extremely large files
- **Method:** Concurrent file reading and processing using multiple channels
- **Advantage:** Maximum throughput for massive files

---

## 📊 Performance Benchmark

I have documented the performance benchmark results for small size log files in the benchmark.txt file.
