using LogCompilerBeta.Models;
using LogCompilerBeta.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;
using Xunit.Abstractions;

public class PerformanceComparisonTest
{
    private readonly string filePath = "C:\\Assignment\\AVATAR3.messages.log";
    private readonly ContentReader _contentReader;
    private readonly ITestOutputHelper _output;

    public PerformanceComparisonTest(ITestOutputHelper output)
    {
        var loggerMock = new Mock<ILogger<ContentReader>>();
        _contentReader = new ContentReader(loggerMock.Object);
        _output = output;
    }

    [Fact]
    public async Task CompareAllMethodsPerformanceBatch50_000()
    {
        _output.WriteLine("=== PERFORMANCE COMPARISON ===");
        _output.WriteLine($"File: {filePath}");
        _output.WriteLine($"File Size: {GetFileSizeMB()} MB");
        _output.WriteLine("----------------------------------------");

        // Test 1: ReadAllAtOnceOptimizedAsync
        _output.WriteLine("1. Testing ReadAllAtOnceOptimizedAsync...");
        var stopwatch1 = Stopwatch.StartNew();
        var result1 = await _contentReader.ReadAllAtOnceOptimizedAsync(filePath);
        stopwatch1.Stop();
        _output.WriteLine($"✓ Completed in: {stopwatch1.ElapsedMilliseconds} ms");
        _output.WriteLine($"  Messages found: {result1.RejectMessages.Count + result1.ExecutionReportMessages.Count}");
        _output.WriteLine($"  Memory usage: ~{EstimateMemoryUsage(result1):N0} MB");

        // Test 2: ReadInBatchesAsync
        _output.WriteLine("2. Testing ReadInBatchesAsync...");
        var stopwatch2 = Stopwatch.StartNew();
        var result2 = await _contentReader.ReadInBatchesAsync(filePath, 50_000);
        stopwatch2.Stop();
        _output.WriteLine($"✓ Completed in: {stopwatch2.ElapsedMilliseconds} ms");
        _output.WriteLine($"  Messages found: {result2.RejectMessages.Count + result2.ExecutionReportMessages.Count}");
        _output.WriteLine($"  Memory usage: ~{EstimateMemoryUsage(result2):N0} MB");

        // Test 3: ReadInBatchesParallelAsync
        _output.WriteLine("3. Testing ReadInBatchesParallelAsync...");
        var stopwatch3 = Stopwatch.StartNew();
        var result3 = await _contentReader.ReadInBatchesParallelAsync(filePath, 50_000);
        stopwatch3.Stop();
        _output.WriteLine($"✓ Completed in: {stopwatch3.ElapsedMilliseconds} ms");
        _output.WriteLine($"  Messages found: {result3.RejectMessages.Count + result3.ExecutionReportMessages.Count}");
        _output.WriteLine($"  Memory usage: ~{EstimateMemoryUsage(result3):N0} MB");
        

        // Test 4: ReadWithChannelsAsync
        _output.WriteLine("4. Testing ReadWithChannelsAsync...");
        var stopwatch4 = Stopwatch.StartNew();
        var result4 = await _contentReader.ReadWithChannelsAsync(filePath, 50_000, 4);
        stopwatch4.Stop();
        _output.WriteLine($"✓ Completed in: {stopwatch4.ElapsedMilliseconds} ms");
        _output.WriteLine($"  Messages found: {result4.RejectMessages.Count + result4.ExecutionReportMessages.Count}");
        _output.WriteLine($"  Memory usage: ~{EstimateMemoryUsage(result4):N0} MB");
        

        // Summary
        _output.WriteLine("=== PERFORMANCE SUMMARY ===");
        _output.WriteLine($"ReadAllAtOnceOptimizedAsync: {stopwatch1.ElapsedMilliseconds} ms");
        _output.WriteLine($"ReadInBatchesAsync:         {stopwatch2.ElapsedMilliseconds} ms");
        _output.WriteLine($"ReadInBatchesParallelAsync: {stopwatch3.ElapsedMilliseconds} ms");
        _output.WriteLine($"ReadWithChannelsAsync:      {stopwatch4.ElapsedMilliseconds} ms");
        

        // Calculate improvements
        var baseline = stopwatch1.ElapsedMilliseconds;
        _output.WriteLine($"Improvement over baseline:");
        _output.WriteLine($"ReadInBatchesAsync:         {CalculateImprovement(baseline, stopwatch2.ElapsedMilliseconds)}");
        _output.WriteLine($"ReadInBatchesParallelAsync: {CalculateImprovement(baseline, stopwatch3.ElapsedMilliseconds)}");
        _output.WriteLine($"ReadWithChannelsAsync:      {CalculateImprovement(baseline, stopwatch4.ElapsedMilliseconds)}");
    }

