using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestDataBuilderGenerator.CodeActions;
using TestDataBuilderGenerator.Diagnostics;

namespace TestDataBuilderGenerator.CodeFixes {
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GenerateFactoryMethodForBuilderCodeFixProvider)), Shared]
    public class GenerateFactoryMethodForBuilderCodeFixProvider : CodeFixProvider {
        public sealed override ImmutableArray<string> FixableDiagnosticIds {
            get {
                return ImmutableArray.Create(FactoryMethodForBuilderExpectedDiagnostic.DIAGNOSTIC_ID);
            }
        }

        public sealed override FixAllProvider GetFixAllProvider() {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }


        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();

            // Register a code action that will invoke the fix.
            GenerateFactoryMethodForBuilderAction codeAction = new GenerateFactoryMethodForBuilderAction(context, declaration);
            context.RegisterCodeFix(codeAction, diagnostic);
        }
    }
}