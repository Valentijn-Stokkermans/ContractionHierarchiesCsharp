using static System.Net.Mime.MediaTypeNames;
using Testing;
class Program
{
    public static void Main()
    {
        Test newTest = new Test();
        newTest.testCorrectness(10);
    }
}