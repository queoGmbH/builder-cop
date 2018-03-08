using Microsoft.CodeAnalysis;

namespace TestDataBuilderGenerator.Helper {
    internal static class TypeHelper {
        internal static bool AreTypesEqual(ITypeSymbol fieldType, ITypeSymbol parameterType) {
            // TODO: Diese Methode ist notwendig, da die ITypeSymbols nicht Equal sind. Kann man sicher auch besser schreiben.
            return fieldType.Name == parameterType.Name &&
                   fieldType.ContainingNamespace.ToString() == parameterType.ContainingNamespace.ToString();
        }
    }
}