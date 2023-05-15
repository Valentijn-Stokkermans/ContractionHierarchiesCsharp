using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ContractionHierarchies.DataStructures
{
    public class ProcessGraph
    {
        public ProcessNode[] Nodes { get; set; }
        public List<Edge> Edges { get; set; }
        int EdgeGroupSize { get; set; }

        public ProcessGraph(string inputCSV, int edgeGroupSize) 
        {
            EdgeGroupSize = edgeGroupSize;
            MakeGraphFromCSV(inputCSV);
            PrintProcessGraph();
        }

        public void MakeGraphFromCSV(string inputCSV)
        {
            // expects a csv file with source_id, target_id, weight
            // with id's starting at 0

            // read all lines and store temporarily in fields
            List<string[]> fields = new List<string[]> { };
            using (TextFieldParser parser = new TextFieldParser(inputCSV))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                while (!parser.EndOfData)
                {
                    //Processing row
                    fields.Add(parser.ReadFields());
                }
            }
            fields.RemoveAt(0); // drop column description

            // count nodes
            int largestNode = 0;
            for (int i = 0; i < fields.Count(); i++)
            {
                int source = int.Parse(fields[i][0]);
                if (source > largestNode)
                {
                    largestNode = source;
                }
                int target = int.Parse(fields[i][1]);
                if (target > largestNode)
                {
                    largestNode = target;
                }
            }

            // initialize the nodes and edges lists.
            Nodes = new ProcessNode[largestNode+1];
            Edges = new List<Edge>(largestNode * EdgeGroupSize);
            for (int i = 0; i <= largestNode; i++)
            {
                Nodes[i] = new ProcessNode(i, i*EdgeGroupSize);
                for (int j = 0; j < EdgeGroupSize; j++)
                {
                    Edges.Add(new Edge());
                }
            }

            // fill nodes and edges lists
            for (int i = 0; i < fields.Count; i++)
            {
                int source = int.Parse(fields[i][0]);
                int target = int.Parse(fields[i][1]);
                float weight = float.Parse(fields[i][2]);

                AddEdgeToProcessNode(Nodes[source], weight, target, true, false, true); // add forward edge
                AddEdgeToProcessNode(Nodes[target], weight, source, false, true, true); // add backward edge
            }
        }

        public void AddEdgeToProcessNode(ProcessNode node, float weight, int target, bool forward, bool backward, bool init) 
        {
            // check if edge is already present in other direction
            for (int i = node.FirstIndex; i <= node.LastIndex; i++)
            {
                Edge edge = Edges[i];
                if (edge.Target == target)
                {
                    // add other direction to existing edge
                    if (forward) 
                    { 
                        edge.Forward = true;
                    }
                    if (backward)
                    {
                        edge.Backward = true;
                    }
                    Edges[i] = edge;
                    return;
                }
            }

            // check if there is still room
            if (Edges[node.LastIndex + 1].Target == -1) 
            {
                // check if init phase
                if (init)
                {
                    // check if not over the max edgeGroupSize initial spots
                    if (node.LastIndex < node.FirstIndex + EdgeGroupSize - 1)
                    {
                        Edges[node.LastIndex + 1] = new Edge(weight, target, forward, backward);
                        node.LastIndex++;
                        return;
                    }
                } 
                else 
                {
                    Edges[node.LastIndex + 1] = new Edge(weight, target, forward, backward);
                    node.LastIndex++;
                    return;
                }
            }

            // if no free spot is found transfer all edges to a new area at the end of the list, old area is free space
            int newStart = Edges.Count;
            int numberOfEdges = 1 + node.LastIndex - node.FirstIndex;
            Console.WriteLine("transfer: " + numberOfEdges + " edges");
            // transfer edges
            Edges.AddRange(Edges.GetRange(node.FirstIndex, numberOfEdges));
            // add new edge
            Edges.Add(new Edge(weight, target, forward, backward));
            // create new extra space, dubble the original size of the edge group
            for (int i = 0; i <= numberOfEdges; i++)
            {
                Edges.Add(new Edge());
            }
            // clear old edges
            for (int i = node.FirstIndex; i <= node.LastIndex; i++)
            {
                Edges[i] = new Edge();
            }
            // update node indexes
            node.FirstIndex = newStart;
            node.LastIndex = newStart + numberOfEdges;
            return;
        }

        public int NodesSize { get
            {
                return Nodes.Length;
            } 
        }

        public int EdgesSize { get
            {
                int edges = 0;
                for (int i = 0; i < NodesSize; i++)
                {
                    ProcessNode oldNode = Nodes[i];
                    int firstEdge = oldNode.FirstIndex;
                    int lastEdge = oldNode.LastIndex;
                    int nodeLevel = oldNode.NodeLevel;
                    // add edges
                    for (int j = firstEdge; j <= lastEdge; j++)
                    {
                        Edge oldEdge = Edges[j];
                        if (oldEdge.Target == -1)
                        {
                            continue;
                        }
                        // check if edge is in upward graph or reverse downward graph
                        if (Nodes[oldEdge.Target].NodeLevel > nodeLevel)
                        {
                            edges++;
                        }
                    }
                }
                return edges;
            }
        }

        public void PrintProcessGraph()
        {
            Console.WriteLine("ProcessGraph:");
            for (int i = 0; i < Nodes.Length; i++)
            {
                ProcessNode node = Nodes[i];
                Console.WriteLine("Node: " + node.ID);
                int startEdges = node.FirstIndex;
                int endEdges = node.LastIndex;
                for (int j = startEdges; j <= endEdges; j++)
                {
                    Edge edge = Edges[j];
                    if (edge.Forward)
                        Console.WriteLine("target: " + edge.Target + " weight: " + edge.Weight + " forward: " + edge.Forward + " backward: " + edge.Backward);
                }
            }
        }
    }
}
