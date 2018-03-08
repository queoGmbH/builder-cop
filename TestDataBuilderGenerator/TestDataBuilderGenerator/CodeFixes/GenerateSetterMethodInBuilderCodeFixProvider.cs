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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GenerateSetterMethodInBuilderCodeFixProvider)), Shared]
    public class GenerateSetterMethodInBuilderCodeFixProvider : CodeFixProvider {
        public sealed override ImmutableArray<string> FixableDiagnosticIds {
            get {
                return ImmutableArray.Create(FieldHasNoSetterInBuilderDiagnostic.DIAGNOSTIC_ID);
            }
        }

        public sealed override FixAllProvider GetFixAllProvider() {
            return WellKnownFixAllProviders.BatchFixer;
        }


        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            
            
            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<FieldDeclarationSyntax>().First();
            
            // Register a code action that will invoke the fix.
            var codeAction = new GenerateSetterMethodInBuilderAction(context, declaration);
            context.RegisterCodeFix(codeAction, diagnostic);
        }
    }
}