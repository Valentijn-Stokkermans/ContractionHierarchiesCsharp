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
        public Node(int nodeIndex, int edgeIndex) 
        {
            this.startIndex = edgeIndex;
            this.lastIndex = edgeIndex - 1;
            contracted = false;
            searchTarget = false;
            nodeLevel = 0;
            id = nodeIndex;

        }

        public Node(int nodeIndex, int startIndex, int lastIndex, int nodeLevel)
        {
            this.startIndex = startIndex;
            this.lastIndex = lastIndex;
            contracted = false;
            searchTarget = false;
            this.nodeLevel = nodeLevel;
            id = nodeIndex;
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
        // settled in dijkstra
        public bool settled = false;
        // distance in dijkstra from source
        public float distance = 0;
    }

    public class CurrentNode
    {
        public CurrentNode(Node node, float edgeWeight, bool forward) 
        { 
            this.node = node;
            distance = edgeWeight;
            this.forward = forward;
        }
        public Node node;
        public float distance = 0;
        public bool forward;

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
