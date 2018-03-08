using Newtonsoft.Json;

namespace TestDataBuilderGenerator.Settings {
    /// <summary>
    /// Defines the behaviour of various <see cref="SettingsHelper"/> methods in the event of a deserialization error.
    /// </summary>
    internal enum DeserializationFailureBehavior {
        /// <summary>
        /// When deserialization fails, return a default <see cref="BuilderCopSettings"/> instance.
        /// </summary>
        ReturnDefaultSettings,

        /// <summary>
        /// When deserialization fails, throw a <see cref="JsonException"/> or
        /// <see cref="InvalidSettingsException"/> containing details about the error.
        /// </summary>
        ThrowException,
    }
}