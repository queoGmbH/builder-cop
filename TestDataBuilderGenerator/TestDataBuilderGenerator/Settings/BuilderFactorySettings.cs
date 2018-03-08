using Newtonsoft.Json.Linq;

namespace TestDataBuilderGenerator.Settings {
    internal class BuilderFactorySettings {
        /// <inheritdoc />
        public BuilderFactorySettings() {
            Name = "Create";
        }

        /// <inheritdoc />
        public BuilderFactorySettings(JToken settingsObject) {
            if (settingsObject == null) {
                return;
            }

            Name = settingsObject.GetValueFromToken("Create", "name");
        }

        /// <summary>
        /// Ruft den Namen der Create-Klasse ab, die für das Erzeugen bzw. 
        /// </summary>
        public string Name { get; set; }
    }
}