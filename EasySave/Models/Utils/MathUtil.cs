namespace EasySave.Models.Utils;

public static class MathUtil
{
    /// <summary>
    ///     Calculates the percentage of a given actual value relative to a total.
    ///     Ensures that the result does not exceed 100%.
    /// </summary>
    /// <param name="actual">The actual value to evaluate.</param>
    /// <param name="total">The total value to compare against.</param>
    /// <returns>The calculated percentage, capped at 100%.</returns>
    public static double Percentage(double actual, double total)
    {
        // If total is less than or equal to 0, return 100% to avoid division by zero.
        return total <= 0 ? 100 : Math.Min(100, actual / total * 100d);
    }
}