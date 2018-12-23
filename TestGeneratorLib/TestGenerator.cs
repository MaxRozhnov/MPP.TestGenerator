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
        
        public TestGenerator(int maxFilesLoadedSimultaneously, int maxFilesGeneratedSimultaneously, int maxFilesWrittenSimultaneously, string outputDirectory)
        {
            _maxFilesLoadedSimultaneously = maxFilesLoadedSimultaneously;
            _maxFilesGeneratedSimultaneously = maxFilesGeneratedSimultaneously;
            _maxFilesWrittenSimultaneously = maxFilesWrittenSimultaneously;
            
            _fileReader = new FileReader();
            _fileWriter = new FileWriter(outputDirectory);
            
        }
        
        public Task Generate(List<string> classFiles)
        {
            var readBlock = new TransformBlock<string, string>(
                new Func<string, Task<string>>(_fileReader.ReadAsync),
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = _maxFilesLoadedSimultaneously
                }
            );
            
            var testClassGenerationBlock = new TransformBlock<string, List<TestClassModel>>(
                new Func<string, List<TestClassModel>>(GenerateTestClassFilesAsync),
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = _maxFilesGeneratedSimultaneously
                }
             );
            
            var writeBLock = new ActionBlock<List<TestClassModel>>(
                new Action<List<TestClassModel>>((x) => _fileWriter.WriteAsync(x).Wait()),
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = _maxFilesWrittenSimultaneously
                }
            );
            
            var options = new DataflowLinkOptions { PropagateCompletion = true };
            
            readBlock.LinkTo(testClassGenerationBlock, options);
            testClassGenerationBlock.LinkTo(writeBLock, options);

            foreach (var classFile in classFiles)
            {
                readBlock.Post(classFile);
            }
            
            readBlock.Complete();
            
            return writeBLock.Completion;
        }
        
        private List<TestClassModel> GenerateTestClassFilesAsync(string fileText)
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

                string nameSpace = (declaredClass.Parent as NamespaceDeclarationSyntax)?.Name?.ToString() ?? "Global";

                var testMethodsList = createTestMethods(methods);
                var testClassDeclaration = createTestClass(declaredClass.Identifier.ValueText + "Test", testMethodsList);
                testClasses.Add(new TestClassModel(declaredClass.Identifier.ValueText + "Test", createFullClassDeclaration(nameSpace, testClassDeclaration, imports).NormalizeWhitespace().ToFullString()));
            }
            
            return testClasses;
        }

        private List<MemberDeclarationSyntax> createTestMethods(IEnumerable<MethodDeclarationSyntax> classMethods)
        {
            List<MemberDeclarationSyntax> testMethods = new List<MemberDeclarationSyntax>();
            foreach (var classMethod in classMethods)
            {
                var methodName = classMethod.Identifier.ToString();
                if (testMethods.Count != 0 && testMethods.Any(x =>
                        (x as MethodDeclarationSyntax)?.Identifier.ToString() == classMethod.Identifier.ToString() + "Test"))
                {
                    int i = 1;
                    while (testMethods.Any(x =>
                        (x as MethodDeclarationSyntax)?.Identifier.ToString() ==
                        classMethod.Identifier.ToString() + i + "Test"))
                    {
                        i++;
                    }

                    methodName += i;
                }
                
                testMethods.Add(createTestMethodDeclaration(methodName));
            }
            
           

            return testMethods;
        }

        private MethodDeclarationSyntax createTestMethodDeclaration(string methodName)
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

        private MemberDeclarationSyntax createTestClass(string testClassName, List<MemberDeclarationSyntax> testMethods)
        {

            return ClassDeclaration(testClassName)

                .WithAttributeLists(SingletonList(AttributeList(SingletonSeparatedList(Attribute(IdentifierName("TestClass"))))))

                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))

                .WithMembers(List(testMethods));

        }

        private CompilationUnitSyntax createFullClassDeclaration(string nameSpace, MemberDeclarationSyntax testClass, List<UsingDirectiveSyntax> imports )
        {
            return CompilationUnit()
                .WithUsings(List(imports))
                .WithMembers(SingletonList<MemberDeclarationSyntax>(
                    NamespaceDeclaration(QualifiedName(IdentifierName(nameSpace), IdentifierName("Test")))
                        .WithMembers(SingletonList(testClass))));
        }
    }
}