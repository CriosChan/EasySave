using System.Text.RegularExpressions;

namespace EasySave.Models.Utils;

/// <summary>
///     Static class containing utility methods for validation.
/// </summary>
public static class Validator
{
    /// <summary>
    ///     Validates if the given string is a valid IPv4 address.
    /// </summary>
    /// <param name="ip">The IP address to validate.</param>
    /// <returns>True if the IP address is valid; otherwise, false.</returns>
    public static bool IsValidIPv4(string ip)
    {
        // Regular expression pattern for matching IPv4 addresses
        var pattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";

        // Use regex to check if the provided IP matches the pattern
        return Regex.IsMatch(ip, pattern);
    }
}