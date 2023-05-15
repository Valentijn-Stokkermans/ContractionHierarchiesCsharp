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
    internal static class Test
    {
        

        public static int random_except_list(int n, HashSet<int> x)
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
