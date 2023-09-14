namespace RabbitMQServer.Services
{
    // This class implements recording of logs to the corresponding files
    public class Logger
    {
        private string LogFilePath { get; }

        public Logger(string logFilePath)
        {
            LogFilePath = logFilePath;
        }

        public void LogInfo(string message)
        {
            LogMessage($"[INFO] {message}");
        }

        public void LogError(string message)
        {
            LogMessage($"[ERROR] {message}");
        }

        private void LogMessage(string message)
        {
            Console.WriteLine(message);

            try
            {
                using (StreamWriter writer = new StreamWriter(LogFilePath, true)) // writes a message to the end of "LogFilePath" file (by using true parameter)
                {
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while logging: {ex.Message}");
            }
        }

        public void ClearFileContent()
        {
            try
            {
                File.WriteAllText(LogFilePath, string.Empty);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing file content: {ex.Message}");
            }
        }
    }
}
