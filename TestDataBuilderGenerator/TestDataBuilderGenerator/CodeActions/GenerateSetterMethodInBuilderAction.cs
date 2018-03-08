using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace TestDataBuilderGenerator.CodeActions {
    public class GenerateSetterMethodInBuilderAction : CodeAction {
        public GenerateSetterMethodInBuilderAction(CodeFixContext context, FieldDeclarationSyntax declaration) {
            Context = context;
            Declaration = declaration;
        }

        public CodeFixContext Context { get; }

        public FieldDeclarationSyntax Declaration { get; }

        public override string EquivalenceKey {
            get {
                Debug.WriteLine("GenerateTestDataBuilder-Setter-Method-Action - Equivalence Key");
                return "Generate Test-DataBuilder-Setter-Method";
            }
        }

        public override string Title {
            get {
                Debug.WriteLine("GenerateTestDataBuilder-Setter-Method-Action - Title");
                return "Generate Test-DataBuilder-Setter-Method";
            }
        }

        protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken) {
            Project documentProject = Context.Document.Project;
            string testProjectName = documentProject.Name + ".Test";
            Project testProject = documentProject.Solution.Projects.SingleOrDefault(p => p.Name == testProjectName);
            if (testProject == null) {
                return null;
            }

            Compilation fieldCompilation = await documentProject.GetCompilationAsync(cancellationToken);
            SemanticModel semanticModel = fieldCompilation.GetSemanticModel(Declaration.SyntaxTree);
            IFieldSymbol fieldSymbol = (IFieldSymbol)semanticModel.GetDeclaredSymbol(Declaration.Declaration.Variables[0]);
            ITypeSymbol type = fieldSymbol.ContainingType;

            string builderTypeName = type.Name + "Builder";
            Compilation compilation = await testProject.GetCompilationAsync(cancellationToken);
            ITypeSymbol builderSymbol = compilation.GetSymbolsWithName(s => s == builderTypeName, SymbolFilter.Type).OfType<ITypeSymbol>()
                .FirstOrDefault();
            if (builderSymbol == null) {
                return null;
            }

            SyntaxReference builderSymbolDeclaringSyntaxReference = builderSymbol.DeclaringSyntaxReferences[0];
            ClassDeclarationSyntax classDeclarationFromSymbol = builderSymbolDeclaringSyntaxReference.GetSyntax() as ClassDeclarationSyntax;
            if (classDeclarationFromSymbol == null) {
                return null;
            }

            Document document = testProject.GetDocument(classDeclarationFromSymbol.SyntaxTree);
            SyntaxTree documentSyntaxTree = document.GetSyntaxTreeAsync(cancellationToken).Result;
            SyntaxNode root = documentSyntaxTree.GetRoot();
            ClassDeclarationSyntax originalClassDeclarationSyntax = root.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if (originalClassDeclarationSyntax == null) {
                return null;
            }

            ClassDeclarationSyntax changedBuilderClassSyntax =
                TestDataBuilderSyntaxWriter.UpdateBuilderToGenerateCodeToSetField(originalClassDeclarationSyntax, builderSymbol, fieldSymbol, type);

            SyntaxNode formattedBuilderClassSyntax = Formatter.Format(changedBuilderClassSyntax, document.Project.Solution.Workspace);
            SyntaxNode changedRootNode = root.ReplaceNode(originalClassDeclarationSyntax, formattedBuilderClassSyntax);

            return document.WithSyntaxRoot(changedRootNode);
        }
    }
}