using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContractionHierarchies;
using ContractionHierarchies.DataStructures;
using Priority_Queue;

class Program
{
     static void Main(string[] args)
     {
        var file = "C:\\Users\\Valentijn\\source\\repos\\ContractionHierarchies\\ContractionHierarchies\\Data\\example_graph.csv";
        var maxSettledNodes = 100;
        var edgeGroupSize = 10;

        var watchPrepCH = System.Diagnostics.Stopwatch.StartNew();
        var ch = new ContractionHierarchie(file, edgeGroupSize);
        ch.PreProcess(1, 0, maxSettledNodes);
        ch.CreateSearchGraph();
        Console.WriteLine(ch.Query(8, 1));
        watchPrepCH.Stop();
        long elapsedMSPrepCH = watchPrepCH.ElapsedMilliseconds;
        Console.WriteLine("time:" + elapsedMSPrepCH);
    }
} 