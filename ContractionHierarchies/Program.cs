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
        int importanceType = 0;
        int contractionType = 1;
        int contractionSearchType = 0;
        bool recalculateImportance = false;

        //Testing.TestPerformance(maxSettledNodes, edgeGroupSize, importanceType, contractionType, contractionSearchType, recalculateImportance);
        Testing.TestCorrectness();
    }

} 