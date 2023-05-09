using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContractionHierarchies.DataStructures
{
    internal class SearchGraph
    {
        public List<Node> nodes { get; set; }
        public List<Edge> edges { get; set; }
        public SearchGraph(ProcessGraph processGraph) 
        {
            edges = new List<Edge>();
            generateSearchGraph(processGraph);
            printSearchGraph();
        }

        private void generateSearchGraph(ProcessGraph processGraph) 
        {
            int startIndex = 0;
            int lastIndex = 0;
            nodes = new List<Node>(processGraph.nodesSize());
            for (int i = 0; i < processGraph.nodesSize(); i++) 
            {
                Node oldNode = processGraph.nodes[i];
                int firstEdge = oldNode.startIndex;
                int lastEdge = oldNode.lastIndex;
                int nodeLevel = oldNode.nodeLevel;
                startIndex = lastIndex;
                // add edges
                for (int j = firstEdge; j <= lastEdge; j++)
                {
                    Edge oldEdge = processGraph.edges[j];
                    if (oldEdge.target == -1)
                    {
                        continue;
                    }
                    // check if edge is in upward graph or reverse downward graph
                    if (processGraph.nodes[oldEdge.target].nodeLevel > nodeLevel)
                    {
                        edges.Add(oldEdge);
                        lastIndex++;
                    }
                }
                // add node
                nodes.Add(new Node(i, startIndex, lastIndex - 1, oldNode.nodeLevel));
            }
        }

        public void printSearchGraph()
        {
            Console.WriteLine("SearchGraph:");
            for ( int i = 0; i < nodes.Count; i++)
            {
                Node node = nodes[i];
                Console.WriteLine("Node: " + node.id + " imp: " + node.nodeLevel);
                int startEdges = node.startIndex;
                int endEdges = node.lastIndex;
                for ( int j = startEdges; j <= endEdges; j++)
                {
                    Edge edge = edges[j];
                    Console.WriteLine(edge.target);
                }
            }
        }
    }
}
