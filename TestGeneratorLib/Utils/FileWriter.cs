using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TestGeneratorLib.Utils
{
    public class FileWriter
    {
        public async Task WriteAsync(List<TestClassModel> classModels, string outputDirectory)
        {
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            foreach (var classModel in classModels)
            {
                string outputPath = Path.Combine(outputDirectory, classModel.Classname + ".cs");
                using (var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    byte[] dataBuffer = Encoding.ASCII.GetBytes(classModel.ClassContent);
                    await fileStream.WriteAsync(dataBuffer, 0, dataBuffer.Length);
                }
            }
        }
    }
}