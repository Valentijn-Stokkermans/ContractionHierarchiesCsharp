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
        FastPriorityQueue<ProcessNode> PriorityQueue { get; set; }
        int edgeGroupSize { get; set; } = 6 ;

        int totalShortCutsAdded = 0;

        public ContractionHierarchie(string inputFile, int edgeGroupSize)
        {
            this.edgeGroupSize = edgeGroupSize;
            ProcessGraph = new ProcessGraph(inputFile, edgeGroupSize);
            PriorityQueue = new FastPriorityQueue<ProcessNode>(ProcessGraph.NodesSize);
        }

        public ContractionHierarchie(string inputFile)
        {
            ProcessGraph = new ProcessGraph(inputFile, edgeGroupSize);
            PriorityQueue = new FastPriorityQueue<ProcessNode>(ProcessGraph.NodesSize);
        }

        public void PreProcess(int chooseImportance, int chooseContraction, int maxSettledNodes)
        {
            ProcessGraph.PrintProcessGraph();
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
            Console.WriteLine("total shortcuts added: " + totalShortCutsAdded);
            ProcessGraph.PrintProcessGraph();
        }

        private int CalculateImportance(int chooseType, ProcessNode node)
        {
            switch (chooseType)
            {
                default:
                case 0:
                    // standard simulation importance
                    return SimulationImportance(node);
                case 1:
                    // simple formula
                    return SimpleImportance(node);
            }
        }

        private int SimulationImportance(ProcessNode node) { return 0; }
        private int SimpleImportance(ProcessNode node) 
        {
            int forward = 0;
            int backward = 0;
            // importance = incomming Edges * outgoing Edges
            for (int i = node.FirstIndex; i <= node.LastIndex; i++)
            {
                if (ProcessGraph.Edges[i].Backward)
                {
                    backward++;
                }
                if (ProcessGraph.Edges[i].Forward)
                {
                    forward++;
                }
            }
            return forward * backward;
        }

        private void ContractNode(int chooseType, ProcessNode node, int maxSettledNodes, bool simulate)
        {
            Console.WriteLine("\n\ncontracting: " + node.ID);
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
                    Console.WriteLine("source: " + sourceNode);
                    // check if node is not already Contracted
                    if (ProcessGraph.Nodes[sourceNode].Contracted)
                    {
                        continue;
                    }

                    // set Nodes of outgoing Edges as targets
                    int numberOfTargets = 0;
                    for (int j = firstEdge; j <= lastEdge; j++)
                    {
                        if (ProcessGraph.Edges[j].Forward) // target
                        {
                            int targetNode = ProcessGraph.Edges[j].Target;
                            // check if node is not already Contracted or if it is the source node
                            if (ProcessGraph.Nodes[targetNode].Contracted || targetNode == sourceNode)
                            {
                                continue;
                            }

                            // cost to outgoing node, find largest
                            if (maxCostToTarget < ProcessGraph.Edges[j].Weight)
                            {
                                maxCostToTarget = ProcessGraph.Edges[j].Weight;
                            }

                            Console.WriteLine("target: " + targetNode);
                            ProcessGraph.Nodes[targetNode].SearchTarget = true;
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

                    switch (chooseType)
                    {
                        default:
                        case 0:
                            // use dijkstra
                            totalShortCutsAdded += Dijkstra(ProcessGraph.Nodes[sourceNode], maxCost, numberOfTargets, maxSettledNodes, node.ID, costFromSource, simulate);
                            break;
                    }
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
            CurrentNodeEqualityComparer currentNodeEqualityComparer = new CurrentNodeEqualityComparer();
            SimplePriorityQueue<CurrentNode, float> dijkstraPriorityQueue = new SimplePriorityQueue<CurrentNode, float>(currentNodeEqualityComparer);

            CurrentNode u = new CurrentNode(source, 0);
            dijkstraPriorityQueue.Enqueue(u, 0);

            int ShortcutsAdded = 0;

            while (dijkstraPriorityQueue.Count != 0)
            {
                if (numberOfTargets == 0)
                {
                    return ShortcutsAdded;
                }

                u = dijkstraPriorityQueue.Dequeue();
                maxSettledNodes--;

                // limit the search space, if targets have not been found add a shortcut to them
                if (maxSettledNodes == 0 || u.Distance > maxCost)
                {
                    // add shortcuts 
                    int firstEdge = ProcessGraph.Nodes[middleNodeID].FirstIndex;
                    int lastEdge = ProcessGraph.Nodes[middleNodeID].LastIndex;
                    for (int i = firstEdge; i <= lastEdge; i++)
                    {
                        // check if forward edge and SearchTarget
                        if (!ProcessGraph.Edges[i].Forward ||
                            !ProcessGraph.Nodes[ProcessGraph.Edges[i].Target].SearchTarget)
                        {
                            continue;
                        }
                        ProcessNode targetNode = ProcessGraph.Nodes[ProcessGraph.Edges[i].Target];
                        float cost = costToMiddleNode + ProcessGraph.Edges[i].Weight;
                        Console.WriteLine("shortcut added from: " + source.ID + " to: " + targetNode.ID);
                        ProcessGraph.AddEdgeToProcessNode(source.ID, source, cost, targetNode.ID, true, true); // add forward edge
                        ProcessGraph.AddEdgeToProcessNode(targetNode.ID, targetNode, cost, source.ID, false, true); // add backward edge
                        targetNode.SearchTarget = false;
                        ShortcutsAdded++;
                    }
                    return ShortcutsAdded;
                }

                Console.WriteLine("Dequeue: " + u.Node.ID);

                if (u.Node.SearchTarget)
                {
                    // add shortcut if cost is smaller
                    float cost = 0;
                    // find cost from u to middleNode
                    for (int i = u.Node.FirstIndex; i <= u.Node.LastIndex; i++)
                    {
                        if (!ProcessGraph.Edges[i].Backward)
                        {
                            continue;
                        }
                        if (ProcessGraph.Edges[i].Target == middleNodeID) 
                        {
                            cost = costToMiddleNode + ProcessGraph.Edges[i].Weight;
                            break;
                        }
                    }
                    Console.WriteLine("target found: " + u.Node.ID + " distance: " + u.Distance + " cost: " + cost);
                    if (u.Distance > cost)
                    {
                        if (!simulate)
                        {
                            // add shortcut
                            Console.WriteLine("shortcut added from: " + source.ID + " to: " + u.Node.ID);
                            ProcessGraph.AddEdgeToProcessNode(source.ID, source, cost, u.Node.ID, true, true); // add forward edge
                            ProcessGraph.AddEdgeToProcessNode(u.Node.ID, u.Node, cost, source.ID, false, true); // add backward edge
                        }
                        ShortcutsAdded++;
                    }
                    u.Node.SearchTarget = false;
                    numberOfTargets--;
                }
                RelaxEdges(u, dijkstraPriorityQueue);
            }
            return ShortcutsAdded;
        }

        private void RelaxEdges(CurrentNode parent, SimplePriorityQueue<CurrentNode, float> dijkstraPriorityQueue)
        {
            float parentDistance = parent.Distance;
            int firstEdge = parent.Node.FirstIndex;
            int lastEdge = parent.Node.LastIndex;

            for (int i = firstEdge; i <= lastEdge; i++)
            {
                ProcessNode targetNode = ProcessGraph.Nodes[ProcessGraph.Edges[i].Target];
                if (targetNode.Contracted)
                {
                    continue;
                }

                // check if wrong direction
                if (!ProcessGraph.Edges[i].Forward)
                {
                    continue;
                }
                
                // new distance from source of search to target of edge
                float newDistance = parentDistance + ProcessGraph.Edges[i].Weight;
                CurrentNode target = new CurrentNode(targetNode, newDistance);
                // if new target already in queue remove to update its cost
                if (dijkstraPriorityQueue.Contains(target))
                {
                    float oldDistance = dijkstraPriorityQueue.GetPriority(target);
                    if (oldDistance > newDistance)
                    {
                        dijkstraPriorityQueue.Remove(target);
                        dijkstraPriorityQueue.Enqueue(target, newDistance);
                        Console.WriteLine("enqueue: " + target.Node.ID + " new: " + newDistance + " old: " + oldDistance);
                    }
                } 
                else
                {
                    // enqueue new target
                    Console.WriteLine("enqueue " + target.Node.ID);
                    dijkstraPriorityQueue.Enqueue(target, newDistance);
                }
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
            SearchNode[] temp = new SearchNode[SearchGraph.Nodes.Length];
            Array.Copy(SearchGraph.Nodes, temp, SearchGraph.Nodes.Length);

            float result = BiDirDijkstra(source, target);

            // set clean nodes back
            SearchGraph.Nodes = temp;
            SearchGraph.PrintSearchGraph();
            return result;
        }

        private float BiDirDijkstra(int source, int target)
        {
            Console.WriteLine("\n\nfrom: " + source + " to: " + target);
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
            Console.WriteLine("parent: " + parent.ID + " forward: " + forward);
            float parentDistance = parent.Distance;
            int firstEdge = parent.FirstIndex;
            int lastEdge = parent.LastIndex;

            for (int i = firstEdge; i <= lastEdge; i++)
            {
                SearchNode targetNode = SearchGraph.Nodes[SearchGraph.Edges[i].Target];
                Console.WriteLine("targetNode: " + targetNode.ID + " forward: " + SearchGraph.Edges[i].Forward + " backward: " + SearchGraph.Edges[i].Backward);
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
                    Console.WriteLine("found " + "A: " + parent.ID + " distance: " + parentDistance + " B: " + targetNode.ID + " distance: " + targetNode.Distance + " total: " + totalDistance);
                    settledBothDir.Add(new Tuple<SearchNode, float>(targetNode, totalDistance));
                    continue;
                }

                // new distance from source of search to target of edge
                float newDistance = parentDistance + SearchGraph.Edges[i].Weight;
                
                Console.WriteLine("settled: " + targetNode.ID + " distance: " + newDistance);
                // if new target already in queue remove to update its cost
                if (PriorityQueue.Contains(targetNode))
                {
                    float oldDistance = PriorityQueue.GetPriority(targetNode); 
                    if (oldDistance > newDistance)
                    {
                        targetNode.Distance = newDistance;
                        PriorityQueue.Remove(targetNode);
                        PriorityQueue.Enqueue(targetNode, newDistance);
                        Console.WriteLine("enqueue: " + targetNode.ID + " new: " + newDistance + " old: " + oldDistance);
                    }
                    SearchGraph.Nodes[SearchGraph.Edges[i].Target] = targetNode;
                }
                else
                {
                    targetNode.Distance = newDistance;
                    SearchGraph.Nodes[SearchGraph.Edges[i].Target] = targetNode;
                    // enqueue new target
                    Console.WriteLine("enqueue " + targetNode.ID);
                    PriorityQueue.Enqueue(targetNode, newDistance);
                }
            }
        }
    }
}
