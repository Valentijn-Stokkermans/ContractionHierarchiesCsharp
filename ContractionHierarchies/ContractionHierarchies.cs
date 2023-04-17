using ContractionHierarchies.DataStructures;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ContractionHierarchies
{
    internal class ContractionHierarchies
    {
        SearchGraph searchGraph { get; set; }
        ProcessGraph processGraph { get; set; }
        FastPriorityQueue<Node> priorityQueue { get; set; }
        int edgeGroupSize { get; set; } = 6 ;

        public ContractionHierarchies(string inputFile)
        {
            processGraph = new ProcessGraph(inputFile, edgeGroupSize);
            priorityQueue = new FastPriorityQueue<Node>(processGraph.nodesSize());
        }

        public void preprocess(int chooseImportance, int chooseContraction, int maxSettledNodes)
        {
            // calculate importance for each node and fill priority queue
            for (int i = 0; i < processGraph.nodesSize(); i++)
            {
                int importance = calculateImportance(chooseImportance, processGraph.nodes[i]);
                priorityQueue.Enqueue(processGraph.nodes[i], importance);
            }
            // take lowest priority and contract
            while (priorityQueue.Count != 0)
            {
                Node node = priorityQueue.Dequeue();
                contractNode(chooseContraction, node, maxSettledNodes, false);
            }
            
        }

        private int calculateImportance(int chooseType, Node node)
        {
            switch (chooseType)
            {
                default:
                case 0:
                    // standard simulation importance
                    return simulationImportance(node);
                case 1:
                    // simple formula
                    return simpleImportance(node);
            }
        }

        private int simulationImportance(Node node) { return 0; }
        private int simpleImportance(Node node) 
        {
            int forward = 0;
            int backward = 0;
            // importance = incomming edges * outgoing edges
            for (int i = node.startIndex; i <= node.lastIndex; i++)
            {
                if (processGraph.edges[i].backward)
                {
                    backward++;
                }
                if (processGraph.edges[i].forward)
                {
                    forward++;
                }
            }
            return forward * backward;
        }

        private void contractNode(int chooseType, Node node, int maxSettledNodes, bool simulate)
        {
            int firstEdge = node.startIndex;
            int lastEdge = node.lastIndex;
            double cost = 0;

            // set nodes of outgoing edges as targets
            int numberOfTargets = 0;
            for (int i = firstEdge; i <= lastEdge; i++)
            {
                if (processGraph.edges[i].forward)
                {
                    int targetNode = processGraph.edges[i].target;
                    // check if node is not already contracted
                    if (processGraph.nodes[targetNode].contracted)
                    {
                        continue;
                    }

                    // cost to outgoing node, find largest
                    if (cost < processGraph.edges[i].weight)
                    {
                        cost = processGraph.edges[i].weight; 
                    }

                    processGraph.nodes[targetNode].searchTarget = true;
                    numberOfTargets++;
                }
            }

            // set current node as contracted
            node.contracted = true;

            // no outgoing edges so no shortcuts needed
            if (numberOfTargets == 0)
            {
                // set node as not contracted again if it is a simulation
                if (simulate)
                {
                    node.contracted = false;
                }
                return;
            }

            // find shortest path for each incomming edge
            for (int i = firstEdge; i <= lastEdge; i++)
            {
                if (processGraph.edges[i].backward)
                {
                    int sourceNode = processGraph.edges[i].target;
                    // check if node is not already contracted
                    if (processGraph.nodes[sourceNode].contracted)
                    {
                        continue;
                    }
                    double maxCost = cost + processGraph.edges[i].weight; // add cost from source to max cost to targets
                    switch (chooseType)
                    {
                        case 0:
                            // use dijkstra
                            dijkstra(processGraph.nodes[sourceNode], maxCost, numberOfTargets, maxSettledNodes);
                            break;
                        case 1:
                            // use A*
                            Astar(processGraph.nodes[sourceNode]);
                            break;
                    }
                }
            }

            // set node as not contracted again if it is a simulation
            if (simulate)
            {
                node.contracted = false;
            }

            // set targets back to false
            for (int i = firstEdge; i <= lastEdge; i++)
            {
                // stop early if all targets have been found
                if (numberOfTargets == 0)
                {
                    break;
                }
                if (processGraph.edges[i].forward)
                {
                    int targetNode = processGraph.edges[i].target;
                    // check if node is searchTarget
                    if (processGraph.nodes[targetNode].searchTarget)
                    {
                        processGraph.nodes[targetNode].searchTarget = false;
                        numberOfTargets--;
                    }
                }
            }
        }
        private void dijkstra(Node source, double maxCost, int numberOfTargets, int maxSettledNodes) 
        {
            SimplePriorityQueue<CurrentNode> dijkstraPriorityQueue = new SimplePriorityQueue<CurrentNode>();

            CurrentNode u = new CurrentNode(source, 0);
            dijkstraPriorityQueue.Enqueue(u, 0);

            while(priorityQueue.Count != 0)
            {
                if (numberOfTargets == 0)
                {
                    return;
                }

                u = dijkstraPriorityQueue.Dequeue();

                if (u.distance > maxCost || maxSettledNodes == 0)
                {
                    return;
                }
                maxSettledNodes--;

                if (u.node.searchTarget) 
                {
                    numberOfTargets--;
                }

                relaxEdges(u, true, dijkstraPriorityQueue);
            }
        }

        private void relaxEdges(CurrentNode parent, bool forward, SimplePriorityQueue<CurrentNode> dijkstraPriorityQueue)
        {
            double parentDistance = parent.distance;
            int firstEdge = parent.node.startIndex;
            int lastEdge = parent.node.lastIndex;

            for (int i = firstEdge; i < lastEdge; i++)
            {
                Node targetNode = processGraph.nodes[processGraph.edges[i].target];
                if (targetNode.contracted)
                {
                    continue;
                }

                // check if wrong direction
                if (!forward && processGraph.edges[i].forward || forward && processGraph.edges[i].backward)
                {
                    continue;
                }

                // new distance from source of search to target of edge
                double newDistance = parentDistance + processGraph.edges[i].weight;
                //dijkstraPriorityQueue.Contains(targetNode);
            }
        }
        private void Astar(Node node) { }
    }
}
