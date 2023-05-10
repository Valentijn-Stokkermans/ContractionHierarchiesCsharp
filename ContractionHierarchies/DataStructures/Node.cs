﻿using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ContractionHierarchies.DataStructures
{

    public struct SearchNode 
    {

        public SearchNode(int nodeIndex, int firstIndex, int lastIndex)
        {
            FirstIndex = firstIndex;
            LastIndex = lastIndex;
            ID = nodeIndex;
        }

        // forward edges start index
        public int FirstIndex { get; set; }
        // last edge index
        public int LastIndex { get; set; }
        public int ID { get; set; }

        // settled in dijkstra
        public bool SettledForward { get; set; } = false;
        public bool SettledBackward { get; set; } = false;
        // distance in dijkstra from source
        public float Distance { get; set; } = 0;
    }

    public class ProcessNode : FastPriorityQueueNode
    {
        public ProcessNode(int nodeIndex, int firstIndex)
        {
            FirstIndex = firstIndex;
            LastIndex = firstIndex - 1;
            ID = nodeIndex;
        }

        public ProcessNode(int nodeIndex, int firstIndex, int lastIndex)
        {
            FirstIndex = firstIndex;
            LastIndex = lastIndex;
            ID = nodeIndex;
        }

        // forward edges start index
        public int FirstIndex { get; set; }
        // last edge index
        public int LastIndex { get; set; }
        // node index in processgraph nodes list
        public int ID { get; set; }

        // flag contracted
        public bool Contracted { get; set; } = false;
        // flag search target
        public bool SearchTarget { get; set; } = false;
        // node level, order of contraction
        public int NodeLevel { get; set; } = 0;
        // node id, index in nodes array
    }

    public class CurrentNode
    {
        public CurrentNode(ProcessNode node, float edgeWeight) 
        { 
            Node = node;
            Distance = edgeWeight;
        }

        public ProcessNode Node { get; set; }
        public float Distance { get; set; }
    }

    class CurrentNodeEqualityComparer : IEqualityComparer<CurrentNode> 
    {
        public bool Equals(CurrentNode x, CurrentNode y) 
        { 
            return x.Node.ID == y.Node.ID;
        }

        public int GetHashCode(CurrentNode obj) 
        { 
            return obj.Node.GetHashCode();
        }
    }

    class SearchNodeEqualityComparer : IEqualityComparer<SearchNode>
    {
        public bool Equals(SearchNode x, SearchNode y)
        {
            return x.ID == y.ID;
        }

        public int GetHashCode(SearchNode obj)
        {
            return obj.GetHashCode();
        }
    }
}
