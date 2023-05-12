using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using ContractionHierarchies;
using ContractionHierarchies.DataStructures;

namespace Testing
{
    internal class Test
    {
        public void TestCorrectness(int size)
        {
            GraphGen newGraph = new GraphGen(size);
            newGraph.genMatrix();
            var file = "C:\\Users\\Valentijn\\source\\repos\\ContractionHierarchies\\ContractionHierarchies\\Data\\small_directed_graph.csv";
            List<string[]> fields = new List<string[]> { };
            using (TextFieldParser parser = new TextFieldParser(file))
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

            HashSet<int> vertices = new HashSet<int>();

            // count unique vertices
            for (int i = 0; i < fields.Count(); i++)
            {
                vertices.Add(int.Parse(fields[i][0]));
            }

            Console.WriteLine(vertices.Count);

            double[,] adjGraph = new double[vertices.Count(), vertices.Count()];

            //get edges
            for (int j = 0; j < fields.Count(); j++)
            {
                int from, to;
                double weight;
                from = int.Parse(fields[j][0]);
                to = int.Parse(fields[j][1]);
                weight = double.Parse(fields[j][2]);

                adjGraph[from, to] = weight;
            }

            var watchTotal = System.Diagnostics.Stopwatch.StartNew();
            var watchPrepCH = System.Diagnostics.Stopwatch.StartNew();

            //preprocessing stage.
            var maxSettledNodes = 100;
            var edgeGroupSize = 10;
            var ch = new ContractionHierarchie(file, edgeGroupSize);
            ch.PreProcess(1, 0, maxSettledNodes);
            ch.CreateSearchGraph();

            watchPrepCH.Stop();
            long elapsedMSPrepCH = watchPrepCH.ElapsedMilliseconds;

            double[,] adjGraphTemp = adjGraph;

            int q = 0;
            long elapsedMsDijkstra = 0;
            long elapsedMsCH = 0;

            Dijkstra t = new Dijkstra(vertices.Count);
            int wrongRes = 0;
            int correctRes = 0;

            for (int i = 0; i < vertices.Count; i++)
            {
                double[] dist;

                var watchDijkstra = System.Diagnostics.Stopwatch.StartNew();
                dist = t.calcDijkstra(adjGraphTemp, i);
                watchDijkstra.Stop();
                elapsedMsDijkstra += watchDijkstra.ElapsedMilliseconds;
                var watchCH = System.Diagnostics.Stopwatch.StartNew();
                for (int j = 0; j < vertices.Count; j++)
                {
                    //acutal distance computation stage.
                    float res = ch.Query(i, j);
                    q++;
                    if (dist[j] != res)
                    {
                        Console.WriteLine($"\n\n\n\n\n\n\n\n\n\n\nWRONG: from: {i} to: {j}, dijkstra: {dist[j]}, CH: {res}\n\n\n\n\n\n\n\n\n");
                        wrongRes++;
                    }
                    else
                    {
                        Console.WriteLine($"CORRECT from: {i} to: {j}, dijkstra: {dist[j]}, CH: {res}");
                        correctRes++;
                    }
                }
                watchCH.Stop();
                elapsedMsCH = elapsedMsCH + watchCH.ElapsedMilliseconds;
            }
            watchTotal.Stop();
            Console.WriteLine($"Total Time: Dijkstra: {elapsedMsDijkstra}, PrepCH: {elapsedMSPrepCH}, CH: {elapsedMsCH}, total: {watchTotal.ElapsedMilliseconds}");
            Console.WriteLine($"One Query Time: Dijkstra: {(double)elapsedMsDijkstra / vertices.Count}, CH: {(double)elapsedMsCH / (vertices.Count * vertices.Count)}");
            Console.WriteLine("Number of wrong results: " + wrongRes);
            Console.WriteLine("Number of correct results: " + correctRes);
        }

