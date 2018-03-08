using System.Linq;
using Microsoft.CodeAnalysis;
using TestDataBuilderGenerator.Settings;

namespace TestDataBuilderGenerator.Helper {
    internal static class NamedTypeSymbolHelper {
        internal static bool ShouldTypeHaveBuilder(this INamedTypeSymbol namedTypeSymbol, BuilderForSettings builderForSettings) {

            if (namedTypeSymbol.GetAccordingTestProject() == null) {
                return false;
            }

            if (namedTypeSymbol.ShouldHaveBuilderBecauseOfBaseClass(builderForSettings)) {
                return true;
            } else if (namedTypeSymbol.ShouldHaveBuilderBecauseOfMarkerInterface(builderForSettings)) {
                return true;
            } else if (namedTypeSymbol.ShouldHaveBuilderBecauseOfMarkerAttribute(builderForSettings)) {
                return true;
            } else {
                return namedTypeSymbol.BaseType.ShouldTypeHaveBuilder(builderForSettings);
            }
        }

        private static bool ImplementsMarkerInterface(this INamedTypeSymbol namedTypeSymbol, string markerInterface) {
            return namedTypeSymbol.Interfaces.Any(
                implementedInterface => implementedInterface.Name == markerInterface ||
                                        implementedInterface.ToDisplayString() == markerInterface);
        }

        private static bool IsDecoratedWithMarkerAttribute(this INamedTypeSymbol namedTypeSymbol, string markerAttribute) {
            return namedTypeSymbol.GetAttributes().Any(
                attribute => attribute.AttributeClass.Name == markerAttribute ||
                             attribute.AttributeClass.ToDisplayString() == markerAttribute);
        }

        private static bool IsDerivedFrom(this INamedTypeSymbol namedTypeSymbol, string baseType) {
            return namedTypeSymbol.BaseType.Name == baseType ||
                   namedTypeSymbol.BaseType.ToDisplayString() == baseType;
        }

        private static bool ShouldHaveBuilderBecauseOfBaseClass(this INamedTypeSymbol namedTypeSymbol, BuilderForSettings builderForSettings) {
            if (namedTypeSymbol.BaseType == null) {
                return false;
            }

            foreach (string baseType in builderForSettings.BaseTypes) {
                if (namedTypeSymbol.IsDerivedFrom(baseType)) {
                    return true;
                }
            }

            return false;
        }

        private static bool ShouldHaveBuilderBecauseOfMarkerAttribute(this INamedTypeSymbol namedTypeSymbol, BuilderForSettings builderForSettings) {
            if (!namedTypeSymbol.GetAttributes().Any()) {
                return false;
            }

            foreach (string markerAttribute in builderForSettings.MarkerAttributes) {
                if (namedTypeSymbol.IsDecoratedWithMarkerAttribute(markerAttribute) ||
                    namedTypeSymbol.IsDecoratedWithMarkerAttribute(markerAttribute + "Attribute")) {
                    return true;
                }
            }

            return false;
        }

        private static bool ShouldHaveBuilderBecauseOfMarkerInterface(this INamedTypeSymbol namedTypeSymbol, BuilderForSettings builderForSettings) {
            if (!namedTypeSymbol.Interfaces.Any()) {
                return false;
            }

            foreach (string markerInterface in builderForSettings.MarkerInterfaces) {
                if (namedTypeSymbol.ImplementsMarkerInterface(markerInterface)) {
                    return true;
                }
            }

            return false;
        }
    }
}