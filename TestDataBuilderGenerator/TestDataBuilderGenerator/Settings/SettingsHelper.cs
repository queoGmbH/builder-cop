using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace TestDataBuilderGenerator.Settings {
    internal static class SettingsHelper {

        internal const string SettingsFileName = "buildercop.json";


        private static readonly SourceTextValueProvider<BuilderCopSettings> SettingsValueProvider = new SourceTextValueProvider<BuilderCopSettings>(
                text => GetBuilderCopSettings(text, DeserializationFailureBehavior.ReturnDefaultSettings));

        internal static BuilderCopSettings GetBuilderCopSettings(this SyntaxTreeAnalysisContext context, CancellationToken cancellationToken) {
            return context.Options.GetBuilderCopSettings(cancellationToken);
        }

        internal static BuilderCopSettings GetBuilderCopSettings(this AnalyzerOptions options, CancellationToken cancellationToken) {
            return GetBuilderCopSettings(options, DeserializationFailureBehavior.ReturnDefaultSettings, cancellationToken);
        }

        internal static BuilderCopSettings GetBuilderCopSettings(this AnalyzerOptions options, DeserializationFailureBehavior failureBehavior, CancellationToken cancellationToken) {
            return GetBuilderCopSettings(options != null ? options.AdditionalFiles : ImmutableArray.Create<AdditionalText>(), failureBehavior, cancellationToken);
        }

        internal static BuilderCopSettings GetBuilderCopSettings(this AnalysisContext context, AnalyzerOptions options, CancellationToken cancellationToken) {
            return GetBuilderCopSettings(context, options, DeserializationFailureBehavior.ReturnDefaultSettings, cancellationToken);
        }

        internal static BuilderCopSettings GetBuilderCopSettings(this AnalysisContext context, AnalyzerOptions options, DeserializationFailureBehavior failureBehavior, CancellationToken cancellationToken) {
            SourceText text = TryGetBuilderCopSettingsText(options, cancellationToken);
            if (text == null) {
                return new BuilderCopSettings();
            }

            if (failureBehavior == DeserializationFailureBehavior.ReturnDefaultSettings) {
                BuilderCopSettings settings;
                if (!context.TryGetValue(text, SettingsValueProvider, out settings)) {
                    return new BuilderCopSettings();
                }

                return settings;
            }

            return GetBuilderCopSettings(text, failureBehavior);
        }

        internal static BuilderCopSettings GetBuilderCopSettings(this CompilationStartAnalysisContext context, AnalyzerOptions options, CancellationToken cancellationToken) {
            return GetBuilderCopSettings(context, options, DeserializationFailureBehavior.ReturnDefaultSettings, cancellationToken);
        }

        internal static BuilderCopSettings GetBuilderCopSettings(this CompilationStartAnalysisContext context, AnalyzerOptions options, DeserializationFailureBehavior failureBehavior, CancellationToken cancellationToken)
        {
            SourceText text = TryGetBuilderCopSettingsText(options, cancellationToken);
            if (text == null) {
                return new BuilderCopSettings();
            }

            if (failureBehavior == DeserializationFailureBehavior.ReturnDefaultSettings) {
                BuilderCopSettings settings;
                if (!context.TryGetValue(text, SettingsValueProvider, out settings)) {
                    return new BuilderCopSettings();
                }

                return settings;
            }

            return GetBuilderCopSettings(text, failureBehavior);
        }


        private static void ValidateResource(JObject settings, Stream schemaStream) {
            if (settings == null) {
                throw new ArgumentNullException(nameof(settings));
            }
            if (schemaStream == null) {
                throw new ArgumentNullException(nameof(schemaStream));
            }

            JObject schemaObject;
            using (StreamReader sr = new StreamReader(schemaStream)) {
                string json = sr.ReadToEnd();
                schemaObject = JsonConvert.DeserializeObject<JObject>(json);
            }
            
            JSchema schema = JSchema.Parse(schemaObject.ToString());
            ValidateResource(settings, schema);
        }

        private static void ValidateResource(JObject settings, JSchema schema) {
            if (settings == null) {
                throw new ArgumentNullException(nameof(settings));
            }
            if (schema == null) {
                throw new ArgumentNullException(nameof(schema));
            }

            if (!settings.IsValid(schema)) {
                throw new JsonException("Die Validierung der JSon-Datei mit den Einstellungen war nicht erfolgreich.");
            }
        }


        private static BuilderCopSettings GetBuilderCopSettings(SourceText text, DeserializationFailureBehavior failureBehavior) {
            try {
                JObject settingsObject = JsonConvert.DeserializeObject<JObject>(text.ToString());
                ValidateResource(
                    settingsObject,
                    typeof(BuilderCopSettings).GetTypeInfo().Assembly.GetManifestResourceStream("TestDataBuilderGenerator.Settings.Resources.BuilderCop.json-schema.json"));
                return new BuilderCopSettings(settingsObject.SelectToken("settings"));
            } catch (Exception ex) when (failureBehavior == DeserializationFailureBehavior.ReturnDefaultSettings) {
                // The settings file is invalid -> return the default settings. 
                Debug.WriteLine(ex.Message);           
            }

            return new BuilderCopSettings();
        }

        private static SourceText TryGetBuilderCopSettingsText(this AnalyzerOptions options, CancellationToken cancellationToken) {
            foreach (var additionalFile in options.AdditionalFiles) {
                if (Path.GetFileName(additionalFile.Path).ToLowerInvariant() == SettingsFileName) {
                    return additionalFile.GetText(cancellationToken);
                }
            }

            return null;
        }

        private static BuilderCopSettings GetBuilderCopSettings(ImmutableArray<AdditionalText> additionalFiles, DeserializationFailureBehavior failureBehavior, CancellationToken cancellationToken) {
            foreach (var additionalFile in additionalFiles) {
                if (Path.GetFileName(additionalFile.Path).ToLowerInvariant() == SettingsFileName) {
                    SourceText additionalTextContent = additionalFile.GetText(cancellationToken);
                    return GetBuilderCopSettings(additionalTextContent, failureBehavior);
                }
            }

            return new BuilderCopSettings();
        }
    }
}