        public void TestCorrectnessSmallDirectedGraph()
        {
            var file = "C:\\Users\\Valentijn\\source\\repos\\ContractionHierarchies\\ContractionHierarchies\\Data\\small_directed_graph.csv";

            var watchTotal = System.Diagnostics.Stopwatch.StartNew();
            var watchPrepCH = System.Diagnostics.Stopwatch.StartNew();

            //preprocessing stage.
            var maxSettledNodes = 100;
            var edgeGroupSize = 10;
            var ch = new ContractionHierarchie(file, edgeGroupSize);
            ch.PreProcess(1, 0, maxSettledNodes);
            ch.CreateSearchGraph();

            watchPrepCH.Stop();
            long elapsedMSPrepCH = watchPrepCH.ElapsedMilliseconds;

            int q = 0;
            long elapsedMsDijkstra = 0;
            long elapsedMsCH = 0;

            for (int i = 0; i < 9; i++)
            {
                var watchDijkstra = System.Diagnostics.Stopwatch.StartNew();
                watchDijkstra.Stop();
                elapsedMsDijkstra += watchDijkstra.ElapsedMilliseconds;
                var watchCH = System.Diagnostics.Stopwatch.StartNew();
                for (int j = 0; j < 9; j++)
                {
                    //acutal distance computation stage.
                    float res = ch.Query(i, j);
                    Console.WriteLine($"from: {i} to: {j}, CH: {res}");
                }
                watchCH.Stop();
                elapsedMsCH += watchCH.ElapsedMilliseconds;
            }
            watchTotal.Stop();
            Console.WriteLine($"Total Time: Dijkstra: {elapsedMsDijkstra}, PrepCH: {elapsedMSPrepCH}, CH: {elapsedMsCH}, total: {watchTotal.ElapsedMilliseconds}");
            //Console.WriteLine("Number of wrong results: " + wrongRes);
            //Console.WriteLine("Number of correct results: " + correctRes);
        }

        public void testExample()
        {
            List<string[]> fields = new List<string[]> { };
            using (TextFieldParser parser = new TextFieldParser(@"C:\Users\valentijn\source\repos\GraphTest\Valentijn\data\roads.csv"))
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

            HashSet<int> vertices = new HashSet<int>();

            // count unique vertices
            for (int i = 0; i < fields.Count(); i++)
            {
                vertices.Add(int.Parse(fields[i][0]));
            }

            //get edges
            for (int j = 0; j < fields.Count(); j++)
            {
                int from, to;
                double weight;
                from = int.Parse(fields[j][0]);
                to = int.Parse(fields[j][1]);
                weight = double.Parse(fields[j][2]);
            }

            var watchPrepCH = System.Diagnostics.Stopwatch.StartNew();
            Console.WriteLine("Prep");
            //preprocessing stage.
            //PreProcess process = new PreProcess();
            //int[] nodeOrdering = process.processing(graph);
            watchPrepCH.Stop();
            long elapsedMSPrepCH = watchPrepCH.ElapsedMilliseconds;

            //Vertex[] graphTemp = graph;
            //BidirectionalDijkstra bd = new BidirectionalDijkstra();
            int q = 0;
            Console.WriteLine("Compute");
            var watchCH = System.Diagnostics.Stopwatch.StartNew();
            Random r = new Random();
            int a = r.Next(0, 10);
            int b = random_except_list(10, new HashSet<int> { a });
            //acutal distance computation stage.
            //graphTemp = graph;
            //double res = bd.computeDist(graphTemp, a, b, q, nodeOrdering);
            q++;
            watchCH.Stop();
            long elapsedMsCH = watchCH.ElapsedMilliseconds;

            Console.WriteLine($"Total Time: PrepCH: {elapsedMSPrepCH} ms, CH: {elapsedMsCH} ms");
        }

        public void testPerformance(int size, int runs)
        {
            GraphGen newGraph = new GraphGen(size);
            newGraph.genMatrix();

            List<string[]> fields = new List<string[]> { };
            using (TextFieldParser parser = new TextFieldParser(@"C:\Users\valentijn\source\repos\GraphTest\Valentijn\data\new_graph.csv"))
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

            HashSet<int> vertices = new HashSet<int>();

            // count unique vertices
            for (int i = 0; i < fields.Count(); i++)
            {
                vertices.Add(int.Parse(fields[i][0]));
            }

            //get edges
            for (int j = 0; j < fields.Count(); j++)
            {
                int from, to;
                double weight;
                from = int.Parse(fields[j][0]);
                to = int.Parse(fields[j][1]);
                weight = double.Parse(fields[j][2]);
            }

            var watchPrepCH = System.Diagnostics.Stopwatch.StartNew();
            Console.WriteLine("Prep");
            //preprocessing stage.
            watchPrepCH.Stop();
            long elapsedMSPrepCH = watchPrepCH.ElapsedMilliseconds;

            int q = 0;
            Console.WriteLine("Compute");
            var watchCH = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < runs; i++)
            {
                Random r = new Random();
                int a = r.Next(0, size);
                int b = random_except_list(size, new HashSet<int> { a });
                //acutal distance computation stage.
                q++;
            }
            watchCH.Stop();
            long elapsedMsCH = watchCH.ElapsedMilliseconds;

            Console.WriteLine($"Total Time: PrepCH: {elapsedMSPrepCH} ms, CH: {elapsedMsCH} ms");
            Console.WriteLine($"Average Query Time over {runs} querys: CH: {(double)elapsedMsCH / runs} ms");
        }

        public int random_except_list(int n, HashSet<int> x)
        {
            Random r = new Random();
            int result = 0;
            do
            {
                result = r.Next(0, n);
            } while (x.Contains(result));
            return result;
        }
    }
}
