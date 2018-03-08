using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TestDataBuilderGenerator {
    public class TestDataBuilderSyntaxWriter {
        private readonly string _builderFor;
        private readonly string _builderForNamespace;
        private readonly string _builderName;
        private readonly string _builderNamespace = "Com.QueoFlow.Immotwist.Maklerportal.Core.Builders";
        
        public TestDataBuilderSyntaxWriter(string builderFor, string builderForNamespace) {
            _builderFor = builderFor;
            _builderForNamespace = builderForNamespace;
            _builderName = GetBuilderName(_builderFor);
        }

        public SyntaxNode CreateBuilderClass() {
            CompilationUnitSyntax compilationUnitSyntax = CompilationUnit();

            CompilationUnitSyntax documentWithUsings = compilationUnitSyntax.WithUsings(
                List(
                    new[] {
                        CreateUsingDirectiveSyntax(_builderForNamespace)
                    }));

            return documentWithUsings
                .WithMembers(
                    SingletonList<MemberDeclarationSyntax>(
                        NamespaceDeclaration(IdentifierName(_builderNamespace))
                            .WithMembers(
                                SingletonList<MemberDeclarationSyntax>(
                                    ClassDeclaration(GetBuilderName(_builderFor)).WithModifiers(
                                            TokenList(Token(SyntaxKind.PublicKeyword)))
                                        .WithBaseList(
                                            BaseList(
                                                SingletonSeparatedList<BaseTypeSyntax>(
                                                    SimpleBaseType(
                                                        GenericName(Identifier("EntityBuilder"))
                                                            .WithTypeArgumentList(
                                                                TypeArgumentList(
                                                                    SingletonSeparatedList<TypeSyntax>(
                                                                        IdentifierName(_builderFor))))))))
                                        .WithMembers(
                                            List(
                                                new MemberDeclarationSyntax[] {
                                                    CreateConstructor(),
                                                    CreateSetEntityFieldsMethod()
                                                }))))))
                .NormalizeWhitespace();
        }

        public static FieldDeclarationSyntax CreateFieldSyntax(string type, string name, params SyntaxKind[] modifiers) {
            return FieldDeclaration(
                    VariableDeclaration(IdentifierName(type)).WithVariables(
                        SingletonSeparatedList(
                            VariableDeclarator(Identifier(name)))))
                .WithModifiers(
                    TokenList(modifiers.Select(Token)))
                .NormalizeWhitespace();
        }

        /// <summary>
        ///     Erzeugt den Namen des Builders für den Typen der zu erstellenden Klasse.
        /// </summary>
        /// <param name="builderFor"></param>
        /// <returns></returns>
        public static string GetBuilderName(string builderFor) {
            return builderFor + "Builder";
        }
        
        /// <summary>
        ///     Erzeugt aus einem Eigenschaften-Namen den Namen für das zugehörige Feld.
        ///     Bsp.: Firstname => _firstname;
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static string GetFieldNameByPropertyName(string propertyName) {
            return "_" + char.ToLowerInvariant(propertyName[0]) + propertyName.Substring(1);
        }

        /// <summary>
        ///     Erzeugt aus einem Typnamen den Namen für das zugehörige Feld.
        ///     Bsp.: TaskDao => _taskDao;
        /// </summary>
        /// <param name="typename"></param>
        /// <returns></returns>
        public static string GetFieldNameByType(string typename) {
            return "_" + ToLowerCamelCase(typename);
        }

        public static int GetIndexForNewConstructor(ClassDeclarationSyntax builderClassSyntax) {
            ConstructorDeclarationSyntax lastConstructorDeclarationInClass =
                builderClassSyntax.Members.OfType<ConstructorDeclarationSyntax>().LastOrDefault();
            if (lastConstructorDeclarationInClass != null) {
                /*Nach dem letzten Konstruktor einfügen*/
                return builderClassSyntax.Members.IndexOf(lastConstructorDeclarationInClass) + 1;
            } else {
                /*Nach dem letzten Feld oder ganz oben einfügen.*/
                return GetIndexForNewField(builderClassSyntax);
            }
        }

        public static int GetIndexForNewField(ClassDeclarationSyntax builderClassSyntax) {
            FieldDeclarationSyntax lastFieldDeclarationInClass = builderClassSyntax.Members.OfType<FieldDeclarationSyntax>().LastOrDefault();

            if (lastFieldDeclarationInClass == null) {
                /*Bisher keine Felder in der Klasse => Das neu Feld ganz oben.*/
                return 0;
            } else {
                int indexOfField = builderClassSyntax.Members.IndexOf(lastFieldDeclarationInClass) + 1;
                return indexOfField;
            }
        }

        public static int GetIndexForNewPublicMethod(ClassDeclarationSyntax builderClassSyntax) {
            MethodDeclarationSyntax lastPublicMethod = builderClassSyntax.Members.OfType<MethodDeclarationSyntax>()
                .LastOrDefault(m => m.Modifiers.Contains(Token(SyntaxKind.PublicKeyword)));
            if (lastPublicMethod == null) {
                /*Bisher keine public Methoden in der Klasse => Die neue Methode nach dem letzten Konstruktor.*/
                return GetIndexForNewConstructor(builderClassSyntax);
            } else {
                /*Nach der letzten öffentlichen Methode einfügen*/
                int indexForNewPublicMethod = builderClassSyntax.Members.IndexOf(lastPublicMethod) + 1;
                return indexForNewPublicMethod;
            }
        }

        /// <summary>
        ///     Erzeugt den Namen des Interfaces für einen Typen, in dem ein "I" vor den Namen gestellt wird.
        /// </summary>
        public static string GetInterfaceName(string type) {
            return "I" + type;
        }

        /// <summary>
        ///     Erzeugt den Standardmäßigen Eigenschafts-Namen anhand des Namens des Felds.
        ///     Bsp.: _firstname => firstname;
        /// </summary>
        /// <returns></returns>
        public static string GetLocalVariableOrParameterNameFromFieldName(string fieldName) {
            return ToLowerCamelCase(fieldName.Substring(1));
        }

        /// <summary>
        ///     Erzeugt aus einem Typnamen den Namen für das zugehörige Feld.
        ///     Bsp.: TaskDao => _taskDao;
        /// </summary>
        /// <param name="source">Der umzuwandelnde String.</param>
        /// <returns></returns>
        public static string GetLocalVariableOrParameterNameFromType(string source) {
            return ToLowerCamelCase(source);
        }

        /// <summary>
        ///     Erzeugt den Standardmäßigen Methodennamen anhand des Namens der Eigenschaft die mit dieser Methode gesetzt werden
        ///     soll.
        ///     Bsp.: _firstname => WithFirstname;
        /// </summary>
        /// <returns></returns>
        public static string GetMethodeNameFromFieldName(string fieldName) {
            return GetMethodeNameFromPropertyName(GetPropertyNameFromFieldName(fieldName));
        }

        /// <summary>
        ///     Erzeugt den Standardmäßigen Methodennamen anhand des Namens der Eigenschaft die mit dieser Methode gesetzt werden
        ///     soll.
        ///     Bsp.: Firstname => WithFirstname;
        /// </summary>
        /// <returns></returns>
        public static string GetMethodeNameFromPropertyName(string propertyName) {
            return "With" + propertyName;
        }

        /// <summary>
        ///     Erzeugt den Standardmäßigen Eigenschafts-Namen anhand des Namens des Felds.
        ///     Bsp.: _firstname => Firstname;
        /// </summary>
        /// <returns></returns>
        public static string GetPropertyNameFromFieldName(string fieldName) {
            return ToUpperCamelCase(fieldName.Substring(1));
        }

        public static string GetTypeNameFromSymbol(ITypeSymbol typeSymbol) {
            return typeSymbol.ToDisplayString();
        }

        public static MethodDeclarationSyntax MethodDeclarationSyntax(string builderTypeName, IFieldSymbol fieldSymbol) {
            string fieldName = fieldSymbol.Name;
            string parameterName = GetLocalVariableOrParameterNameFromFieldName(fieldName);
            string fieldTypeName = GetTypeNameFromSymbol(fieldSymbol.Type);
            return
                MethodDeclaration(
                        IdentifierName(builderTypeName),
                        GetMethodeNameFromFieldName(fieldName))
                    .WithModifiers(
                        TokenList(
                            Token(SyntaxKind.PublicKeyword)))
                    .WithParameterList(
                        ParameterList(
                            SingletonSeparatedList(
                                Parameter(Identifier(parameterName)).WithType(IdentifierName(fieldTypeName)))))
                    .WithBody(
                        Block(
                            ExpressionStatement(
                                AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    IdentifierName(fieldName),
                                    IdentifierName(parameterName))),
                            ReturnStatement(ThisExpression()))).NormalizeWhitespace();
        }

        public static string ToLowerCamelCase(string value) {
            return char.ToLowerInvariant(value[0]) + value.Substring(1);
        }

        public static string ToUpperCamelCase(string value) {
            return char.ToUpperInvariant(value[0]) + value.Substring(1);
        }

        public static ClassDeclarationSyntax UpdateBuilderToGenerateCodeToSetField(ClassDeclarationSyntax originalClassDeclarationSyntax, ITypeSymbol builderSymbol, IFieldSymbol fieldSymbol, ITypeSymbol builtType) {
            ClassDeclarationSyntax changedBuilderClassSyntax = originalClassDeclarationSyntax;

            /*Feld einfügen*/
            int indexOfField = GetIndexForNewField(originalClassDeclarationSyntax);
            changedBuilderClassSyntax = originalClassDeclarationSyntax.WithMembers(
                changedBuilderClassSyntax.Members.Insert(
                    indexOfField,
                    CreateFieldSyntax(fieldSymbol.Type.ToDisplayString(), fieldSymbol.Name, SyntaxKind.PrivateKeyword)));

            /*Setter-Methode einfügen*/
            int indexOfSetterMethod = GetIndexForNewPublicMethod(changedBuilderClassSyntax);
            changedBuilderClassSyntax = changedBuilderClassSyntax.WithMembers(
                changedBuilderClassSyntax.Members.Insert(indexOfSetterMethod, MethodDeclarationSyntax(builderSymbol.Name, fieldSymbol)));

            /*Setzen des Felds in Entity hinzufügen.*/
            changedBuilderClassSyntax = AddSetFieldViaReflectionExpression(changedBuilderClassSyntax, fieldSymbol, GetLocalVariableOrParameterNameFromType(builtType.Name));

            return changedBuilderClassSyntax;
        }

        private static ClassDeclarationSyntax AddSetFieldViaReflectionExpression(ClassDeclarationSyntax builderDeclarationSyntax, IFieldSymbol fieldSymbol, string setterMethodParameterName) {
            MethodDeclarationSyntax setEntityFieldsMethod = GetSetEntityFieldsMethod(builderDeclarationSyntax);

            BlockSyntax changedBlockSyntax = setEntityFieldsMethod.Body.AddStatements(
                ExpressionStatement(
                    InvocationExpression(IdentifierName("SetFieldViaReflection")).WithArgumentList(
                        ArgumentList(
                            SeparatedList<ArgumentSyntax>(
                                new SyntaxNodeOrToken[] {
                                    Argument(IdentifierName(setterMethodParameterName)), Token(SyntaxKind.CommaToken),
                                    Argument(
                                        InvocationExpression(IdentifierName("nameof")).WithArgumentList(
                                            ArgumentList(SingletonSeparatedList(Argument(IdentifierName(fieldSymbol.Name)))))),
                                    Token(SyntaxKind.CommaToken), Argument(IdentifierName(fieldSymbol.Name))
                                })))).NormalizeWhitespace());

            MethodDeclarationSyntax changedSetEntityFieldsMethod = setEntityFieldsMethod.WithBody(changedBlockSyntax);

            return builderDeclarationSyntax.ReplaceNode(setEntityFieldsMethod, changedSetEntityFieldsMethod);
        }

        private ConstructorDeclarationSyntax CreateConstructor() {
            string builderName = _builderName;
            
            return ConstructorDeclaration(Identifier(builderName)).WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))                
                .WithBody(Block()).NormalizeWhitespace();
        }

        private MethodDeclarationSyntax CreateSetEntityFieldsMethod() {
            string entityType = _builderFor;
            string entityParameterName = GetLocalVariableOrParameterNameFromType(entityType);

            return
                MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier("SetEntityFields"))
                    .WithModifiers(TokenList(Token(SyntaxKind.ProtectedKeyword), Token(SyntaxKind.OverrideKeyword)))
                    .WithParameterList(
                        ParameterList(
                            SingletonSeparatedList(
                                Parameter(Identifier(entityParameterName))
                                    .WithType(IdentifierName(entityType)))))
                    .WithBody(Block()).NormalizeWhitespace();
        }

        private static UsingDirectiveSyntax CreateUsingDirectiveSyntax(string useNamespace) {
            return UsingDirective(IdentifierName(useNamespace));
        }

        private static MethodDeclarationSyntax GetSetEntityFieldsMethod(ClassDeclarationSyntax builderClassDeclarationSyntax) {
            MethodDeclarationSyntax setEntityFieldsDeclarationSyntax =
                builderClassDeclarationSyntax
                    .DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .FirstOrDefault(md => md.Identifier.Text == "SetEntityFields");

            return setEntityFieldsDeclarationSyntax;
        }
    }
}