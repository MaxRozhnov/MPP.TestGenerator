using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestGeneratorLib.Utils;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TestGeneratorLib
{
    
    public class TestGenerator
    {
        private readonly int _maxFilesLoadedSimultaneously;
        private readonly int _maxFilesGeneratedSimultaneously; 
        private readonly int _maxFilesWrittenSimultaneously;

        private FileReader _fileReader;
        private FileWriter _fileWriter;
        
        public TestGenerator(int maxFilesLoadedSimultaneously, int maxFilesGeneratedSimultaneously, int maxFilesWrittenSimultaneously)
        {
            _maxFilesLoadedSimultaneously = maxFilesLoadedSimultaneously;
            _maxFilesGeneratedSimultaneously = maxFilesGeneratedSimultaneously;
            _maxFilesWrittenSimultaneously = maxFilesWrittenSimultaneously;
            
            _fileReader = new FileReader();
            _fileWriter = new FileWriter();
            
        }
        
        public Task<List<string>> Generate(List<string> Files)
        {
            var readBlock = new TransformBlock<string, string>(
                new Func<string, Task<string>>(_fileReader.ReadAsync),
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = _maxFilesLoadedSimultaneously
                }
            );
            
            //TODO: add two more blocks with processing and writing
            
            //TODO: should probably add a return statement as well.
        }
        
        private async Task<List<TestClassModel>> GenerateTestClassFiles(string fileText)
        {
            var testClasses = new List<TestClassModel>();
            
            CompilationUnitSyntax compilationUnitSyntax = ParseCompilationUnit(fileText);
            var declaredClasses = compilationUnitSyntax.DescendantNodes().OfType<ClassDeclarationSyntax>();
            var imports = new List<UsingDirectiveSyntax>();
            imports.Add(UsingDirective(QualifiedName(IdentifierName("Microsoft.VisualStudio"), IdentifierName("TestTools.UnitTesting"))));

            foreach (var declaredClass in declaredClasses)
            {
                var methods = declaredClass.DescendantNodes().OfType<MethodDeclarationSyntax>()
                    .Where(x => x.Modifiers.Any(y => y.ValueText == "public"));

                string nameSpace = (declaredClass.Parent as NamespaceDeclarationSyntax)?.Name.ToString();
                if (nameSpace == null)
                {
                    nameSpace = "Global";
                }
            }
            
            return testClasses;
        }

        private List<MemberDeclarationSyntax> generateTestMethods(IEnumerable<MethodDeclarationSyntax> classMethods)
        {
            List<MemberDeclarationSyntax> testMethods = new List<MemberDeclarationSyntax>();
            foreach (var classMethod in classMethods)
            {
                var methodName = classMethod.Identifier.ToString();
            }


            return testMethods;
        }

        private MethodDeclarationSyntax createMethodDeclaration(string methodName)
        {
            return MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier(methodName + "Test"))
                .WithAttributeLists(SingletonList(AttributeList(SingletonSeparatedList( //Method with attribute [TestMethods]
                    Attribute(IdentifierName("TestMethod"))))))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))              //Method with "public" access modifier
                .WithBody(Block(ExpressionStatement(InvocationExpression(
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("Assert"), IdentifierName("Fail")))          //Method with body only containing Assert.Fail(test)
                    .WithArgumentList(ArgumentList(SingletonSeparatedList(
                        Argument(LiteralExpression(SyntaxKind.StringLiteralExpression,
                            Literal("test")))))))));
        }
        
        
    }
}