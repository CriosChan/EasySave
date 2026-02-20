using EasySave.Core.Models;
using EasySave.Models.Backup;
using EasySave.Models.Backup.Interfaces;

namespace EasySaveTest;

/// <summary>
///     Tests business software blocking behavior during backup execution.
/// </summary>
public class BusinessSoftwareBlockingTest
{
    /// <summary>
    ///     Test monitor that returns a predefined running/not-running sequence.
    /// </summary>
    /// <param name="sequence">Sequence of values returned by process checks.</param>
    private sealed class SequenceBusinessSoftwareMonitor(params bool[] sequence) : IBusinessSoftwareMonitor
    {
        private readonly Queue<bool> _sequence = new(sequence);
        private bool _last;

        /// <summary>
        ///     Gets the configured software names used for assertions and logs.
        /// </summary>
        public IReadOnlyList<string> ConfiguredSoftwareNames { get; } = ["CalculatorApp"];

        /// <summary>
        ///     Returns the next configured value, then repeats the last known value.
        /// </summary>
        /// <returns>True when business software should be considered running.</returns>
        public bool IsBusinessSoftwareRunning()
        {
            if (_sequence.Count == 0)
                return _last;

            _last = _sequence.Dequeue();
            return _last;
        }
    }
}