    [Fact]
    public async Task CompareAllMethodsPerformanceBatch250_000Channel4()
    {
        _output.WriteLine("=== PERFORMANCE COMPARISON ===");
        _output.WriteLine($"File: {filePath}");
        _output.WriteLine($"File Size: {GetFileSizeMB()} MB");
        _output.WriteLine("----------------------------------------");

        // Test 1: ReadAllAtOnceOptimizedAsync
        _output.WriteLine("1. Testing ReadAllAtOnceOptimizedAsync...");
        var stopwatch1 = Stopwatch.StartNew();
        var result1 = await _contentReader.ReadAllAtOnceOptimizedAsync(filePath);
        stopwatch1.Stop();
        _output.WriteLine($"✓ Completed in: {stopwatch1.ElapsedMilliseconds} ms");
        _output.WriteLine($"  Messages found: {result1.RejectMessages.Count + result1.ExecutionReportMessages.Count}");
        _output.WriteLine($"  Memory usage: ~{EstimateMemoryUsage(result1):N0} MB");

        // Test 2: ReadInBatchesAsync
        _output.WriteLine("2. Testing ReadInBatchesAsync...");
        var stopwatch2 = Stopwatch.StartNew();
        var result2 = await _contentReader.ReadInBatchesAsync(filePath, 250_000);
        stopwatch2.Stop();
        _output.WriteLine($"✓ Completed in: {stopwatch2.ElapsedMilliseconds} ms");
        _output.WriteLine($"  Messages found: {result2.RejectMessages.Count + result2.ExecutionReportMessages.Count}");
        _output.WriteLine($"  Memory usage: ~{EstimateMemoryUsage(result2):N0} MB");

        // Test 3: ReadInBatchesParallelAsync
        _output.WriteLine("3. Testing ReadInBatchesParallelAsync...");
        var stopwatch3 = Stopwatch.StartNew();
        var result3 = await _contentReader.ReadInBatchesParallelAsync(filePath, 250_000);
        stopwatch3.Stop();
        _output.WriteLine($"✓ Completed in: {stopwatch3.ElapsedMilliseconds} ms");
        _output.WriteLine($"  Messages found: {result3.RejectMessages.Count + result3.ExecutionReportMessages.Count}");
        _output.WriteLine($"  Memory usage: ~{EstimateMemoryUsage(result3):N0} MB");


        // Test 4: ReadWithChannelsAsync
        _output.WriteLine("4. Testing ReadWithChannelsAsync...");
        var stopwatch4 = Stopwatch.StartNew();
        var result4 = await _contentReader.ReadWithChannelsAsync(filePath, 250_000, 4);
        stopwatch4.Stop();
        _output.WriteLine($"✓ Completed in: {stopwatch4.ElapsedMilliseconds} ms");
        _output.WriteLine($"  Messages found: {result4.RejectMessages.Count + result4.ExecutionReportMessages.Count}");
        _output.WriteLine($"  Memory usage: ~{EstimateMemoryUsage(result4):N0} MB");


        // Summary
        _output.WriteLine("=== PERFORMANCE SUMMARY ===");
        _output.WriteLine($"ReadAllAtOnceOptimizedAsync: {stopwatch1.ElapsedMilliseconds} ms");
        _output.WriteLine($"ReadInBatchesAsync:         {stopwatch2.ElapsedMilliseconds} ms");
        _output.WriteLine($"ReadInBatchesParallelAsync: {stopwatch3.ElapsedMilliseconds} ms");
        _output.WriteLine($"ReadWithChannelsAsync:      {stopwatch4.ElapsedMilliseconds} ms");


        // Calculate improvements
        var baseline = stopwatch1.ElapsedMilliseconds;
        _output.WriteLine($"Improvement over baseline:");
        _output.WriteLine($"ReadInBatchesAsync:         {CalculateImprovement(baseline, stopwatch2.ElapsedMilliseconds)}");
        _output.WriteLine($"ReadInBatchesParallelAsync: {CalculateImprovement(baseline, stopwatch3.ElapsedMilliseconds)}");
        _output.WriteLine($"ReadWithChannelsAsync:      {CalculateImprovement(baseline, stopwatch4.ElapsedMilliseconds)}");
    }

