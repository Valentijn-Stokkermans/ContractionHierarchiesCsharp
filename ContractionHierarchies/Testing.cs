using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContractionHierarchies
{
    public static class Testing
    {
        public static void TestPerformance(int maxSettledNodes, int edgeGroupSize, int importanceType, int contractionType, int contractionSearchType, bool recalculateImportance)
        {
            string graphFile = @"C:\\Users\\Valentijn\\source\\repos\\ContractionHierarchies\\ContractionHierarchies\\Data\\netherlands.csv";
            string queryFile = @"C:\Users\Valentijn\source\repos\ContractionHierarchies\ContractionHierarchies\Data\netherlandsQuery.csv";

            // preprocess
            var ch = new ContractionHierarchie(graphFile, edgeGroupSize);
            var watchPreprocessing = System.Diagnostics.Stopwatch.StartNew();
            ch.PreProcess(importanceType, contractionType, contractionSearchType, recalculateImportance, maxSettledNodes);
            ch.CreateSearchGraph();
            watchPreprocessing.Stop();
            long elapsedMSPreprocessing = watchPreprocessing.ElapsedMilliseconds;
            Console.WriteLine("Preprocessing time: " + elapsedMSPreprocessing);
            // read query file
            List<string[]> fields = new() { };
            using (TextFieldParser parser = new(queryFile))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                while (!parser.EndOfData)
                {
                    //Processing row
                    fields.Add(parser.ReadFields());
                }
            }

            // run queries
            var watchQuery = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < fields.Count; i++)
            {
                int source = int.Parse(fields[i][0]);
                int target = int.Parse(fields[i][1]);

                ch.QueryCH(source, target);
            }
            watchQuery.Stop();
            long elapsedMSQuery = watchQuery.ElapsedMilliseconds;
            Console.WriteLine("Total query time: " + elapsedMSQuery + " over " + fields.Count + " queries, Average: " + (double)elapsedMSQuery / fields.Count);
        }

        public static void TestCorrectness()
        {
            var file = "C:\\Users\\Valentijn\\source\\repos\\ContractionHierarchies\\ContractionHierarchies\\Data\\example_graph.csv";
            //var file = "C:\\Users\\Valentijn\\source\\repos\\ContractionHierarchies\\ContractionHierarchies\\Data\\small_directed_graph.csv";
            List<string[]> fields = new() { };
            using (TextFieldParser parser = new(file))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                while (!parser.EndOfData)
                {
                    //Processing row
                    fields.Add(parser.ReadFields());
                }
            }
            fields.RemoveAt(0); // drop column description

            HashSet<int> nodes = new HashSet<int>();

            // count unique nodes
            for (int i = 0; i < fields.Count; i++)
            {
                nodes.Add(int.Parse(fields[i][0]));
            }

            //preprocessing stage.
            var maxSettledNodes = 100;
            var edgeGroupSize = 2;
            int importanceType = 0;
            int contractionType = 0;
            int contractionSearchType = 0;
            bool recalculateImportance = false;
            var ch = new ContractionHierarchie(file, edgeGroupSize);
            ch.PreProcess(importanceType, contractionType, contractionSearchType, recalculateImportance, maxSettledNodes);
            ch.CreateSearchGraph();

            int wrongRes = 0;
            int correctRes = 0;

            for (int i = 0; i < nodes.Count; i++)
            {
                for (int j = 0; j < nodes.Count; j++)
                {
                    //acutal distance computation stage.
                    float resCH = ch.QueryCH(i, j);
                    float resDijkstra = ch.QueryDijkstra(i, j);
                    if (resCH != resDijkstra)
                    {
                        Console.WriteLine($"\n\n\n\n\n\n\n\n\n\n\nWRONG: from: {i} to: {j}, dijkstra: {resDijkstra}, CH: {resCH}\n\n\n\n\n\n\n\n\n");
                        wrongRes++;
                    }
                    else
                    {
                        Console.WriteLine($"CORRECT from: {i} to: {j}, dijkstra: {resDijkstra}, CH: {resCH}");
                        correctRes++;
                    }
                }
            }
            Console.WriteLine("Number of wrong results: " + wrongRes);
            Console.WriteLine("Number of correct results: " + correctRes);
        }

        public static void CreateCSVQueries()
        {
            string filePath = @"C:\Users\Valentijn\source\repos\ContractionHierarchies\ContractionHierarchies\Data\netherlandsQuery.csv";
            int rowCount = 1000; // number of queries to be made
            int minValue = 0;
            int maxValue = 3189645; // 234615 nodes in franceRoute500.csv (+ 1), 3189645 nodes in netherlands.csv

            using (StreamWriter sw = new(filePath))
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

        public static int GenerateRandomNumber(int minValue, int maxValue)
        {
            Random rnd = new();
            return rnd.Next(minValue, maxValue + 1);
        }
    }
}
