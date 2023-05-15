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
        var maxSettledNodes = 500;
        var edgeGroupSize = 2;
        int importanceType = 1;
        int contractionType = 1;
        int contractionSearchType = 0;
        bool recalculateImportance = true;
        int maxWrongImportance = 200;

        //Testing.TestPerformance(maxSettledNodes, edgeGroupSize, importanceType, contractionType, contractionSearchType, recalculateImportance);
        //Testing.TestCorrectnessBig(maxSettledNodes, edgeGroupSize, importanceType, contractionType, contractionSearchType, recalculateImportance, maxWrongImportance);
        //Testing.CreateCSVQueries();
        Testing.TestCorrectness();
    }
} 