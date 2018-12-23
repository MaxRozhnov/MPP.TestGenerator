using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TestGeneratorLib.Utils
{
    public class FileWriter
    {
        private string _outputDirectory;
        public FileWriter(string outputDirectory)
        {
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            _outputDirectory = outputDirectory;
        }
        public async Task WriteAsync(List<TestClassModel> classModels)
        {
            

            foreach (var classModel in classModels)
            {
                string outputPath = Path.Combine(_outputDirectory, classModel.Classname + ".cs");
                using (var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    byte[] dataBuffer = Encoding.UTF8.GetBytes(classModel.ClassContent);
                    await fileStream.WriteAsync(dataBuffer, 0, dataBuffer.Length);
                }
            }
        }
    }
}