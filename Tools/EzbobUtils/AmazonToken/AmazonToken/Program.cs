namespace AmazonToken
{

    internal class Program
    {
        private static void Main()
        {
            BackFill linesRetriver = new BackFill(4);
            linesRetriver.Execute();
        }
    }
}