    [Fact]
    public async Task CompareAllMethodsPerformanceBatch250_000Channel8()
    {
        _output.WriteLine("=== PERFORMANCE COMPARISON ===");
        _output.WriteLine($"File: {filePath}");
        _output.WriteLine($"File Size: {GetFileSizeMB()} MB");
        _output.WriteLine("----------------------------------------");

        // Test 1: ReadAllAtOnceOptimizedAsync
        _output.WriteLine("1. Testing ReadAllAtOnceOptimizedAsync...");
        var stopwatch1 = Stopwatch.StartNew();
        var result1 = await _contentReader.ReadAllAtOnceOptimizedAsync(filePath);
        stopwatch1.Stop();
        _output.WriteLine($"✓ Completed in: {stopwatch1.ElapsedMilliseconds} ms");
        _output.WriteLine($"  Messages found: {result1.RejectMessages.Count + result1.ExecutionReportMessages.Count}");
        _output.WriteLine($"  Memory usage: ~{EstimateMemoryUsage(result1):N0} MB");

        // Test 2: ReadInBatchesAsync
        _output.WriteLine("2. Testing ReadInBatchesAsync...");
        var stopwatch2 = Stopwatch.StartNew();
        var result2 = await _contentReader.ReadInBatchesAsync(filePath, 250_000);
        stopwatch2.Stop();
        _output.WriteLine($"✓ Completed in: {stopwatch2.ElapsedMilliseconds} ms");
        _output.WriteLine($"  Messages found: {result2.RejectMessages.Count + result2.ExecutionReportMessages.Count}");
        _output.WriteLine($"  Memory usage: ~{EstimateMemoryUsage(result2):N0} MB");

        // Test 3: ReadInBatchesParallelAsync
        _output.WriteLine("3. Testing ReadInBatchesParallelAsync...");
        var stopwatch3 = Stopwatch.StartNew();
        var result3 = await _contentReader.ReadInBatchesParallelAsync(filePath, 250_000);
        stopwatch3.Stop();
        _output.WriteLine($"✓ Completed in: {stopwatch3.ElapsedMilliseconds} ms");
        _output.WriteLine($"  Messages found: {result3.RejectMessages.Count + result3.ExecutionReportMessages.Count}");
        _output.WriteLine($"  Memory usage: ~{EstimateMemoryUsage(result3):N0} MB");


        // Test 4: ReadWithChannelsAsync
        _output.WriteLine("4. Testing ReadWithChannelsAsync...");
        var stopwatch4 = Stopwatch.StartNew();
        var result4 = await _contentReader.ReadWithChannelsAsync(filePath, 250_000, 8);
        stopwatch4.Stop();
        _output.WriteLine($"✓ Completed in: {stopwatch4.ElapsedMilliseconds} ms");
        _output.WriteLine($"  Messages found: {result4.RejectMessages.Count + result4.ExecutionReportMessages.Count}");
        _output.WriteLine($"  Memory usage: ~{EstimateMemoryUsage(result4):N0} MB");


        // Summary
        _output.WriteLine("=== PERFORMANCE SUMMARY ===");
        _output.WriteLine($"ReadAllAtOnceOptimizedAsync: {stopwatch1.ElapsedMilliseconds} ms");
        _output.WriteLine($"ReadInBatchesAsync:         {stopwatch2.ElapsedMilliseconds} ms");
        _output.WriteLine($"ReadInBatchesParallelAsync: {stopwatch3.ElapsedMilliseconds} ms");
        _output.WriteLine($"ReadWithChannelsAsync:      {stopwatch4.ElapsedMilliseconds} ms");


        // Calculate improvements
        var baseline = stopwatch1.ElapsedMilliseconds;
        _output.WriteLine($"Improvement over baseline:");
        _output.WriteLine($"ReadInBatchesAsync:         {CalculateImprovement(baseline, stopwatch2.ElapsedMilliseconds)}");
        _output.WriteLine($"ReadInBatchesParallelAsync: {CalculateImprovement(baseline, stopwatch3.ElapsedMilliseconds)}");
        _output.WriteLine($"ReadWithChannelsAsync:      {CalculateImprovement(baseline, stopwatch4.ElapsedMilliseconds)}");
    }

