using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using EasySave.Data.Configuration;

namespace EasySave.Models.Logger;

/// <summary>
///     Singleton class for logging over a network using UDP.
/// </summary>
public sealed class NetworkLog
{
    // Lazy initialization for the singleton instance
    private static readonly Lazy<NetworkLog> instance = new(() => new NetworkLog());
    private IPEndPoint _endpoint; // Endpoint for sending logs

    private UdpClient? _udpClient; // UDP client for sending log messages
    public EventHandler? OnConnect; // Event triggered when the connection is established
    public EventHandler? OnDisconnect; // Event triggered when the connection is lost

    /// <summary>
    ///     Private constructor to prevent direct instantiation.
    ///     Initializes the socket.
    /// </summary>
    private NetworkLog()
    {
        CreateSocket();
    }

    /// <summary>
    ///     Gets the singleton instance of the NetworkLog class.
    /// </summary>
    public static NetworkLog Instance => instance.Value;

    /// <summary>
    ///     Creates a UDP socket for logging.
    /// </summary>
    public void CreateSocket()
    {
        lock (this) // Ensure thread safety
        {
            CloseSocket(); // Ensure any existing socket is closed

            try
            {
                // Load the server IP and port from the application configuration
                _endpoint = new IPEndPoint(
                    IPAddress.Parse(ApplicationConfiguration.Load().EasySaveServerIp),
                    ApplicationConfiguration.Load().EasySaveServerPort);

                _udpClient = new UdpClient(); // Instantiate the UDP client
                OnConnectEvent(); // Trigger the connect event
                Console.WriteLine("Socket created and ready to use.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error creating socket: {e.Message}");
                OnDisconnectEvent(); // Trigger the disconnect event on error
            }
        }
    }

    /// <summary>
    ///     Closes the UDP socket if it is open.
    /// </summary>
    public void CloseSocket()
    {
        try
        {
            if (_udpClient != null)
            {
                _udpClient.Close(); // Close the socket
                _udpClient = null; // Clear the reference
                Console.WriteLine("Socket closed.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error when closing socket: {ex.Message}");
        }
    }

    /// <summary>
    ///     Sends a log message to the defined endpoint.
    /// </summary>
    /// <typeparam name="T">The type of the message to log.</typeparam>
    /// <param name="message">The message to be sent as a log.</param>
    public void Log<T>(T message)
    {
        lock (this) // Ensure thread safety
        {
            // Serialize the message to JSON and convert it to byte array
            var data = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(message, new JsonSerializerOptions
            {
                WriteIndented = false // No indentation for compact messages
            }));

            try
            {
                _udpClient?.Send(data, data.Length, _endpoint); // Send the log message
            }
            catch
            {
                // On failure to send, attempt to recreate the socket
                CreateSocket();
            }
        }
    }

    /// <summary>
    ///     Raises the OnDisconnect event.
    /// </summary>
    private void OnDisconnectEvent()
    {
        OnDisconnect?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///     Raises the OnConnect event.
    /// </summary>
    private void OnConnectEvent()
    {
        OnConnect?.Invoke(this, EventArgs.Empty);
    }
}