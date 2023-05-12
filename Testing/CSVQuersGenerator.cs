using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing
{
    public static class CSVQuersGenerator
    {
        public static void Create()
        {
            string filePath = @"C:\Users\Valentijn\source\repos\ContractionHierarchies\ContractionHierarchies\Data\CSVQuery.csv";
            int rowCount = 1000; // number of queries to be made
            int minValue = 0;
            int maxValue = 234615; // number of nodes in roads.csv + 1

            using (StreamWriter sw = new StreamWriter(filePath))
            {
                for (int i = 0; i < rowCount; i++)
                {
                    int num1 = GenerateRandomNumber(minValue, maxValue);
                    int num2 = GenerateRandomNumber(minValue, maxValue);
                    sw.WriteLine($"{num1},{num2}");
                }
            }
            Console.WriteLine($"CSV file with {rowCount} rows has been generated at: {filePath}");
        }

        static int GenerateRandomNumber(int minValue, int maxValue)
        {
            Random rnd = new Random();
            return rnd.Next(minValue, maxValue + 1);
        }
    }
}
