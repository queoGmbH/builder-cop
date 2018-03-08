using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace TestDataBuilderGenerator.Settings {
    internal class BuilderForSettings {
        /// <inheritdoc />
        public BuilderForSettings() {
            BaseTypes = ImmutableArray.Create<string>();
            MarkerInterfaces = ImmutableArray.Create<string>();
            MarkerAttributes = ImmutableArray.Create<string>();
            Types = ImmutableArray.Create<string>();
        }

        /// <inheritdoc />
        public BuilderForSettings(JToken settingsObject) : this() {
            Types = settingsObject.GetImmutableStringArrayFromToken<string>("types");
            BaseTypes = settingsObject.GetImmutableStringArrayFromToken<string>("baseTypes");
            MarkerAttributes = settingsObject.GetImmutableStringArrayFromToken<string>("markerAttributes");
            MarkerInterfaces = settingsObject.GetImmutableStringArrayFromToken<string>("markerInterfaces");
        }

        

        /// <summary>
        /// Ruft die Liste mit expliziten Typen ab, für die ein Builder existieren muss oder legt diese fest.
        /// </summary>
        public ImmutableArray<string> Types { get; set; }

        /// <summary>
        /// Ruft die Liste der Typen ab, die wenn eine Klasse davon erbt, dazu führen, dass für diese Klasse die ein Builder existieren muss oder legt diese fest.
        /// </summary>
        public ImmutableArray<string> BaseTypes {
            get; set;
        }

        /// <summary>
        /// Ruft die Liste der Marker-Interfaces ab, die dazu führen, dass wenn ein Klasse mindestens eines dieser Interfaces implementiert, ein Builder für diese Klasse existieren muss oder legt diese fest.
        /// </summary>
        public ImmutableArray<string> MarkerInterfaces {
            get; set;
        }

        /// <summary>
        /// Ruft die Liste mit Marker-Attributen ab, die dazu führen, dass wenn eine Klasse mit mindestens einem dieser Attribute ausgezeichnet ist, ein Builder für diese Klasse existieren muss oder legt diese fest.
        /// </summary>
        public ImmutableArray<string> MarkerAttributes {
            get; set;
        }

    }
}