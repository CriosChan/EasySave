using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using EasyLog;
using EasySave.Log.Model;

namespace Server
{
    /// <summary>
    /// Main class for the UDP server that receives messages.
    /// </summary>
    public static class Server
    {
        // Logger used for logging messages
        private static AbstractLogger<LogEntry> _logger;  
        // UDP client for receiving messages
        private static UdpClient _udpServer;              

        /// <summary>
        /// Main entry point of the application.
        /// </summary>
        public static void Main(string[] args)
        {
            StartServer();  // Start the server

            // Check the log type to use, either XML or JSON
            string? logType = Environment.GetEnvironmentVariable("EasySaveLogType");
            if (logType == "xml")
            {
                _logger = new XmlLogger<LogEntry>("./logs/"); // Initialize XML logger
            }
            else
            {
                _logger = new JsonLogger<LogEntry>("./logs/"); // Initialize JSON logger
            }

            Console.WriteLine("Server is listening for UDP messages...");

            // Infinite loop to listen for client messages
            while (true)
            {
                ListenForClients();  // Listen for incoming messages
            }
        }

        /// <summary>
        /// Initializes the UDP server by binding to a specified port.
        /// </summary>
        private static void StartServer()
        {
            _udpServer = new UdpClient(5000); // Bind to port 5000
        }

        /// <summary>
        /// Listens for clients to receive UDP messages.
        /// </summary>
        private static void ListenForClients()
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0); // Remote endpoint for receiving messages
            byte[] buffer = new byte[1024]; // Buffer for storing received data

            try
            {
                // Receive asynchronously and get the remote endpoint info
                var receivedResult = _udpServer.Receive(ref remoteEndPoint);
                string data = Encoding.ASCII.GetString(receivedResult); // Convert received data to string
                
                // Deserialize the received data into a log entry
                LogEntry? entry = JsonSerializer.Deserialize<LogEntry>(data);
                if (entry != null)
                {
                    entry.ClientIPAddress = remoteEndPoint.Address.ToString(); // Add client IP address to log entry
                    string logMessage = FormatLogMessage(entry.ToString(), remoteEndPoint); // Format log message
                    Console.WriteLine(logMessage); // Display the message in the console

                    Log(entry); // Log the entry
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}"); // Handle exceptions and display the error
            }
        }

        /// <summary>
        /// Formats the log message with a timestamp and the client's IP address.
        /// </summary>
        /// <param name="message">The log message.</param>
        /// <param name="client">The client's endpoint.</param>
        /// <returns>Formatted message.</returns>
        private static string FormatLogMessage(string message, IPEndPoint client)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); // Get the timestamp
            string clientIp = client.Address.ToString(); // Get the client's IP address
            return $"[{timestamp}] [{clientIp}] {message}"; // Return the formatted message
        }

        /// <summary>
        /// Logs the provided message in the appropriate format.
        /// </summary>
        /// <param name="message">The log entry to log.</param>
        private static void Log(LogEntry message)
        {
            // Create the logs directory if it doesn't already exist
            if (!Directory.Exists("logs"))
            {
                Directory.CreateDirectory("logs");
            }
            _logger.Log(message); // Log the message using the logger
        }
    }
}
