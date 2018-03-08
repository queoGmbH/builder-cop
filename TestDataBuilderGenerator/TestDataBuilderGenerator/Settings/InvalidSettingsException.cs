using System;

namespace TestDataBuilderGenerator.Settings {
    /// <summary>
    /// Exception thrown when an invalid settings have been encountered.
    /// </summary>
    internal class InvalidSettingsException : Exception {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidSettingsException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        internal InvalidSettingsException(string message)
            : base(message) {
        }
    }
}