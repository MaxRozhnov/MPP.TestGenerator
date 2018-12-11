using System;
using System.IO;
using System.Threading.Tasks;

namespace TestGeneratorLib.Utils
{
    public class FileWriter
    {
        public async Task<string> ReadAsync(string filepath)
        {
            try
            {
                using (StreamReader streamReader = new StreamReader(filepath))
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