namespace EasySave.Models.Utils;

public static class StringExtension
{
    /// <summary>
    ///     Validates that the provided string is not null, empty, or whitespace.
    ///     Throws an exception if the validation fails.
    /// </summary>
    /// <param name="value">The string value to validate.</param>
    /// <param name="paramName">The name of the parameter, used in the exception if validation fails.</param>
    /// <returns>The trimmed string if validation succeeds.</returns>
    /// <exception cref="ArgumentException">Thrown when the value is null, empty, or whitespace.</exception>
    public static string ValidateNonEmpty(this string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be empty.", paramName); // Throw exception if validation fails

        return value.Trim(); // Return the trimmed value if validation succeeds
    }
}