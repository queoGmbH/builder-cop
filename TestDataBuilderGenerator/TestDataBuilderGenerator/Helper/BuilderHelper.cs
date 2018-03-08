using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using TestDataBuilderGenerator.Settings;

namespace TestDataBuilderGenerator.Helper {
    internal static class BuilderHelper {


        internal static bool TryGetTestDataBuilder(this INamedTypeSymbol namedTypeSymbol, BuilderCopSettings builderCopSettings, out INamedTypeSymbol builderSymbol) {

            Project testProject = namedTypeSymbol.GetAccordingTestProject();
            IList<ISymbol> foundBuilderDeclarations = SymbolFinder.FindDeclarationsAsync(testProject, namedTypeSymbol.Name + "Builder", true).Result.ToList();
            builderSymbol = foundBuilderDeclarations.OfType<INamedTypeSymbol>().FirstOrDefault();

            return builderSymbol != null;
        }


        internal static INamedTypeSymbol GetTestDataBuilder(this INamedTypeSymbol namedTypeSymbol, BuilderCopSettings builderCopSettings) {
            INamedTypeSymbol builderSymbol;
            if (TryGetTestDataBuilder(namedTypeSymbol, builderCopSettings, out builderSymbol)) {
                return builderSymbol;
            } else {
                return null;
            }
        }

        internal static IList<IFieldSymbol> FindFieldsWithoutSetterMethodInBuilder(INamedTypeSymbol builderSymbol, INamedTypeSymbol builderForSymbol) {
            IList<IFieldSymbol> entityTypeFields =
                builderForSymbol.GetMembers().Where(mem => mem.Kind == SymbolKind.Field).Cast<IFieldSymbol>().ToList();
            IEnumerable<IMethodSymbol> builderMethods = builderSymbol.GetMembers().Where(mem => mem.Kind == SymbolKind.Method).Cast<IMethodSymbol>();
            IList<IMethodSymbol> builderOrdinaryMethods =
                builderMethods.Where(mem => mem.MethodKind == MethodKind.Ordinary && mem.ReturnType.Name == builderSymbol.Name).ToList();
            IList<IFieldSymbol> fieldsWithoutSetterMethod =
                entityTypeFields.Where(ef => !builderOrdinaryMethods.Any(bm => IsAccordingSetterMethod(ef, bm))).ToList();
            return fieldsWithoutSetterMethod;
        }

        internal static bool IsAccordingSetterMethod(IFieldSymbol field, IMethodSymbol setterMethod) {
            string fieldNameWithoutUnderscore = field.Name.Substring(1);
            string expectedNameInMethod = TestDataBuilderSyntaxWriter.ToUpperCamelCase(fieldNameWithoutUnderscore);

            if (setterMethod.Name.EndsWith(expectedNameInMethod) && !setterMethod.Parameters.Any()) {
                /*Die Methode hat den gleichen Namen wie das Feld (ohne _ und mit großem Anfangsbuchstaben) und hat keine Parameter => sollte passen. */
                /*Bsp.: _isPublished => Published() */
                return true;
            } else {
                foreach (IParameterSymbol methodParameter in setterMethod.Parameters) {
                    if (methodParameter.Name == fieldNameWithoutUnderscore && TypeHelper.AreTypesEqual(field.Type, methodParameter.Type)) {
                        /*Der Name des Parameters entspricht dem Namen (ohne _) und die Typen sind gleich. => sollte passen*/
                        /*Bsp.: string _firstname => WithFirstname(string firstname) */
                        /*Bsp.: string _name => WithFirstname(string name) */
                        /*Bsp.: DateTime _createdAt und User _createdBy => CreatedByAndAt(User createdBy, DateTime createdAt) */
                        return true;
                    }
                }
            }

            return false;
        }
    }
}