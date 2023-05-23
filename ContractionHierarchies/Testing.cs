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
        /// <summary>
        /// <para> 
        /// <paramref name="importanceType"/> 
        /// 0: simple heuristic
        /// 1: simulation
        /// <paramref name="contractionType"/>
        /// 0: BiDir speedup contraction
        /// 1: normal contraction
        /// <paramref name="contractionSearchType"/>
        /// 0: dijkstra
        /// <paramref name="recalculateImportance"/>
        /// true: check if the importance is still the smallest
        /// false: do not recalculate the importance
        /// <paramref name="maxSettledNodes"/>
        /// number of nodes that should be settled before placing a shortcut
        /// </para>
        /// </summary>
        public static void TestPerformancePreProcess(int maxSettledNodes, int edgeGroupSize, int importanceType, int contractionType, int contractionSearchType, bool recalculateImportance, int maxWrongImportance)
        {
            //string graphFile = @"C:\\Users\\Valentijn\\source\\repos\\ContractionHierarchies\\ContractionHierarchies\\Data\\flevoland.csv";
            string graphFile = @"C:\\Users\\Valentijn\\source\\repos\\ContractionHierarchies\\ContractionHierarchies\\Data\\franceRoute500.csv";

            // preprocess
            var ch = new ContractionHierarchie(graphFile, edgeGroupSize, importanceType, contractionType, contractionSearchType, recalculateImportance, maxSettledNodes, maxSettledNodes, maxWrongImportance);
            var watchPreprocessing = System.Diagnostics.Stopwatch.StartNew();
            ch.PreProcess();
            ch.CreateSearchGraph();
            watchPreprocessing.Stop();
            long elapsedMSPreprocessing = watchPreprocessing.ElapsedMilliseconds;
            Console.WriteLine("Preprocessing time: " + elapsedMSPreprocessing);
        }

        public static void TestPerformanceQuery(int maxSettledNodes, int edgeGroupSize, int importanceType, int contractionType, int contractionSearchType, bool recalculateImportance, int maxWrongImportance)
        {
            string graphFile = @"C:\\Users\\Valentijn\\source\\repos\\ContractionHierarchies\\ContractionHierarchies\\Data\\franceRoute500.csv";
            string queryFile = @"C:\Users\Valentijn\source\repos\ContractionHierarchies\ContractionHierarchies\Data\franceRoute500Query.csv";

            // preprocess
            var ch = new ContractionHierarchie(graphFile, edgeGroupSize, importanceType, contractionType, contractionSearchType, recalculateImportance, maxSettledNodes, maxSettledNodes, maxWrongImportance);
            var watchPreprocessing = System.Diagnostics.Stopwatch.StartNew();
            ch.PreProcess();
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

        /// <summary>
        /// <para> 
        /// <paramref name="importanceType"/> 
        /// 0: simple heuristic
        /// 1: simulation
        /// <paramref name="contractionType"/>
        /// 0: BiDir speedup contraction
        /// 1: normal contraction
        /// <paramref name="contractionSearchType"/>
        /// 0: dijkstra
        /// <paramref name="recalculateImportance"/>
        /// true: check if the importance is still the smallest
        /// false: do not recalculate the importance
        /// <paramref name="maxSettledNodes"/>
        /// number of nodes that should be settled before placing a shortcut
        /// </para>
        /// </summary>
        public static void TestCorrectnessBig(int maxSettledNodes, int edgeGroupSize, int importanceType, int contractionType, int contractionSearchType, bool recalculateImportance, int maxWrongImportance)
        {
            string graphFile = @"C:\\Users\\Valentijn\\source\\repos\\ContractionHierarchies\\ContractionHierarchies\\Data\\franceRoute500.csv";
            string queryFile = @"C:\Users\Valentijn\source\repos\ContractionHierarchies\ContractionHierarchies\Data\franceRoute500QuerySmall.csv";

            // preprocess
            var ch = new ContractionHierarchie(graphFile, edgeGroupSize, importanceType, contractionType, contractionSearchType, recalculateImportance, maxSettledNodes, maxSettledNodes, maxWrongImportance);
            var watchPreprocessing = System.Diagnostics.Stopwatch.StartNew();
            ch.PreProcess();
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

            int wrongRes = 0;
            int correctRes = 0;
            // run queries

            for (int i = 0; i < fields.Count; i++)
            {
                int source = int.Parse(fields[i][0]);
                int target = int.Parse(fields[i][1]);
                float resCH = ch.QueryCH(source, target);
                float resDijkstra = ch.QueryDijkstra(source, target);
                if (resCH != resDijkstra)
                {
                    Console.WriteLine($"WRONG: from: {source} to: {target}, dijkstra: {resDijkstra}, CH: {resCH}");
                    wrongRes++;
                }
                else
                {
                    Console.WriteLine($"CORRECT from: {source} to: {target}, dijkstra: {resDijkstra}, CH: {resCH}");
                    correctRes++;
                }
            }
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
            var edgeGroupSize = 10;
            int importanceType = 1;
            int contractionType = 1;
            int contractionSearchType = 0;
            bool recalculateImportance = true;
            int maxWrongImportance = 10;
            var ch = new ContractionHierarchie(file, edgeGroupSize, importanceType, contractionType, contractionSearchType, recalculateImportance, maxSettledNodes, maxSettledNodes, maxWrongImportance);
            ch.PreProcess();
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
            string filePath = @"C:\Users\Valentijn\source\repos\ContractionHierarchies\ContractionHierarchies\Data\franceRoute500Query.csv";
            int rowCount = 10000; // number of queries to be made
            int minValue = 0;
            int maxValue = 234614; // 234614 nodes in franceRoute500.csv, 3189645 nodes in netherlands.csv

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
