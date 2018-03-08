using Newtonsoft.Json.Linq;

namespace TestDataBuilderGenerator.Settings {
    internal class BuilderSettings {
        /// <inheritdoc />
        public BuilderSettings() {
            NamingPattern = new BuilderClassNamingPattern();
        }

        /// <inheritdoc />
        public BuilderSettings(JToken settingsObject) {
            NamingPattern = new BuilderClassNamingPattern(settingsObject.SelectToken("namingPattern", false));
            BaseClass = settingsObject.GetValueFromToken<string>(null, "baseClass");
        }

        /// <summary>
        /// Ruft die Einstellungen für das Namens-Muster ab, anhand dessen der Name eines Builders für eine Klasse ermittelt wird oder legt dieses fest.
        /// </summary>
        internal BuilderClassNamingPattern NamingPattern { get; set; }

        /// <summary>
        /// Ruft den optionalen Typen ab, von welchem die Builder erben sollen oder legt diesen fest.
        /// </summary>
        internal string BaseClass { get; set; }

    }
}