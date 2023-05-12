using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContractionHierarchies;
using ContractionHierarchies.DataStructures;
using Microsoft.VisualBasic.FileIO;
using Priority_Queue;
using static System.Runtime.InteropServices.JavaScript.JSType;

class Program
{
     static void Main(string[] args)
     {
        var maxSettledNodes = 100;
        var edgeGroupSize = 10;

        TestPerformance(maxSettledNodes, edgeGroupSize);
    }

    static void TestPerformance(int maxSettledNodes, int edgeGroupSize)
    {
        string graphFile = @"C:\\Users\\Valentijn\\source\\repos\\ContractionHierarchies\\ContractionHierarchies\\Data\\roads.csv";
        string queryFile = @"C:\Users\Valentijn\source\repos\ContractionHierarchies\ContractionHierarchies\Data\CSVQuery.csv";

        // preprocess
        var watchPreprocessing = System.Diagnostics.Stopwatch.StartNew();
        var ch = new ContractionHierarchie(graphFile, edgeGroupSize);
        ch.PreProcess(1, 0, maxSettledNodes);
        ch.CreateSearchGraph();
        watchPreprocessing.Stop();
        long elapsedMSPreprocessing = watchPreprocessing.ElapsedMilliseconds;

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
        Console.WriteLine("Preprocessing time: " + elapsedMSPreprocessing);

        // run queries
        var watchQuery = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < fields.Count; i++)
        {
            int source = int.Parse(fields[i][0]);
            int target = int.Parse(fields[i][1]);

            ch.Query(source, target);
        }
        watchQuery.Stop();
        long elapsedMSQuery = watchQuery.ElapsedMilliseconds;
        Console.WriteLine("Total query time: " + elapsedMSQuery + " over " + fields.Count + " queries, Average: " + (double)elapsedMSQuery/fields.Count);
    }
} 