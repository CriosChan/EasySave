using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using EasyLog;
using EasySave.Log.Model;

namespace EasySave.Server;

/// <summary>
///     Main class for the TCP server that receives messages.
/// </summary>
public static class Server
{
    // Logger used for logging messages
    private static AbstractLogger<LogEntry> _logger;

    // TCP listener for receiving messages
    private static TcpListener _tcpServer;

    /// <summary>
    ///     Main entry point of the application.
    /// </summary>
    public static void Main(string[] args)
    {
        StartServer(); // Start the server

        // Check the log type to use, either XML or JSON
        var logType = Environment.GetEnvironmentVariable("EasySaveLogType");
        if (logType == "xml")
            _logger = new XmlLogger<LogEntry>("./logs/"); // Initialize XML logger
        else
            _logger = new JsonLogger<LogEntry>("./logs/"); // Initialize JSON logger

        Console.WriteLine("Server is listening for TCP messages...");

        // Infinite loop to listen for client messages
        while (true)
        {
            ListenForClients(); // Listen for incoming messages
        }
    }

    /// <summary>
    ///     Initializes the TCP server by binding to a specified port.
    /// </summary>
    private static void StartServer()
    {
        _tcpServer = new TcpListener(IPAddress.Any, 5000); // Bind to port 5000
        _tcpServer.Start(); // Start listening for TCP connections
    }

    /// <summary>
    ///     Listens for clients to receive TCP messages.
    /// </summary>
    private static void ListenForClients()
    {
        try
        {
            using var client = _tcpServer.AcceptTcpClient(); // Accept incoming TCP client connection
            using var networkStream = client.GetStream(); // Get the network stream for the client

            var lengthBuffer = new byte[4]; // Buffer to read the length of the incoming message

            try
            {
                // Read the first 4 bytes to get the length of the incoming data
                int bytesRead = networkStream.Read(lengthBuffer, 0, lengthBuffer.Length);
                if (bytesRead == 4) // Make sure we read the correct amount
                {
                    int dataLength = BitConverter.ToInt32(lengthBuffer, 0); // Convert bytes to integer

                    var dataBuffer = new byte[dataLength]; // Create a buffer for the incoming data
                    bytesRead = networkStream.Read(dataBuffer, 0, dataLength); // Read the actual data

                    var data = Encoding.ASCII.GetString(dataBuffer, 0, bytesRead); // Convert received data to string

                    // Deserialize the received data into a log entry
                    var entry = JsonSerializer.Deserialize<LogEntry>(data);
                    if (entry != null)
                    {
                        entry.ClientIPAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString(); // Add client IP address to log entry
                        var logMessage = FormatLogMessage(entry.ToString(), (IPEndPoint)client.Client.RemoteEndPoint); // Format log message
                        Console.WriteLine(logMessage); // Display the message in the console

                        Log(entry); // Log the entry
                    }
                }
                else
                {
                    Console.WriteLine("Failed to read the length of the incoming data.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}"); // Handle exceptions and display the error
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}"); // Handle exceptions and display the error
        }
    }

    /// <summary>
    ///     Formats the log message with a timestamp and the client's IP address.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="client">The client's endpoint.</param>
    /// <returns>Formatted message.</returns>
    private static string FormatLogMessage(string message, IPEndPoint client)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); // Get the timestamp
        var clientIp = client.Address.ToString(); // Get the client's IP address
        return $"[{timestamp}] [{clientIp}] {message}"; // Return the formatted message
    }

    /// <summary>
    ///     Logs the provided message in the appropriate format.
    /// </summary>
    /// <param name="message">The log entry to log.</param>
    private static void Log(LogEntry message)
    {
        // Create the logs directory if it doesn't already exist
        if (!Directory.Exists("logs")) Directory.CreateDirectory("logs");
        _logger.Log(message); // Log the message using the logger
    }
}
