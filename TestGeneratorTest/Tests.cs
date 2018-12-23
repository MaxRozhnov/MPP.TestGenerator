using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using TestGeneratorLib;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TestGeneratorTest
{
    [TestClass]
    public class Tests
    {
        private string _inputDirectory;
        private string _outputDirectory;
        private TestGenerator _testGenerator;

        [TestInitialize]

        public void Init()

        {
            _inputDirectory = Path.Combine("..", "..", "..","TestFiles");
            _outputDirectory = Path.Combine("..", "..", "..", "TestDirectoryOutput");
            _testGenerator = new TestGenerator(1, 1, 1, _outputDirectory);

            var testFiles = Directory.GetFiles(_inputDirectory).ToList();
            _testGenerator.Generate(testFiles).Wait();

        }

        [TestMethod]
        public void TestFilesAmount()
        {
            var testClassFiles = Directory.GetFiles(_outputDirectory).ToList();
            Assert.AreEqual(3, testClassFiles.Count);
        }

        [TestMethod]
        public void TestMethodAmount()
        {
            var generatedClass = ParseCompilationUnit(File.ReadAllText(Path.Combine(_outputDirectory, "TestClass3Test.cs")));
            var methods = generatedClass.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();
            Assert.AreEqual(2, methods.Count);
        }

        [TestMethod]
        public void TestImport()
        {
            var generatedClass = ParseCompilationUnit(File.ReadAllText(Path.Combine(_outputDirectory, "TestClass3Test.cs")));
            var msTestLib = "Microsoft.VisualStudio.TestTools.UnitTesting";
            Assert.IsTrue(generatedClass.DescendantNodes().OfType<UsingDirectiveSyntax>().Any(x => x.Name.ToString() == msTestLib));
        }

        [TestMethod]
        public void TestClassAttribute()
        {
            var generatedClass = ParseCompilationUnit(File.ReadAllText(Path.Combine(_outputDirectory, "TestClass3Test.cs")));
            var testClassAttribute = "TestClass";
            Assert.IsTrue(generatedClass.DescendantNodes().OfType<ClassDeclarationSyntax>()
                          .All(x => x.AttributeLists.Any(y => y.Attributes
                          .Any(z => z.ToString() == testClassAttribute))));
        }

        [TestMethod]
        public void TestMethodAttribute()
        {
            var generatedClass = ParseCompilationUnit(File.ReadAllText(Path.Combine(_outputDirectory, "TestClass3Test.cs")));
            var testClassAttribute = "TestMethod";
            Assert.IsTrue(generatedClass.DescendantNodes().OfType<MethodDeclarationSyntax>()
                          .All(x => x.AttributeLists.Any(y => y.Attributes
                          .Any(z => z.ToString() == testClassAttribute))));
        }

        [TestCleanup]
        public void CleanUp()

        {
            var files = Directory.GetFiles(_outputDirectory);
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }
    }
}
