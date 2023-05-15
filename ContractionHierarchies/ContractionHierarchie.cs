﻿using ContractionHierarchies.DataStructures;
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

        /// <summary>
        /// <para> 
        /// <paramref name="importanceType"/> 
        /// 0: simple heuristic
        /// 1: simulation
        /// <paramref name="contractionType"/>
        /// 0: BiDir speedup contraction
        /// 1: normal contraction
        /// <paramref name="contractionSearchType"/>
        /// 0: dijkstra
        /// <paramref name="recalculateImportance"/>
        /// true: check if the importance is still the smallest
        /// false: do not recalculate the importance
        /// <paramref name="maxSettledNodes"/>
        /// number of nodes that should be settled before placing a shortcut
        /// </para>
        /// </summary>
        public void PreProcess(int importanceType, int contractionType, int contractionSearchType, bool recalculateImportance, int maxSettledNodes, int maxWrongImportance)
        {
            // calculate importance for each node and fill priority queue
            int nodeLevel = 0;
            CalculateImportanceForAll(nodeLevel, importanceType, contractionSearchType, maxSettledNodes);

            // take lowest priority and contract
            while (PriorityQueue.TryDequeue(out ProcessNode node, out int priority)) // recursief maken
            {
                int wrongImportance = 0;
                int importance = CalculateImportance(importanceType, node, contractionSearchType, maxSettledNodes);
                if (recalculateImportance)
                {
                    if (priority <= importance)
                    {
                        int oldShortcutsAdded = TotalShortCutsAdded;
                        ContractNode(contractionType, contractionSearchType, node, maxSettledNodes, false);
                        Console.WriteLine("contraction node: " + nodeLevel + " / " + ProcessGraph.NodesSize + " shortcuts added: " + (TotalShortCutsAdded - oldShortcutsAdded) + " total shortcuts: " + TotalShortCutsAdded);
                        node.NodeLevel = nodeLevel;
                        nodeLevel++;
                    }
                    else
                    {
                        wrongImportance++;
                        if (wrongImportance > maxWrongImportance)
                        {
                            CalculateImportanceForAll(nodeLevel, importanceType, contractionSearchType, maxSettledNodes);
                            continue;
                        }
                        PriorityQueue.Enqueue(node, importance);
                    }
                }
                else
                {
                    Console.WriteLine("contraction node: " + nodeLevel + " / " + ProcessGraph.NodesSize + " total shortcuts: " + TotalShortCutsAdded);
                    ContractNode(contractionType, contractionSearchType, node, maxSettledNodes, false);
                    node.NodeLevel = nodeLevel;
                    nodeLevel++;
                }
            }
            Console.WriteLine("Total number of shortcuts added: " + TotalShortCutsAdded);
        }

        private void CalculateImportanceForAll(int nodeLevel, int importanceType, int contractionSearchType, int maxSettledNodes)
        {
            PriorityQueue = new(ProcessGraph.NodesSize - nodeLevel);
            for (int i = 0; i < ProcessGraph.NodesSize; i++)
            {
                ProcessNode node = ProcessGraph.Nodes[i];
                if (node.Contracted)
                {
                    continue;
                }
                int importance = CalculateImportance(importanceType, ProcessGraph.Nodes[i], contractionSearchType, maxSettledNodes);
                PriorityQueue.Enqueue(ProcessGraph.Nodes[i], importance);
            }
        }

        private void ContractNode(int contractionType, int contractionSearchType, ProcessNode node, int maxSettledNodes, bool simulate)
        {
            switch (contractionType)
            {
                case 0:  default: ContractNodeBiDir(contractionSearchType, node, maxSettledNodes, simulate); break;// standard simulation importance
                case 1: ContractNodeNormal(contractionSearchType, node, maxSettledNodes, simulate); break;// simple formula
            };
        }

        private int CalculateImportance(int importanceType, ProcessNode node, int contractionSearchType, int maxSettledNodes)
        {
            return importanceType switch
            {
                1 => SimulationImportance(node, contractionSearchType, maxSettledNodes),// standard simulation importance
                _ => SimpleImportance(node),// simple formula
            };
        }

        private int SimulationImportance(ProcessNode node, int contractionSearchType, int maxSettledNodes) 
        { 
            return ContractNodeNormal(contractionSearchType, node, maxSettledNodes, true); 
        }
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

        private void ContractNodeBiDir(int contractionSearchType, ProcessNode node, int maxSettledNodes, bool simulate)
        {
            int firstEdge = node.FirstIndex;
            int lastEdge = node.LastIndex;
            float maxCostToTarget = 0;
            List<int> completedBiDirSources = new();

            // set current node as Contracted
            node.Contracted = true;

            // find shortest path for each incomming edge
            for (int i = firstEdge; i <= lastEdge; i++)
            {
                bool sourceBidir = false;
                Edge edgeToSource = ProcessGraph.Edges[i];
                if (edgeToSource.Backward) // source
                {
                    if (edgeToSource.Forward)
                        sourceBidir = true;
                    List<int> BiDirTargets = new();

                    int sourceNodeID = edgeToSource.Target;
                    // check if node is not already Contracted
                    if (ProcessGraph.Nodes[sourceNodeID].Contracted)
                    {
                        continue;
                    }

                    // set Nodes of outgoing Edges as targets
                    int numberOfTargets = 0;
                    for (int j = firstEdge; j <= lastEdge; j++)
                    {
                        bool targetBidir = false;
                        Edge edgeToTarget = ProcessGraph.Edges[j];
                        if (edgeToTarget.Forward) // target
                        {
                            if (edgeToTarget.Backward)
                                targetBidir = true;
                            ProcessNode targetNode = ProcessGraph.Nodes[edgeToTarget.Target];
                            // check if node is not already Contracted or if it is the source node
                            if (targetNode.Contracted || edgeToTarget.Target == sourceNodeID)
                            {
                                continue;
                            }

                            // shortcut already placed in both directions when targetNode was the source
                            if (sourceBidir && targetBidir)
                            {
                                if (completedBiDirSources.Contains(targetNode.ID))
                                {
                                    continue;
                                }
                                BiDirTargets.Add(targetNode.ID);
                            }

                            // cost to outgoing node, find largest
                            if (maxCostToTarget < edgeToTarget.Weight)
                            {
                                maxCostToTarget = edgeToTarget.Weight;
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

                    float costFromSource = edgeToSource.Weight; // add cost from source to max cost to targets
                    float maxCost = costFromSource + maxCostToTarget; // add cost from source to max cost to targets
                    completedBiDirSources.Add(sourceNodeID);

                    TotalShortCutsAdded += contractionSearchType switch
                    {
                        _ => Dijkstra(ProcessGraph.Nodes[sourceNodeID], maxCost, numberOfTargets, BiDirTargets, maxSettledNodes, node.ID, costFromSource, simulate),// use dijkstra
                    };
                }
            }
            // set node back to not Contracted if it is a simulation
            if (simulate)
            {
                node.Contracted = false;
            }
        }

        private int ContractNodeNormal(int contractionSearchType, ProcessNode node, int maxSettledNodes, bool simulate)
        {
            int firstEdge = node.FirstIndex;
            int lastEdge = node.LastIndex;
            float maxCostToTarget = 0;
            int addedShortCuts = 0;

            // set current node as Contracted
            node.Contracted = true;

            // find shortest path for each incomming edge
            for (int i = firstEdge; i <= lastEdge; i++)
            {
                Edge edgeIncomming = ProcessGraph.Edges[i];
                if (edgeIncomming.Backward) // source
                {
                    int sourceNode = edgeIncomming.Target;
                    // check if node is not already Contracted
                    if (ProcessGraph.Nodes[sourceNode].Contracted)
                    {
                        continue;
                    }

                    // set Nodes of outgoing Edges as targets
                    int numberOfTargets = 0;
                    for (int j = firstEdge; j <= lastEdge; j++)
                    {
                        Edge edgeOutgoing = ProcessGraph.Edges[j];
                        if (edgeOutgoing.Forward) // target
                        {
                            ProcessNode targetNode = ProcessGraph.Nodes[edgeOutgoing.Target];
                            // check if node is not already Contracted or if it is the source node
                            if (targetNode.Contracted || edgeOutgoing.Target == sourceNode)
                            {
                                continue;
                            }

                            // cost to outgoing node, find largest
                            if (maxCostToTarget < edgeOutgoing.Weight)
                            {
                                maxCostToTarget = edgeOutgoing.Weight;
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

                    addedShortCuts = contractionSearchType switch
                    {
                        _ => Dijkstra(ProcessGraph.Nodes[sourceNode], maxCost, numberOfTargets, new List<int>(), maxSettledNodes, node.ID, costFromSource, simulate),// use dijkstra
                    };
                }
            }
            // set node back to not Contracted if it is a simulation
            if (simulate)
            {
                node.Contracted = false;
            }

            TotalShortCutsAdded += addedShortCuts;
            return addedShortCuts;
        }

        private int Dijkstra(ProcessNode source, double maxCost, int numberOfTargets, List<int> BiDirTargets, int maxSettledNodes, int middleNodeID, float costToMiddleNode, bool simulate) 
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
                        if (simulate)
                        {
                            ShortcutsAdded++;
                            targetNode.SearchTarget = false;
                            continue;
                        }
                        float cost = costToMiddleNode + edge.Weight;
                        if (BiDirTargets.Contains(targetNode.ID))
                        {
                            ProcessGraph.AddEdgeToProcessNode(source, cost, targetNode.ID, true, true, false); // add forward and backward edge to source
                            ProcessGraph.AddEdgeToProcessNode(targetNode, cost, source.ID, true, true, false); // add foward and backward edge to target
                            targetNode.SearchTarget = false;
                            ShortcutsAdded += 2;
                        } 
                        else
                        {
                            ProcessGraph.AddEdgeToProcessNode(source, cost, targetNode.ID, true, false, false); // add forward edge
                            ProcessGraph.AddEdgeToProcessNode(targetNode, cost, source.ID, false, true, false); // add backward edge
                            targetNode.SearchTarget = false;
                            ShortcutsAdded++;
                        }
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
                            if (BiDirTargets.Contains(currentNode.ID))
                            {
                                ProcessGraph.AddEdgeToProcessNode(source, cost, currentNode.ID, true, true, false); // add forward and backward edge to source
                                ProcessGraph.AddEdgeToProcessNode(currentNode, cost, source.ID, true, true, false); // add foward and backward edge to target
                                ShortcutsAdded++; // incremented again 5 lines down
                            }
                            ProcessGraph.AddEdgeToProcessNode(source, cost, currentNode.ID, true, false, false); // add forward edge
                            ProcessGraph.AddEdgeToProcessNode(currentNode, cost, source.ID, false, true, false); // add backward edge
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

        public float QueryCH(int source, int target)
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
            PriorityQueue<SearchNode, float> forwardQueue = new();
            PriorityQueue<SearchNode, float> backwardQueue = new();

            SearchNode sourceNode = SearchGraph.Nodes[source];
            SearchNode targetNode = SearchGraph.Nodes[target];

            sourceNode.SettledForward = true;
            sourceNode.Distance = 0;
            targetNode.SettledBackward = true;
            targetNode.Distance = 0;

            SearchGraph.Nodes[source] = sourceNode;
            SearchGraph.Nodes[target] = targetNode;

            forwardQueue.Enqueue(sourceNode, 0);
            backwardQueue.Enqueue(targetNode, 0);

            SearchNode u;
            List<Tuple<SearchNode, float>> settledBothDir = new();

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
                else if (forwardQueue.Peek().Distance <= backwardQueue.Peek().Distance)
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

        private void BiDirRelaxEdges(SearchNode parent, bool forward, PriorityQueue<SearchNode, float> PriorityQueue, List<Tuple<SearchNode, float>> settledBothDir)
        {
            float parentDistance = parent.Distance;
            int firstEdge = parent.FirstIndex;
            int lastEdge = parent.LastIndex;

            for (int i = firstEdge; i <= lastEdge; i++)
            {
                Edge edge = SearchGraph.Edges[i];
                SearchNode targetNode = SearchGraph.Nodes[edge.Target];
                if (edge.Forward != forward && edge.Backward == forward)
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
                    float totalDistance = parentDistance + edge.Weight + targetNode.Distance; // total distance from source to target found
                    settledBothDir.Add(new Tuple<SearchNode, float>(targetNode, totalDistance));
                    continue;
                }

                // new distance from source of search to target of edge
                float newDistance = parentDistance + edge.Weight;
                
                if (newDistance < targetNode.Distance)
                {
                    targetNode.Distance = newDistance;
                    PriorityQueue.Enqueue(targetNode, newDistance);
                }
                SearchGraph.Nodes[edge.Target] = targetNode;
            }
        }

        public float QueryDijkstra(int source, int target)
        {
            PriorityQueue<ProcessNode, float> priorityQueue = new();

            priorityQueue.Enqueue(ProcessGraph.Nodes[source], 0);
            float minDistance = float.MaxValue;
            while (priorityQueue.TryDequeue(out ProcessNode currentNode, out float distance))
            {
                if (currentNode.ID == target)
                {
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                    }
                }

                // relaxe edges
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

                    // new distance from source of search to target of edge
                    float newDistance = distance + edge.Weight;
                    if (newDistance < minDistance)
                    {
                        // enqueue new target
                        priorityQueue.Enqueue(targetNode, newDistance);
                    }
                }
            }
            return minDistance;
        }
    }
}
