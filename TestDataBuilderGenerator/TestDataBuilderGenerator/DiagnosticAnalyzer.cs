using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FindSymbols;
using TestDataBuilderGenerator.Diagnostics;
using TestDataBuilderGenerator.Helper;
using TestDataBuilderGenerator.Settings;

namespace TestDataBuilderGenerator {
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TestDataBuilderGeneratorAnalyzer : DiagnosticAnalyzer {
        
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics {
            get {
                return ImmutableArray.Create(
                    TypeShouldHaveBuilderDiagnostic.GetDescriptor(),
                    FactoryMethodForBuilderExpectedDiagnostic.GetDescriptor(),
                    FieldHasNoSetterInBuilderDiagnostic.GetDescriptor());
            }
        }

        public void AnalyzeSymbol(SymbolAnalysisContext context) {


            BuilderCopSettings builderCopSettings = context.Options.GetBuilderCopSettings(new CancellationToken());

            INamedTypeSymbol namedTypeSymbol = (INamedTypeSymbol)context.Symbol;
            if (!namedTypeSymbol.ShouldTypeHaveBuilder(builderCopSettings.BuilderForSettings)) {
                return;
            }

            INamedTypeSymbol builderSymbol;
            if (!namedTypeSymbol.TryGetTestDataBuilder(builderCopSettings, out builderSymbol)) {
                context.ReportTypeShouldHaveBuilderDiagnostic(namedTypeSymbol);
            } else {
                AnalyseFactory(context, namedTypeSymbol.GetAccordingTestProject(), builderSymbol, builderCopSettings);
                AnalyseBuilder(context, builderSymbol, namedTypeSymbol);
            }
        }
        
        public override void Initialize(AnalysisContext context) {
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }
        
        /// <summary>
        ///     Überprüft, ob es eine Create-Klasse gibt, die eine Instanz des Builders erzeugen kann.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="testProject"></param>
        /// <param name="builderSymbol"></param>
        /// <param name="builderCopSettings"></param>
        private void AnalyseFactory(SymbolAnalysisContext context, Project testProject, INamedTypeSymbol builderSymbol, BuilderCopSettings builderCopSettings) {
            ITypeSymbol createClassSymbol = testProject.GetBuilderFactorySymbol(builderCopSettings);
            if (createClassSymbol == null) {
                return;
            }

            foreach (IMethodSymbol method in createClassSymbol.GetMembers().OfType<IMethodSymbol>()) {
                if (method.MethodKind == MethodKind.Ordinary && TypeHelper.AreTypesEqual(method.ReturnType, builderSymbol)) {
                    return;
                }
            }

            /* Die Diagnostic geht auf die Entity-Klasse, weil dass die Klasse ist, die untersucht wird. 
             * Wird die Location auf den Builder gelegt, fliegt eine Exception, da der Builder nicht Teil des Kompilats ist, 
             * welches analysiert wird.*/
            context.ReportFactoryMethodForBuilderExpected(context.Symbol);
        }

        

        private void AnalyseBuilder(SymbolAnalysisContext context, INamedTypeSymbol builderSymbol, INamedTypeSymbol builderForSymbol) {
            IList<IFieldSymbol> fieldsWithoutSetterMethods = BuilderHelper.FindFieldsWithoutSetterMethodInBuilder(builderSymbol, builderForSymbol);
            foreach (IFieldSymbol fieldWithoutSetterMethod in fieldsWithoutSetterMethods) {
                context.ReportFieldHasNoSetterInBuilder(fieldWithoutSetterMethod);
            }
        }
    }
}