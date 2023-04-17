using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContractionHierarchies.DataStructures
{
    internal class ProcessGraph
    {
        public List<Node> nodes { get; set; }
        public List<Edge> edges { get; set; }
        int edgeGroupSize { get; set; }

        public ProcessGraph(string inputCSV, int edgeGroupSize) 
        {
            this.edgeGroupSize = edgeGroupSize;
            makeGraphFromCSV(inputCSV);
        }

        public void makeGraphFromCSV(string inputCSV)
        {
            // expects a csv file with source_id, target_id, weight
            // with node id's starting at 0

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
                int node = int.Parse(fields[i][0]);
                if (node > largestNode)
                {
                    largestNode = node;
                }
            }

            // initialize the nodes and edges lists.
            nodes = new List<Node>(largestNode);
            edges = new List<Edge>(largestNode * edgeGroupSize); // normally max 4 edges per node for both directions, 2 spaces left for shortcuts
            for (int i = 0; i <= largestNode; i++)
            {
                nodes.Add(new Node(i));
                for (int j = 0; j < edgeGroupSize; j++)
                {
                    edges.Add(new Edge());
                }
            }

            // fill nodes and edges lists
            for (int i = 0; i < fields.Count(); i++)
            {
                var source = int.Parse(fields[i][0]);
                var target = int.Parse(fields[i][1]);
                var weight = double.Parse(fields[i][2]);

                addEdge(source, nodes[source], weight, target, true, true); // add forward edge
                addEdge(target, nodes[target], weight, source, false, true); // add backward edge
            }
        }

        public void addEdge(int source, Node node, double weight, int target, bool forward, bool init) 
        {
            // check if edge is already present in other direction
            for (int i = node.startIndex; i <= node.lastIndex; i++)
            {
                if (edges[i].target == target)
                {
                    // add other direction to existing edge
                    Edge edge = edges[i];
                    if (forward) 
                    { 
                        edge.forward = true;
                    }
                    else
                    {
                        edge.backward = true;
                    }
                    edges[i] = edge;
                    return;
                }
            }

            // check if there is still room
            if (edges[node.lastIndex + 1].target == -1) 
            {
                // check if init phase
                if (init)
                {
                    // check if not over the max edgeGroupSize initial spots
                    if (node.lastIndex < node.startIndex + edgeGroupSize - 1)
                    {
                        edges[node.lastIndex + 1] = new Edge();
                        node.lastIndex++;
                        return;
                    }
                } 
                else 
                {
                    edges[node.lastIndex + 1] = new Edge();
                    node.lastIndex++;
                    return;
                }
            }

            // if no free spot is found transfer all edges to a new area at the end of the list, old area is free space
            int newStart = edges.Count;
            int numberOfEdges = 1 + node.lastIndex - node.startIndex;
            // transfer edges
            edges.AddRange(edges.GetRange(node.startIndex, numberOfEdges));
            // add new edge
            edges.Add(new Edge(weight, target, forward, !forward));
            // create new extra space, dubble the original size of the edge group
            for (int i = 0; i <= numberOfEdges; i++)
            {
                edges.Add(new Edge());
            }
            // clear old edges
            for (int i = node.startIndex; i <= node.lastIndex; i++)
            {
                edges[i] = new Edge();
            }
            // update node indexes
            node.startIndex = newStart;
            node.lastIndex = newStart + numberOfEdges - 1;
            return;
        }

        public int nodesSize()
        {
            return nodes.Count;
        }
    }
}
