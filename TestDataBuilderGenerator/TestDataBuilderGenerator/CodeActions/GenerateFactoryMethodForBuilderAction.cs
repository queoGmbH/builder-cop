using System;
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
    public class GenerateFactoryMethodForBuilderAction : CodeAction {
        public GenerateFactoryMethodForBuilderAction(CodeFixContext context, TypeDeclarationSyntax declaration) {
            Context = context;
            Declaration = declaration;
        }

        public CodeFixContext Context { get; }

        public TypeDeclarationSyntax Declaration { get; }

        public override string EquivalenceKey {
            get { return "Generate Test-DataBuilder-Create-Method"; }
        }

        public override string Title {
            get { return "Generate TestDataBuilder-Create-Method"; }
        }

        protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken) {
            return await CreateTestDataBuilderCreateMethodAsync(cancellationToken);
        }

        private ClassDeclarationSyntax AddCreateMethodForBuilder(
            ClassDeclarationSyntax originalCreateClassDeclarationSyntax, ITypeSymbol builderTypeSymbol) {
            int indexForNewPublicMethod = TestDataBuilderSyntaxWriter.GetIndexForNewPublicMethod(originalCreateClassDeclarationSyntax);

            SyntaxList<MemberDeclarationSyntax> memberDeclarationSyntaxs =
                originalCreateClassDeclarationSyntax.Members.Insert(indexForNewPublicMethod, GetCreateMethodSyntax(builderTypeSymbol));
            return originalCreateClassDeclarationSyntax.WithMembers(memberDeclarationSyntaxs);
        }

        public async Task<Document> CreateTestDataBuilderCreateMethodAsync(CancellationToken cancellationToken) {
            try {
                Project documentProject = Context.Document.Project;
                string testProjectName = documentProject.Name + ".Test";
                Project testProject = documentProject.Solution.Projects.SingleOrDefault(p => p.Name == testProjectName);
                if (testProject == null) {
                    return null;
                }

                Compilation fieldCompilation = await documentProject.GetCompilationAsync(cancellationToken);
                SemanticModel semanticModel = fieldCompilation.GetSemanticModel(Declaration.SyntaxTree);
                ITypeSymbol entityTypeSymbol = semanticModel.GetDeclaredSymbol(Declaration);

                string builderTypeName = entityTypeSymbol.Name + "Builder";
                Compilation compilation = await testProject.GetCompilationAsync(cancellationToken);
                ITypeSymbol builderSymbol = compilation.GetSymbolsWithName(s => s == builderTypeName, SymbolFilter.Type).OfType<ITypeSymbol>()
                    .FirstOrDefault();
                if (builderSymbol == null) {
                    return null;
                }

                ITypeSymbol createClassSymbol =
                    compilation.GetSymbolsWithName(s => s == "Create", SymbolFilter.Type).OfType<ITypeSymbol>().FirstOrDefault();
                if (createClassSymbol == null) {
                    return null;
                }

                SyntaxReference createClassDeclarationSyntaxReference = createClassSymbol.DeclaringSyntaxReferences[0];
                ClassDeclarationSyntax originalCreateClassDeclarationSyntax =
                    createClassDeclarationSyntaxReference.GetSyntax() as ClassDeclarationSyntax;
                if (originalCreateClassDeclarationSyntax == null) {
                    return null;
                }

                Document document = testProject.GetDocument(originalCreateClassDeclarationSyntax.SyntaxTree);
                SyntaxTree documentSyntaxTree = document.GetSyntaxTreeAsync(cancellationToken).Result;
                SyntaxNode root = documentSyntaxTree.GetRoot();

                ClassDeclarationSyntax changedClassDeclarationSyntax = AddCreateMethodForBuilder(originalCreateClassDeclarationSyntax, builderSymbol);

                SyntaxNode formattedBuilderClassSyntax = Formatter.Format(changedClassDeclarationSyntax, document.Project.Solution.Workspace);
                SyntaxNode changedRootNode = root.ReplaceNode(originalCreateClassDeclarationSyntax, formattedBuilderClassSyntax);

                return document.WithSyntaxRoot(changedRootNode);
            } catch (Exception e) {
                Debug.WriteLine(e.Message);
                throw;
            }
        }

        private MethodDeclarationSyntax GetCreateMethodSyntax(ITypeSymbol builderTypeSymbol) {
            string builderTypeName = builderTypeSymbol.Name;
            string buildEntity = GetEntityFromBuilderTypeSymbol(builderTypeSymbol);

            return SyntaxFactory.MethodDeclaration(SyntaxFactory.IdentifierName(builderTypeName), SyntaxFactory.Identifier(buildEntity))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword))).WithBody(
                    SyntaxFactory.Block(
                        SyntaxFactory.SingletonList<StatementSyntax>(
                            SyntaxFactory.IfStatement(
                                    SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName("HasContext")),
                                    SyntaxFactory.Block(
                                        SyntaxFactory.SingletonList<StatementSyntax>(
                                            SyntaxFactory.ReturnStatement(
                                                SyntaxFactory.InvocationExpression(
                                                    SyntaxFactory.MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        SyntaxFactory.IdentifierName("_context"),
                                                        SyntaxFactory.GenericName(SyntaxFactory.Identifier("GetObject")).WithTypeArgumentList(
                                                            SyntaxFactory.TypeArgumentList(
                                                                SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                                                    SyntaxFactory.IdentifierName(builderTypeName))))))))))
                                .WithElse(
                                    SyntaxFactory.ElseClause(
                                        SyntaxFactory.Block(
                                            SyntaxFactory.SingletonList<StatementSyntax>(
                                                SyntaxFactory.ReturnStatement(
                                                    SyntaxFactory.ObjectCreationExpression(SyntaxFactory.IdentifierName(builderTypeName))
                                                        .WithArgumentList(
                                                            SyntaxFactory.ArgumentList(
                                                                SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                                                    new SyntaxNodeOrToken[] {
                                                                        SyntaxFactory.Argument(
                                                                            SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression))
                                                                    })))))))))))
                .NormalizeWhitespace();
        }

        private static string GetEntityFromBuilderTypeSymbol(ITypeSymbol builderType) {
            INamedTypeSymbol builderTypeBaseType = builderType.BaseType;
            if (builderTypeBaseType.IsGenericType) {
                return builderTypeBaseType.TypeArguments[0].Name;
            }

            return builderType.Name.Replace("Builder", "");
        }
    }
}