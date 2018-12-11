using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestGeneratorLib
{
    
    public class TestGenerator
    {
        private int _maxFilesLoadedSimultaneously;
        private int _maxFilesGeneratedSimultaneously; 
        private int _maxFilesWrittenSimultaneously;
        
        public TestGenerator(int maxFilesLoadedSimultaneously, int maxFilesGeneratedSimultaneously, int maxFilesWrittenSimultaneously)
        {
            _maxFilesLoadedSimultaneously = maxFilesLoadedSimultaneously;
            _maxFilesGeneratedSimultaneously = maxFilesGeneratedSimultaneously;
            _maxFilesWrittenSimultaneously = maxFilesWrittenSimultaneously;
        }
        
        public Task<List<string>> Generate()
        {
            
        }
        
        
    }
}