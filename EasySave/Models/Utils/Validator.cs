using System.Text.RegularExpressions;

namespace EasySave.Models.Utils;

public static class Validator
{
    public static bool IsValidIPv4(string ip)
    {
        string pattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
        return Regex.IsMatch(ip, pattern);
    }
}