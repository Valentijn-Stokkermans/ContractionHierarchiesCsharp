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
        List<Node> nodes;
        List<Edge> edges;

        public ProcessGraph(string inputCSV) 
        {
            generateGraph(inputCSV);
        }

        public void generateGraph(string inputCSV)
        {
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

            // used to count unique nodes
            HashSet<int> countNodes = new HashSet<int>();

            // count nodes
            for (int i = 0; i < fields.Count(); i++)
            {
                if(countNodes.Add(int.Parse(fields[i][0])));
            }

            // initialize the nodes and edges lists.
            nodes = new List<Node>(countNodes.Count);
            for (int i = 0; i < countNodes.Count; i++)
            {
                nodes[i] = new Node(i);
            }
            edges = new List<Edge>(countNodes.Count * 6); // normally max 4 edges per node for both directions, 2 spaces left for shortcuts
            
            for (int i = 0; i < countNodes.Count * 6; i++)
            {
                edges[i] = new Edge();
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

            // first edge of node exception
            if (edges[node.lastIndex].target == -1)
            {
                edges[node.lastIndex] = new Edge(weight, target, forward, !forward);
                return;
            }

            // check if there is still room
            if (edges[node.lastIndex + 1].target == -1) 
            { 
                // check if not init phase and over the max 6 initial spots
                if (!(init && node.startIndex + 5 == node.lastIndex))
                {
                    edges[node.lastIndex + 1] = new Edge();
                    node.lastIndex++;
                    return;
                }
            }

            // if no free spot is found transfer all edges to a new area at the end of the list, old area is free space
            int newStart = edges.Count;
            int count = 1 + node.lastIndex - node.startIndex;
            edges.AddRange(edges.GetRange(node.startIndex, count));
            edges.Add(new Edge(weight, target, forward, !forward));
            // create new extra space, dubble the original size of the edge group
            for (int i = 0; i < node.lastIndex - node.startIndex; i++)
            {
                edges.Add(new Edge());
            }
            // update node indexes
            node.startIndex = newStart;
            node.lastIndex = newStart + count - 1;
            return;
        }

    }
}
