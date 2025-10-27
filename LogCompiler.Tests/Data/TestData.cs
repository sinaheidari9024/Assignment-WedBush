namespace LogCompiler.Tests.Data
{
    public static class TestData
    {
        public static void CreateLargeTestFile(string filePath, long targetSizeInBytes)
        {
            using var writer = new StreamWriter(filePath);
            var baseLine = "2023-12-01 10:00:00.000 8=FIX.4.2|9=100|35=8|49=SENDER|56=TARGET|34=1|52=20231201-10:00:00|10=001";
            var lineSize = System.Text.Encoding.UTF8.GetByteCount(baseLine + Environment.NewLine);

            var linesNeeded = (int)(targetSizeInBytes / lineSize) + 1;

            for (int i = 0; i < linesNeeded; i++)
            {
                string messageType;
                if (i % 10000 == 0) // Every 10000th line is a reject
                {
                    messageType = "35=3"; // Reject
                }
                else
                {
                    messageType = "35=8"; // Execution Report
                }

                var line = $"2023-12-01 10:00:00.000 8=FIX.4.2|9=100|{messageType}|49=SENDER|56=TARGET|34={i + 1}|52=20231201-10:00:00|10=001";
                writer.WriteLine(line);
            }
        }

        public static void CreateLargeTestFilePrecise(string filePath, long targetSizeInBytes)
        {
            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            using var writer = new StreamWriter(fileStream);

            var random = new Random();
            long currentSize = 0;
            int lineNumber = 1;

            while (currentSize < targetSizeInBytes)
            {
                string messageType = random.Next(0, 10000) == 0 ? "35=3" : "35=8";

                var line = $"2023-12-01 10:00:00.000 8=FIX.4.2|9=100|{messageType}|49=SENDER|56=TARGET|34={lineNumber}|52=20231201-10:00:00|10=001";
                writer.WriteLine(line);

                currentSize += System.Text.Encoding.UTF8.GetByteCount(line + Environment.NewLine);
                lineNumber++;
            }
        }

        public static List<string> GenerateSampleLines(int count)
        {
            var lines = new List<string>();
            var random = new Random();

            for (int i = 0; i < count; i++)
            {
                string messageType = random.Next(0, 10000) == 0 ? "35=3" : "35=8";

                var line = $"2023-12-01 10:00:00.000 8=FIX.4.2|9=100|{messageType}|49=SENDER|56=TARGET|34={i + 1}|52=20231201-10:00:00|10=001";
                lines.Add(line);
            }

            return lines;
        }

        public static List<string> GenerateSampleLinesWithSpecificRatio(int totalCount, int rejectCount)
        {
            var lines = new List<string>();
            var random = new Random();

            for (int i = 0; i < totalCount; i++)
            {
                string messageType = i < rejectCount ? "35=3" : "35=8";
                var line = $"2023-12-01 10:00:00.000 8=FIX.4.2|9=100|{messageType}|49=SENDER|56=TARGET|34={i + 1}|52=20231201-10:00:00|10=001";
                lines.Add(line);
            }

            return lines.OrderBy(x => random.Next()).ToList();
        }
    }
}
