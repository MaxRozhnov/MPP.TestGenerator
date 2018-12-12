namespace TestGeneratorLib.Utils
{
    public class TestClassModel
    {
        public readonly string Classname;
        public readonly string ClassContent;

        public TestClassModel(string name, string content)
        {
            Classname = name;
            ClassContent = content;
        }
    }
}