using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

namespace TestDataBuilderGenerator.CodeActions {
    public class GenerateBuilderAction : CodeAction {
        public GenerateBuilderAction(CodeFixContext context, TypeDeclarationSyntax declaration) {
            Context = context;
            Declaration = declaration;
        }

        public CodeFixContext Context { get; }
        public TypeDeclarationSyntax Declaration { get; }

        public override string EquivalenceKey {
            get { return nameof(GenerateBuilderAction); }
        }

        public override string Title {
            get { return nameof(GenerateBuilderAction); }
        }

        protected override async Task<Solution> GetChangedSolutionAsync(CancellationToken cancellationToken) {
            Solution changedSolutionAsync = await CreateTestDataBuilderAsync(Context.Document, Declaration, CancellationToken.None);
            return changedSolutionAsync;
        }

        private async Task<Solution> CreateTestDataBuilderAsync(
            Document contextDocument, TypeDeclarationSyntax declaration, CancellationToken cancellationToken) {
            try {
                Project contextDocumentProject = contextDocument.Project;
                Solution originalSolution = contextDocumentProject.Solution;
                Project testProject = originalSolution.Projects.FirstOrDefault(p => p.Name == contextDocument.Project.Name + ".Test");
                if (testProject == null) {
                    return originalSolution;
                }

                SemanticModel semanticModel = await contextDocument.GetSemanticModelAsync(cancellationToken);
                ISymbol typeSymbol = semanticModel.GetDeclaredSymbol(declaration, cancellationToken);

                /*Datei erstellen*/
                string[] folders = { "src", "Builders" };
                TestDataBuilderSyntaxWriter testDataBuilderSyntaxWriter = new TestDataBuilderSyntaxWriter(
                    typeSymbol.Name,
                    typeSymbol.ContainingNamespace.ToString());
                SyntaxNode syntaxNode = testDataBuilderSyntaxWriter.CreateBuilderClass();

                SyntaxNode formattedBuilderClassSyntax = Formatter.Format(syntaxNode, testProject.Solution.Workspace);

                /*Klasse anpassen*/
                SourceText sourceText = formattedBuilderClassSyntax.GetText();
                Document testDataBuilderDocument = testProject.AddDocument(typeSymbol.Name + "Builder.cs", sourceText, folders);
                return testDataBuilderDocument.Project.Solution;
            } catch (Exception e) {
                Debug.WriteLine(e.Message);
                throw;
            }
        }
    }
}