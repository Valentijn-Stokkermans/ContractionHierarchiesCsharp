using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ContractionHierarchies.DataStructures
{

    public struct Node
    {
        public Node(int index) 
        {
            this.startIndex = index;
            this.lastIndex = index;
            contracted = false;
            searchTarget = false;
        }

        // forward edges start index
        public int startIndex;
        // last edge index
        public int lastIndex;
        // flag contracted
        public bool contracted;
        // flag search target
        public bool searchTarget;
    }

}
