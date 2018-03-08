using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using TestDataBuilderGenerator.Diagnostics;

namespace TestDataBuilderGenerator.Helper {
    internal static class SymbolAnalysisContextHelper {
        internal static Diagnostic ReportTypeShouldHaveBuilderDiagnostic(this SymbolAnalysisContext context, INamedTypeSymbol namedTypeSymbol) {
            Diagnostic diagnostic = TypeShouldHaveBuilderDiagnostic.GetDiagnostic(namedTypeSymbol);
            context.ReportDiagnostic(diagnostic);
            return diagnostic;
        }

        internal static Diagnostic ReportFactoryMethodForBuilderExpected(this SymbolAnalysisContext context, ISymbol contextSymbol) {
            Diagnostic diagnostic = Diagnostic.Create(
                FactoryMethodForBuilderExpectedDiagnostic.GetDescriptor(),
                contextSymbol.Locations[0],
                contextSymbol.Name);
            context.ReportDiagnostic(diagnostic);
            return diagnostic;
        }

        internal static Diagnostic ReportFieldHasNoSetterInBuilder(this SymbolAnalysisContext context, IFieldSymbol fieldWithoutSetterMethod) {
            Diagnostic diagnostic = FieldHasNoSetterInBuilderDiagnostic.GetDiagnostic(fieldWithoutSetterMethod);
            context.ReportDiagnostic(diagnostic);

            return diagnostic;
        }
    }
}