using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ContractionHierarchies.DataStructures
{

    public class Node : FastPriorityQueueNode
    {
        public Node(int index) 
        {
            this.startIndex = index;
            this.lastIndex = index - 1;
            contracted = false;
            searchTarget = false;
            nodeLevel = 0;
        }

        // forward edges start index
        public int startIndex;
        // last edge index
        public int lastIndex;
        // flag contracted
        public bool contracted;
        // flag search target
        public bool searchTarget;
        // node level, order of contraction
        public int nodeLevel;
        // node id, index in nodes array
        public int id;
    }

    public class CurrentNode
    {
        public CurrentNode(Node node, float edgeWeight) 
        { 
            this.node = node;
            distance = edgeWeight;
        }
        public Node node;
        public float distance = 0;
    }

    class CurrentNodeEqualityComparer : IEqualityComparer<CurrentNode> 
    {
        public bool Equals(CurrentNode x, CurrentNode y) 
        { 
            return x.node == y.node;
        }

        public int GetHashCode(CurrentNode obj) 
        { 
            return obj.node.GetHashCode();
        }
    }
}
