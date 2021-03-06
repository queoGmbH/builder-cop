﻿using Microsoft.CodeAnalysis;

namespace TestDataBuilderGenerator.Diagnostics {
    internal static class TypeShouldHaveBuilderDiagnostic {
        internal const string DIAGNOSTIC_ID = "TypeShouldHaveTestDataBuilder";

        internal static DiagnosticDescriptor GetDescriptor() {
            return new DiagnosticDescriptor(
                DIAGNOSTIC_ID,
                Resources.TypeShouldHaveBuilder_Diagnostic_Title,
                Resources.TypeShouldHaveBuilder_Diagnostic_Message,
                "Test-Data-Builder",
                DiagnosticSeverity.Error,
                true,
                Resources.TypeShouldHaveBuilder_Diagnostic_Description);
        }

        internal static Diagnostic GetDiagnostic(INamedTypeSymbol namedTypeSymbol) {
            return Diagnostic.Create(GetDescriptor(), namedTypeSymbol.Locations[0], namedTypeSymbol.Name);
        }
    }
}