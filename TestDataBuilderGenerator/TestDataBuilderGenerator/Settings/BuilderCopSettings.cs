using Newtonsoft.Json.Linq;

namespace TestDataBuilderGenerator.Settings {
    internal class BuilderCopSettings {

        private readonly BuilderForSettings _builderForSettings;
        private readonly BuilderFactorySettings _builderFactorySettings;
        private readonly BuilderSettings _builderSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuilderCopSettings"/> class.
        /// </summary>
        protected internal BuilderCopSettings() {
            _builderForSettings = new BuilderForSettings();
            _builderFactorySettings = new BuilderFactorySettings();
            _builderSettings = new BuilderSettings();
        }

        protected internal BuilderCopSettings(JToken settingsObject) {

            JToken builderForToken = settingsObject.SelectToken("builderForSettings", false);
            if (builderForToken != null) {
                _builderForSettings = new BuilderForSettings(builderForToken);
            } else {
                _builderForSettings = new BuilderForSettings();
            }
            
            JToken builderFactoryToken = settingsObject.SelectToken("builderFactorySettings", false);
            if (builderFactoryToken != null) {
                _builderFactorySettings = new BuilderFactorySettings(builderFactoryToken);
            } else {
                _builderFactorySettings = new BuilderFactorySettings();
            }

            JToken builderSettingsToken = settingsObject.SelectToken("builderSettings", false);
            if (builderSettingsToken != null) {
                _builderSettings = new BuilderSettings(builderSettingsToken);
            } else {
                _builderSettings = new BuilderSettings();
            }
        }

        public BuilderForSettings BuilderForSettings {
            get { return _builderForSettings; }
        }

        public BuilderFactorySettings BuilderFactorySettings {
            get { return _builderFactorySettings; }
        }

        public BuilderSettings BuilderSettings {
            get { return _builderSettings; }
        }
    }
}