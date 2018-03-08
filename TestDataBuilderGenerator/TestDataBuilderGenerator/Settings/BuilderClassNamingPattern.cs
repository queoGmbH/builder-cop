using Newtonsoft.Json.Linq;

namespace TestDataBuilderGenerator.Settings {
    internal class BuilderClassNamingPattern {
        /// <inheritdoc />
        public BuilderClassNamingPattern() {
        }

        /// <inheritdoc />
        public BuilderClassNamingPattern(JToken namingPatternToken) {

        }

        /// <summary>
        /// Ruft den Präfix des verwendeten Namens-Musters ab, anhand dessen der Name eines Builders für eine Klasse ermittelt wird oder legt diesen fest.
        /// </summary>
        public string Prefix {
            get; set;
        }

        /// <summary>
        /// Ruft den Suffix des verwendeten Namens-Musters ab, anhand dessen der Name eines Builders für eine Klasse ermittelt wird oder legt diesen fest.
        /// </summary>
        public string SuffixPrefix {
            get; set;
        }
    }
}