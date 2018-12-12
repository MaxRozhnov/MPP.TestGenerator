namespace TestGeneratorLib.Utils
{
    public class ClassModel
    {
        public readonly string Classname;
        public readonly string ClassContent;

        public ClassModel(string name, string content)
        {
            Classname = name;
            ClassContent = content;
        }
    }
}