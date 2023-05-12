using ContractionHierarchies.DataStructures;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ContractionHierarchies
{
    public class ContractionHierarchie
    {
        SearchGraph SearchGraph { get; set; }
        ProcessGraph ProcessGraph { get; set; }
        PriorityQueue<ProcessNode, int> PriorityQueue { get; set; }
        int EdgeGroupSize { get; set; } = 6 ;

        int TotalShortCutsAdded = 0;

        public ContractionHierarchie(string inputFile, int edgeGroupSize)
        {
            this.EdgeGroupSize = edgeGroupSize;
            ProcessGraph = new ProcessGraph(inputFile, edgeGroupSize);
            PriorityQueue = new(ProcessGraph.NodesSize);
        }

        public ContractionHierarchie(string inputFile)
        {
            ProcessGraph = new ProcessGraph(inputFile, EdgeGroupSize);
            PriorityQueue = new(ProcessGraph.NodesSize);
        }

        public void PreProcess(int chooseImportance, int chooseContraction, int maxSettledNodes)
        {
            
            // calculate importance for each node and fill priority queue
            for (int i = 0; i < ProcessGraph.NodesSize; i++)
            {
                int importance = CalculateImportance(chooseImportance, ProcessGraph.Nodes[i]);
                PriorityQueue.Enqueue(ProcessGraph.Nodes[i], importance);
            }
            
            
            int nodeLevel = 0;
            // take lowest priority and contract
            while (PriorityQueue.Count != 0) // recursief maken
            {
                ProcessNode node = PriorityQueue.Dequeue();
                ContractNode(chooseContraction, node, maxSettledNodes, false);
                node.NodeLevel = nodeLevel;
                nodeLevel++;
            }
            Console.WriteLine("Total number of shortcuts added: " + TotalShortCutsAdded);
        }

        private int CalculateImportance(int chooseType, ProcessNode node)
        {
            return chooseType switch
            {
                1 => SimpleImportance(node),// simple formula
                _ => SimulationImportance(node),// standard simulation importance
            };
        }

        private int SimulationImportance(ProcessNode node) { return 0; }
        private int SimpleImportance(ProcessNode node) 
        {
            int forward = 0;
            int backward = 0;
            // importance = incomming Edges * outgoing Edges
            for (int i = node.FirstIndex; i <= node.LastIndex; i++)
            {
                Edge edge = ProcessGraph.Edges[i];
                if (edge.Backward)
                {
                    backward++;
                }
                if (edge.Forward)
                {
                    forward++;
                }
            }
            return forward * backward;
        }

        private void ContractNode(int chooseType, ProcessNode node, int maxSettledNodes, bool simulate)
        {
            int firstEdge = node.FirstIndex;
            int lastEdge = node.LastIndex;
            float maxCostToTarget = 0;

            // set current node as Contracted
            node.Contracted = true;

            // find shortest path for each incomming edge
            for (int i = firstEdge; i <= lastEdge; i++)
            {
                if (ProcessGraph.Edges[i].Backward) // source
                {
                    int sourceNode = ProcessGraph.Edges[i].Target;
                    // check if node is not already Contracted
                    if (ProcessGraph.Nodes[sourceNode].Contracted)
                    {
                        continue;
                    }

                    // set Nodes of outgoing Edges as targets
                    int numberOfTargets = 0;
                    for (int j = firstEdge; j <= lastEdge; j++)
                    {
                        Edge edge = ProcessGraph.Edges[j];
                        if (edge.Forward) // target
                        {
                            ProcessNode targetNode = ProcessGraph.Nodes[edge.Target];
                            // check if node is not already Contracted or if it is the source node
                            if (targetNode.Contracted || edge.Target == sourceNode)
                            {
                                continue;
                            }

                            // cost to outgoing node, find largest
                            if (maxCostToTarget < edge.Weight)
                            {
                                maxCostToTarget = edge.Weight;
                            }

                            targetNode.SearchTarget = true;
                            numberOfTargets++;
                        }
                    }

                    // no targets so no shortcuts needed
                    if (numberOfTargets == 0)
                    {
                        continue;
                    }

                    float costFromSource = ProcessGraph.Edges[i].Weight; // add cost from source to max cost to targets
                    float maxCost = costFromSource + maxCostToTarget; // add cost from source to max cost to targets

                    TotalShortCutsAdded += chooseType switch
                    {
                        _ => Dijkstra(ProcessGraph.Nodes[sourceNode], maxCost, numberOfTargets, maxSettledNodes, node.ID, costFromSource, simulate),// use dijkstra
                    };
                }
            }
            // set node back to not Contracted if it is a simulation
            if (simulate)
            {
                node.Contracted = false;
            }
        }

        private int Dijkstra(ProcessNode source, double maxCost, int numberOfTargets, int maxSettledNodes, int middleNodeID, float costToMiddleNode, bool simulate) 
        {
            PriorityQueue<ProcessNode, float> dijkstraPriorityQueue = new();

            dijkstraPriorityQueue.Enqueue(source, 0);

            int ShortcutsAdded = 0;

            while (dijkstraPriorityQueue.TryDequeue(out ProcessNode currentNode, out float distance))
            {
                if (numberOfTargets == 0)
                {
                    return ShortcutsAdded;
                }

                maxSettledNodes--;

                // limit the search space, if targets have not been found add a shortcut to them or if the distance is larget than the maxcost
                if (maxSettledNodes == 0 || distance > maxCost)
                {
                    // add shortcuts 
                    int firstEdge = ProcessGraph.Nodes[middleNodeID].FirstIndex;
                    int lastEdge = ProcessGraph.Nodes[middleNodeID].LastIndex;
                    for (int i = firstEdge; i <= lastEdge; i++)
                    {
                        Edge edge = ProcessGraph.Edges[i];
                        // check if forward edge and SearchTarget
                        if (!edge.Forward)
                        {
                            continue;
                        }
                        ProcessNode targetNode = ProcessGraph.Nodes[edge.Target];
                        if (!targetNode.SearchTarget)
                        {
                            continue;
                        }
                        float cost = costToMiddleNode + edge.Weight;

                        ProcessGraph.AddEdgeToProcessNode(source, cost, targetNode.ID, true, true); // add forward edge
                        ProcessGraph.AddEdgeToProcessNode(targetNode, cost, source.ID, false, true); // add backward edge
                        targetNode.SearchTarget = false;
                        ShortcutsAdded++;
                    }
                    return ShortcutsAdded;
                }

                if (currentNode.SearchTarget)
                {
                    // add shortcut if cost is smaller
                    float cost = 0;
                    // find cost from u to middleNode
                    for (int i = currentNode.FirstIndex; i <= currentNode.LastIndex; i++)
                    {
                        Edge edge = ProcessGraph.Edges[i];
                        if (!edge.Backward)
                        {
                            continue;
                        }
                        if (edge.Target == middleNodeID) 
                        {
                            cost = costToMiddleNode + edge.Weight;
                            break;
                        }
                    }
                    if (distance > cost)
                    {
                        if (!simulate)
                        {
                            // add shortcut
                            ProcessGraph.AddEdgeToProcessNode(source, cost, currentNode.ID, true, true); // add forward edge
                            ProcessGraph.AddEdgeToProcessNode(currentNode, cost, source.ID, false, true); // add backward edge
                        }
                        ShortcutsAdded++;
                    }
                    currentNode.SearchTarget = false;
                    numberOfTargets--;
                }
                RelaxEdges(currentNode, distance, dijkstraPriorityQueue);
            }
            return ShortcutsAdded;
        }

        private void RelaxEdges(ProcessNode currentNode, float distance, PriorityQueue<ProcessNode, float> dijkstraPriorityQueue)
        {
            int firstEdge = currentNode.FirstIndex;
            int lastEdge = currentNode.LastIndex;

            for (int i = firstEdge; i <= lastEdge; i++)
            {
                Edge edge = ProcessGraph.Edges[i];
                // check if edge is in the right direction
                if (!edge.Forward)
                {
                    continue;
                }
                ProcessNode targetNode = ProcessGraph.Nodes[edge.Target];
                // check if target node is not already contracted
                if (targetNode.Contracted )
                {
                    continue;
                }
                
                // new distance from source of search to target of edge
                float newDistance = distance + edge.Weight;
                // enqueue new target
                dijkstraPriorityQueue.Enqueue(targetNode, newDistance);
            }
        }

        public void CreateSearchGraph() 
        {
            SearchGraph = new SearchGraph(ProcessGraph, ProcessGraph.EdgesSize);
        }

        public float Query(int source, int target)
        {
            if (source == target)
            {
                return 0;
            }
            // save temp nodes for reuse of the graph
            float result = BiDirDijkstra(source, target);

            // set clean nodes back
            Array.Copy(SearchGraph.NodesBackup, SearchGraph.Nodes, SearchGraph.Nodes.Length);
            return result;
        }

        private float BiDirDijkstra(int source, int target)
        {
            SearchNodeEqualityComparer currentNodeEqualityComparer = new();
            SimplePriorityQueue<SearchNode, float> forwardQueue = new(currentNodeEqualityComparer);
            SimplePriorityQueue<SearchNode, float> backwardQueue = new(currentNodeEqualityComparer);

            SearchNode sourceNode = SearchGraph.Nodes[source];
            SearchNode targetNode = SearchGraph.Nodes[target];

            sourceNode.SettledForward = true;
            targetNode.SettledBackward = true;

            SearchGraph.Nodes[source] = sourceNode;
            SearchGraph.Nodes[target] = targetNode;

            forwardQueue.Enqueue(sourceNode, 0);
            backwardQueue.Enqueue(targetNode, 0);

            SearchNode u;
            List<Tuple<SearchNode, float>> settledBothDir = new List<Tuple<SearchNode, float>>();

            while (forwardQueue.Count != 0 || backwardQueue.Count != 0)
            {
                // choose node from one of the priority queues
                if (forwardQueue.Count == 0)
                {
                    u = backwardQueue.Dequeue();
                    BiDirRelaxEdges(u, false, backwardQueue, settledBothDir);
                } 
                else if (backwardQueue.Count == 0)
                { 
                    u = forwardQueue.Dequeue();
                    BiDirRelaxEdges(u, true, forwardQueue, settledBothDir);
                }
                else if (forwardQueue.First().Distance <= backwardQueue.First().Distance)
                {
                    u = forwardQueue.Dequeue();
                    BiDirRelaxEdges(u, true, forwardQueue, settledBothDir);
                } 
                else
                {
                    u = backwardQueue.Dequeue();
                    BiDirRelaxEdges(u, false, backwardQueue, settledBothDir);
                }
            }
            if (settledBothDir.Count == 0)
            {
                return -1;
            }
            return settledBothDir.Min(t => t.Item2);
        }

        private void BiDirRelaxEdges(SearchNode parent, bool forward, SimplePriorityQueue<SearchNode, float> PriorityQueue, List<Tuple<SearchNode, float>> settledBothDir)
        {
            float parentDistance = parent.Distance;
            int firstEdge = parent.FirstIndex;
            int lastEdge = parent.LastIndex;

            for (int i = firstEdge; i <= lastEdge; i++)
            {
                SearchNode targetNode = SearchGraph.Nodes[SearchGraph.Edges[i].Target];
                if (!(SearchGraph.Edges[i].Forward == forward || SearchGraph.Edges[i].Backward != forward))
                {
                    continue;
                }

                if (forward)
                {
                    targetNode.SettledForward = true;
                }
                else
                {
                    targetNode.SettledBackward = true;
                }

                if (targetNode.SettledForward && targetNode.SettledBackward)
                {
                    float totalDistance = parentDistance + SearchGraph.Edges[i].Weight + targetNode.Distance; // total distance from source to target found
                    settledBothDir.Add(new Tuple<SearchNode, float>(targetNode, totalDistance));
                    continue;
                }

                // new distance from source of search to target of edge
                float newDistance = parentDistance + SearchGraph.Edges[i].Weight;
                
                // if new target already in queue remove to update its cost
                if (PriorityQueue.Contains(targetNode))
                {
                    float oldDistance = PriorityQueue.GetPriority(targetNode); 
                    if (oldDistance > newDistance)
                    {
                        targetNode.Distance = newDistance;
                        PriorityQueue.Remove(targetNode);
                        PriorityQueue.Enqueue(targetNode, newDistance);
                    }
                    SearchGraph.Nodes[SearchGraph.Edges[i].Target] = targetNode;
                }
                else
                {
                    targetNode.Distance = newDistance;
                    SearchGraph.Nodes[SearchGraph.Edges[i].Target] = targetNode;
                    // enqueue new target
                    PriorityQueue.Enqueue(targetNode, newDistance);
                }
            }
        }
    }
}
