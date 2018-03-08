using Microsoft.CodeAnalysis;

namespace TestDataBuilderGenerator.Diagnostics {
    internal static class FactoryMethodForBuilderExpectedDiagnostic {
        internal const string DIAGNOSTIC_ID = "FactoryMethodForBuilderExpectedDiagnostic";

        internal static DiagnosticDescriptor GetDescriptor() {
            return new DiagnosticDescriptor(
                DIAGNOSTIC_ID,
                Resources.FactoryMethodForBuilderExpected_Diagnostic_Title,
                Resources.FactoryMethodForBuilderExpected_Diagnostic_Message,
                "Test-Data-Builder",
                DiagnosticSeverity.Error,
                true,
                Resources.FactoryMethodForBuilderExpected_Diagnostic_Description);
        }

        internal static Diagnostic GetDiagnostic(INamedTypeSymbol namedTypeSymbol) {
            return Diagnostic.Create(GetDescriptor(), namedTypeSymbol.Locations[0], namedTypeSymbol.Name);
        }
    }
}