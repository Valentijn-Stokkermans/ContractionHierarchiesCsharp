using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContractionHierarchies;
using ContractionHierarchies.DataStructures;
using Microsoft.VisualBasic.FileIO;
using OsmSharp;
using static System.Runtime.InteropServices.JavaScript.JSType;

class Program
{
     static void Main(string[] args)
     {
        var maxSettledNodesImportance = 10;
        var maxSettledNodesContraction = 10;
        var edgeGroupSize = 10;
        int importanceType = 0;
        int contractionType = 1;
        int contractionSearchType = 0;
        bool recalculateImportance = true;
        int maxWrongImportance = 5;

        Testing.TestPerformancePreProcess(maxSettledNodesImportance, maxSettledNodesContraction, edgeGroupSize, importanceType, contractionType, contractionSearchType, recalculateImportance, maxWrongImportance);
        //Testing.TestCorrectnessBig(maxSettledNodes, edgeGroupSize, importanceType, contractionType, contractionSearchType, recalculateImportance, maxWrongImportance);
        //Testing.CreateCSVQueries();
        //Testing.TestCorrectness();
    }
} 