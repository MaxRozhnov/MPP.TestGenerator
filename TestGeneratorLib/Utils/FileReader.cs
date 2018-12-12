using System;
using System.IO;
using System.Threading.Tasks;

namespace TestGeneratorLib.Utils
{
    public class FileReader
    {
        public async Task<string> ReadAsync(string filepath)
        {
            try
            {
                using (var streamReader = new StreamReader(filepath))
                {
                    return await streamReader.ReadToEndAsync();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went terribly wrong, the file could not be read.");
                return ""; //wut?
            }
        }
        
    }
}