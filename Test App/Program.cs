using System.Collections.Generic;
using System.IO;
using TestGeneratorLib;

namespace Test_App
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            TestGenerator testGenerator = new TestGenerator(1,1,1, @"C:\Users\Max\Desktop");
            
            testGenerator.Generate(new List<string>()
            {
                Path.Combine(@"C:\Users\Max\RiderProjects\Lab Task 4\TestGeneratorLib\Utils\Class1.cs"),              
            }).Wait();
        }
    }
}