using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ContractionHierarchies.DataStructures
{
    public class SearchGraph
    {
        public SearchNode[] Nodes { get; set; }
        public Edge[] Edges { get; set; }

        public SearchGraph(ProcessGraph processGraph, int numberOfEdges)
        {
            Edges = new Edge[numberOfEdges];
            Nodes = new SearchNode[processGraph.NodesSize];
            GenerateSearchGraph(processGraph);
        }

        private void GenerateSearchGraph(ProcessGraph processGraph) 
        {
            int startIndex;
            int lastIndex = 0;
            for (int i = 0; i < processGraph.NodesSize; i++) 
            {
                ProcessNode oldNode = processGraph.Nodes[i];
                int firstEdge = oldNode.FirstIndex;
                int lastEdge = oldNode.LastIndex;
                int nodeLevel = oldNode.NodeLevel;
                startIndex = lastIndex;
                // add edges
                for (int j = firstEdge; j <= lastEdge; j++)
                {
                    Edge oldEdge = processGraph.Edges[j];
                    if (oldEdge.Target == -1)
                    {
                        continue;
                    }
                    // check if edge is in upward graph or reverse downward graph
                    if (processGraph.Nodes[oldEdge.Target].NodeLevel > nodeLevel)
                    {
                        Edges[lastIndex] = oldEdge;
                        lastIndex++;
                    }
                }
                // add node
                Nodes[i] = new SearchNode(i, startIndex, lastIndex - 1);
            }
        }

        public void PrintSearchGraph()
        {
            Console.WriteLine("SearchGraph:");
            for ( int i = 0; i < Nodes.Length; i++)
            {
                SearchNode node = Nodes[i];
                Console.WriteLine("Node: " + node.ID + " distance: " + node.Distance + " settledFroward: " + node.SettledForward + " settledBackward: " + node.SettledBackward);
                int startEdges = node.FirstIndex;
                int endEdges = node.LastIndex;
                for ( int j = startEdges; j <= endEdges; j++)
                {
                    Edge edge = Edges[j];
                    Console.WriteLine("target: " + edge.Target + " weight: " + edge.Weight + " forward: " + edge.Forward + " backward: " + edge.Backward);
                }
            }
        }
    }
}
