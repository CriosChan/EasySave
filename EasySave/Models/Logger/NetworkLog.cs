using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using EasySave.Data.Configuration;

namespace EasySave.Models.Logger;

/// <summary>
///     Singleton class for logging over a network using TCP.
/// </summary>
public sealed class NetworkLog
{
    // Lazy initialization for the singleton instance
    private static readonly Lazy<NetworkLog> instance = new(() => new NetworkLog());
    private IPEndPoint? _endpoint; // Endpoint for sending logs

    private TcpClient? _tcpClient; // TCP client for sending log messages
    public EventHandler? OnConnect; // Event triggered when the connection is established
    public EventHandler? OnDisconnect; // Event triggered when the connection is lost

    /// <summary>
    ///     Private constructor to prevent direct instantiation.
    ///     Socket creation is deferred to an explicit <see cref="CreateSocket"/> call
    ///     so that callers can subscribe to <see cref="OnConnect"/>/<see cref="OnDisconnect"/>
    ///     before the connection is attempted.
    /// </summary>
    private NetworkLog()
    {
    }

    /// <summary>
    ///     Gets the singleton instance of the NetworkLog class.
    /// </summary>
    public static NetworkLog Instance => instance.Value;

    /// <summary>
    ///     Creates a TCP socket for logging.
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

                _tcpClient = new TcpClient(); // Instantiate the TCP client
                _tcpClient.Connect(_endpoint); // Establish the TCP connection
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
    ///     Closes the TCP socket if it is open.
    /// </summary>
    public void CloseSocket()
    {
        try
        {
            if (_tcpClient != null)
            {
                _tcpClient.Close(); // Close the socket
                _tcpClient = null; // Clear the reference
                Console.WriteLine("Socket closed.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error when closing socket: {ex.Message}");
        }
    }

    private readonly JsonSerializerOptions _options = new JsonSerializerOptions
    {
        WriteIndented = false // No indentation for compact messages
    };
    
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
            var data = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(message, _options));

            try
            {
                if (_tcpClient is { Connected: true })
                {
                    NetworkStream stream = _tcpClient.GetStream();
                    // Send the length of the data first
                    var lengthBytes = BitConverter.GetBytes(data.Length);
                    stream.Write(lengthBytes, 0, lengthBytes.Length); // Send length (4 bytes)
            
                    // Then send the actual data
                    stream.Write(data, 0, data.Length); // Send log entry data
                }
                else
                {
                    throw new InvalidOperationException("TCP client is not connected.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
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
