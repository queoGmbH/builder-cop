using Microsoft.CodeAnalysis;

namespace TestDataBuilderGenerator.Diagnostics {
    internal static class FieldHasNoSetterInBuilderDiagnostic {

        internal const string DIAGNOSTIC_ID = "FieldShouldHaveSetterMethodInBuilder";

        internal static DiagnosticDescriptor GetDescriptor() {
            return new DiagnosticDescriptor(
                DIAGNOSTIC_ID,
                Resources.FieldShouldHaveSetterMethodInBuilder_Diagnostic_Title,
                Resources.FieldShouldHaveSetterMethodInBuilder_Diagnostic_Message,
                "Test-Data-Builder",
                DiagnosticSeverity.Error,
                true,
                Resources.FieldShouldHaveSetterMethodInBuilder_Diagnostic_Description);
        }


        internal static Diagnostic GetDiagnostic(IFieldSymbol fieldSymbol) {
            return Diagnostic.Create(GetDescriptor(), fieldSymbol.Locations[0], fieldSymbol.Name);
        }
    }
}