    [Fact]
    public async Task CompareMethodsWithDifferentBatchSizes()
    {
        _output.WriteLine("=== BATCH SIZE COMPARISON ===");

        var batchSizes = new[] { 10_000, 50_000, 100_000, 250_000 };

        foreach (var batchSize in batchSizes)
        {
            _output.WriteLine($"\nBatch Size: {batchSize:N0}");
            _output.WriteLine("----------------------------");

            // Batches Async
            var stopwatch1 = Stopwatch.StartNew();
            var result1 = await _contentReader.ReadInBatchesAsync(filePath, batchSize);
            stopwatch1.Stop();
            _output.WriteLine($"ReadInBatchesAsync: {stopwatch1.ElapsedMilliseconds} ms");

            // Parallel Batches
            var stopwatch2 = Stopwatch.StartNew();
            var result2 = await _contentReader.ReadInBatchesParallelAsync(filePath, batchSize);
            stopwatch2.Stop();
            _output.WriteLine($"ReadInBatchesParallelAsync: {stopwatch2.ElapsedMilliseconds} ms");

            // Channels
            var stopwatch3 = Stopwatch.StartNew();
            var result3 = await _contentReader.ReadWithChannelsAsync(filePath, batchSize, 4);
            stopwatch3.Stop();
            _output.WriteLine($"ReadWithChannelsAsync: {stopwatch3.ElapsedMilliseconds} ms");
        }
    }

    [Fact]
    public async Task CompareChannelsWithDifferentParallelism()
    {
        _output.WriteLine("=== CHANNELS PARALLELISM COMPARISON ===");

        var parallelismLevels = new[] { 2, 4, 8, Environment.ProcessorCount };

        foreach (var parallelism in parallelismLevels)
        {
            _output.WriteLine($"\nParallelism: {parallelism} threads");
            _output.WriteLine("----------------------------");

            var stopwatch = Stopwatch.StartNew();
            var result = await _contentReader.ReadWithChannelsAsync(filePath, 50_000, parallelism);
            stopwatch.Stop();

            _output.WriteLine($"Execution Time: {stopwatch.ElapsedMilliseconds} ms");
            _output.WriteLine($"Messages Processed: {result.RejectMessages.Count + result.ExecutionReportMessages.Count}");
        }
    }

    [Fact]
    public async Task WarmUpAndMultipleRuns()
    {
        _output.WriteLine("=== WARM UP AND AVERAGE TIMES ===");

        // Warm up - first run is usually slower
        _output.WriteLine("Warming up...");
        await _contentReader.ReadAllAtOnceOptimizedAsync(filePath);

        var runs = 3;
        var times = new List<long>();

        _output.WriteLine($"Running {runs} iterations...");

        for (int i = 0; i < runs; i++)
        {
            _output.WriteLine($"Run {i + 1}:");

            var stopwatch1 = Stopwatch.StartNew();
            var result1 = await _contentReader.ReadAllAtOnceOptimizedAsync(filePath);
            stopwatch1.Stop();
            _output.WriteLine($"  ReadAllAtOnce: {stopwatch1.ElapsedMilliseconds} ms");

            var stopwatch2 = Stopwatch.StartNew();
            var result2 = await _contentReader.ReadInBatchesParallelAsync(filePath, 50_000);
            stopwatch2.Stop();
            _output.WriteLine($"  ParallelBatches: {stopwatch2.ElapsedMilliseconds} ms");

            var stopwatch3 = Stopwatch.StartNew();
            var result3 = await _contentReader.ReadWithChannelsAsync(filePath, 50_000, 4);
            stopwatch3.Stop();
            _output.WriteLine($"  Channels: {stopwatch3.ElapsedMilliseconds} ms");

            times.Add(stopwatch1.ElapsedMilliseconds);
        }

        _output.WriteLine($"\nAverage ReadAllAtOnce time: {times.Average():N0} ms");
    }

    private double GetFileSizeMB()
    {
        var fileInfo = new FileInfo(filePath);
        return fileInfo.Exists ? fileInfo.Length / (1024.0 * 1024.0) : 0;
    }

    private double EstimateMemoryUsage(FixMessageResult result)
    {
        // Rough estimation: each string ~100 bytes + overhead
        var totalMessages = result.RejectMessages.Count + result.ExecutionReportMessages.Count;
        return (totalMessages * 100) / (1024.0 * 1024.0);
    }

    private string CalculateImprovement(long baseline, long current)
    {
        var improvement = ((double)baseline - current) / baseline * 100;
        var sign = improvement >= 0 ? "+" : "-";
        return $"{sign}{Math.Abs(improvement):F1}%";
    }
}