using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ContractionHierarchies.Experiments;
using static System.Net.Mime.MediaTypeNames;

namespace ContractionHierarchies
{
    public static class Experiments
    {
        public class Result
        {
            public string Graph { get; set; } = "";
            public int Shortcuts { get; set; }
            public double AvgQueryTime { get; set; }
            public double ConstructionTime { get; set; }
        }

        public static void CppComparison() 
        {
            var maxSettledNodesImportance = 10;
            var maxSettledNodesContraction = 10;
            var edgeGroupSize = 10;
            int importanceType = 0;
            int contractionType = 1;
            int contractionSearchType = 0;

            string workingDirectory = Environment.CurrentDirectory;
            string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;

            string outputFile = projectDirectory + "\\Data\\CppComparison.csv";

            var results = Experiment(outputFile, maxSettledNodesImportance, maxSettledNodesContraction, edgeGroupSize, importanceType, contractionType, contractionSearchType);
            
            var csv = new StringBuilder();

            foreach ( var row in results )
            {
                var newLine = $"{row.Graph},{row.Shortcuts},{row.AvgQueryTime},{row.ConstructionTime}";
                csv.AppendLine(newLine);
            }

            File.WriteAllText(outputFile, csv.ToString());
        }

        public static Result[] Experiment(string outputFile, int maxSettledNodesImportance, int maxSettledNodesContraction, int edgeGroupSize, int importanceType, int contractionType, int contractionSearchType)
        {
            string flevolandQuery = @"C:\\Users\\Valentijn\\source\\repos\\ContractionHierarchies\\ContractionHierarchies\\Data\\flevolandQuery.csv";
            string zeelandQuery = @"C:\\Users\\Valentijn\\source\\repos\\ContractionHierarchies\\ContractionHierarchies\\Data\\zeelandQuery.csv";
            string guadeloupeQuery = @"C:\\Users\\Valentijn\\source\\repos\\ContractionHierarchies\\ContractionHierarchies\\Data\\guadeloupeQuery.csv";
            string corseQuery = @"C:\\Users\\Valentijn\\source\\repos\\ContractionHierarchies\\ContractionHierarchies\\Data\\corseQuery.csv";

            string flevoland_ddsg = @"C:\\Users\\Valentijn\\source\\repos\\ContractionHierarchies\\ContractionHierarchies\\Data\\flevoland_ddsg.csv";
            string zeeland_ddsg = @"C:\\Users\\Valentijn\\source\\repos\\ContractionHierarchies\\ContractionHierarchies\\Data\\zeeland_ddsg.csv";
            string guadeloupe_ddsg = @"C:\\Users\\Valentijn\\source\\repos\\ContractionHierarchies\\ContractionHierarchies\\Data\\guadeloupe_ddsg.csv";
            string corse_ddsg = @"C:\\Users\\Valentijn\\source\\repos\\ContractionHierarchies\\ContractionHierarchies\\Data\\corse_ddsg.csv";

            List<string> queries = new()
            {
                flevolandQuery, zeelandQuery, guadeloupeQuery, corseQuery 
            };

            List<string> graphs = new()
            {
                flevoland_ddsg, zeeland_ddsg, guadeloupe_ddsg, corse_ddsg
            };

            Result[] results = new Result[queries.Count];

            for (int i = 0; i < queries.Count; i++)
            {
                results[i] = TestPerformanceQuery(outputFile, graphs[i], queries[i], maxSettledNodesImportance, maxSettledNodesContraction, edgeGroupSize, importanceType, contractionType, contractionSearchType);
            }
            return results;
        }
        

        public static Result TestPerformanceQuery(string outputFile, string graphFile, string queryFile, int maxSettledNodesImportance, int maxSettledNodesContraction, int edgeGroupSize, int importanceType, int contractionType, int contractionSearchType)
        {
            string graphFileName = Path.GetFileName(graphFile);
            string graphName = graphFileName.Split('_')[0];
            Console.WriteLine(graphName);

            // preprocess
            var ch = new ContractionHierarchie(graphFile, edgeGroupSize, importanceType, contractionType, contractionSearchType, maxSettledNodesImportance, maxSettledNodesContraction);
            var watchPreprocessing = System.Diagnostics.Stopwatch.StartNew();
            int shortcuts = ch.PreProcess();
            watchPreprocessing.Stop();
            long elapsedMSPreprocessing = watchPreprocessing.ElapsedMilliseconds;
            ch.CreateSearchGraph();

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
            var elapsedMSQuery = watchQuery.ElapsedMilliseconds;

            return new Result() { 
                Graph = graphName,
                AvgQueryTime = (double)elapsedMSQuery / fields.Count, 
                ConstructionTime = elapsedMSPreprocessing, 
                Shortcuts = shortcuts 
            };
        }

        

    }
}
