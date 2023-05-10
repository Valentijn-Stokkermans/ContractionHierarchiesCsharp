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
        SearchGraph searchGraph { get; set; }
        ProcessGraph processGraph { get; set; }
        FastPriorityQueue<Node> priorityQueue { get; set; }
        int edgeGroupSize { get; set; } = 6 ;

        int totalShortCutsAdded = 0;

        public ContractionHierarchie(string inputFile, int edgeGroupSize)
        {
            this.edgeGroupSize = edgeGroupSize;
            processGraph = new ProcessGraph(inputFile, edgeGroupSize);
            priorityQueue = new FastPriorityQueue<Node>(processGraph.nodesSize());
        }

        public ContractionHierarchie(string inputFile)
        {
            processGraph = new ProcessGraph(inputFile, edgeGroupSize);
            priorityQueue = new FastPriorityQueue<Node>(processGraph.nodesSize());
        }

        public void preprocess(int chooseImportance, int chooseContraction, int maxSettledNodes)
        {
            processGraph.printProcessGraph();
            // calculate importance for each node and fill priority queue
            for (int i = 0; i < processGraph.nodesSize(); i++)
            {
                int importance = calculateImportance(chooseImportance, processGraph.nodes[i]);
                priorityQueue.Enqueue(processGraph.nodes[i], importance);
            }
            int nodeLevel = 0;
            // take lowest priority and contract
            while (priorityQueue.Count != 0) // recursief maken
            {
                Node node = priorityQueue.Dequeue();
                contractNode(chooseContraction, node, maxSettledNodes, false);
                node.nodeLevel = nodeLevel;
                nodeLevel++;
            }
            Console.WriteLine("total shortcuts added: " + totalShortCutsAdded);
            processGraph.printProcessGraph();
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
            Console.WriteLine("\n\ncontracting: " + node.id);
            int firstEdge = node.startIndex;
            int lastEdge = node.lastIndex;
            float maxCostToTarget = 0;

            // set current node as contracted
            node.contracted = true;

            // find shortest path for each incomming edge
            for (int i = firstEdge; i <= lastEdge; i++)
            {
                if (processGraph.edges[i].backward) // source
                {
                    int sourceNode = processGraph.edges[i].target;
                    Console.WriteLine("source: " + sourceNode);
                    // check if node is not already contracted
                    if (processGraph.nodes[sourceNode].contracted)
                    {
                        continue;
                    }

                    // set nodes of outgoing edges as targets
                    int numberOfTargets = 0;
                    for (int j = firstEdge; j <= lastEdge; j++)
                    {
                        if (processGraph.edges[j].forward) // target
                        {
                            int targetNode = processGraph.edges[j].target;
                            // check if node is not already contracted or if it is the source node
                            if (processGraph.nodes[targetNode].contracted || targetNode == sourceNode)
                            {
                                continue;
                            }

                            // cost to outgoing node, find largest
                            if (maxCostToTarget < processGraph.edges[j].weight)
                            {
                                maxCostToTarget = processGraph.edges[j].weight;
                            }

                            Console.WriteLine("target: " + targetNode);
                            processGraph.nodes[targetNode].searchTarget = true;
                            numberOfTargets++;
                        }
                    }

                    // no targets so no shortcuts needed
                    if (numberOfTargets == 0)
                    {
                        continue;
                    }

                    float costFromSource = processGraph.edges[i].weight; // add cost from source to max cost to targets
                    float maxCost = costFromSource + maxCostToTarget; // add cost from source to max cost to targets

                    switch (chooseType)
                    {
                        default:
                        case 0:
                            // use dijkstra
                            totalShortCutsAdded += dijkstra(processGraph.nodes[sourceNode], maxCost, numberOfTargets, maxSettledNodes, node.id, costFromSource, simulate);
                            break;
                    }
                }
            }
            // set node back to not contracted if it is a simulation
            if (simulate)
            {
                node.contracted = false;
            }
        }

        private int dijkstra(Node source, double maxCost, int numberOfTargets, int maxSettledNodes, int middleNodeID, float costToMiddleNode, bool simulate) 
        {
            CurrentNodeEqualityComparer currentNodeEqualityComparer = new CurrentNodeEqualityComparer();
            SimplePriorityQueue<CurrentNode, float> dijkstraPriorityQueue = new SimplePriorityQueue<CurrentNode, float>(currentNodeEqualityComparer);

            CurrentNode u = new CurrentNode(source, 0, true);
            dijkstraPriorityQueue.Enqueue(u, 0);

            int ShortcutsAdded = 0;

            return recursiveDijkstra(source, maxCost, numberOfTargets, maxSettledNodes, middleNodeID, costToMiddleNode, 
                simulate, ShortcutsAdded, dijkstraPriorityQueue);

        }

        private int recursiveDijkstra(Node source, double maxCost, int numberOfTargets, int maxSettledNodes, int middleNodeID, float costToMiddleNode, bool simulate, int shortcutsAdded, SimplePriorityQueue<CurrentNode, float> dijkstraPriorityQueue)
        {
            if (numberOfTargets == 0 || dijkstraPriorityQueue.Count == 0)
            {
                return shortcutsAdded;
            }

            CurrentNode u = dijkstraPriorityQueue.Dequeue();
            maxSettledNodes--;

            // limit the search space, if targets have not been found add a shortcut to them
            if (maxSettledNodes == 0 || u.distance > maxCost)
            {
                // add shortcuts 
                int firstEdge = processGraph.nodes[middleNodeID].startIndex;
                int lastEdge = processGraph.nodes[middleNodeID].lastIndex;
                for (int i = firstEdge; i <= lastEdge; i++)
                {
                    // check if forward edge and searchTarget
                    if (!processGraph.edges[i].forward ||
                        !processGraph.nodes[processGraph.edges[i].target].searchTarget)
                    {
                        continue;
                    }
                    Node targetNode = processGraph.nodes[processGraph.edges[i].target];
                    float cost = costToMiddleNode + processGraph.edges[i].weight;
                    Console.WriteLine("shortcut added from: " + source.id + " to: " + targetNode.id);
                    processGraph.addEdge(source.id, source, cost, targetNode.id, true, true); // add forward edge
                    processGraph.addEdge(targetNode.id, targetNode, cost, source.id, false, true); // add backward edge
                    targetNode.searchTarget = false;
                    shortcutsAdded++;
                }
                return shortcutsAdded;
            }

            Console.WriteLine("Dequeue: " + u.node.id);

            if (u.node.searchTarget)
            {
                // add shortcut if cost is smaller
                float cost = 0;
                // find cost from u to middleNode
                for (int i = u.node.startIndex; i <= u.node.lastIndex; i++)
                {
                    if (!processGraph.edges[i].backward)
                    {
                        continue;
                    }
                    if (processGraph.edges[i].target == middleNodeID)
                    {
                        cost = costToMiddleNode + processGraph.edges[i].weight;
                        break;
                    }
                }
                Console.WriteLine("target found: " + u.node.id + " distance: " + u.distance + " cost: " + cost);
                if (u.distance > cost)
                {
                    if (!simulate)
                    {
                        // add shortcut
                        Console.WriteLine("shortcut added from: " + source.id + " to: " + u.node.id);
                        processGraph.addEdge(source.id, source, cost, u.node.id, true, true); // add forward edge
                        processGraph.addEdge(u.node.id, u.node, cost, source.id, false, true); // add backward edge
                    }
                    shortcutsAdded++;
                }
                u.node.searchTarget = false;
                numberOfTargets--;
            }

            // relaxe edges
            u.node.settled = true;

            for (int i = u.node.startIndex; i <= u.node.lastIndex; i++)
            {
                Node targetNode = processGraph.nodes[processGraph.edges[i].target];
                if (targetNode.contracted || targetNode.settled)
                {
                    continue;
                }

                // check if wrong direction
                if (!processGraph.edges[i].forward)
                {
                    continue;
                }

                // new distance from source of search to target of edge
                float newDistance = u.distance + processGraph.edges[i].weight;
                CurrentNode target = new CurrentNode(targetNode, newDistance, true);
                // if new target already in queue remove to update its cost
                if (dijkstraPriorityQueue.Contains(target))
                {
                    float oldDistance = dijkstraPriorityQueue.GetPriority(target);
                    if (oldDistance > newDistance)
                    {
                        dijkstraPriorityQueue.Remove(target);
                        dijkstraPriorityQueue.Enqueue(target, newDistance);
                        Console.WriteLine("enqueue: " + target.node.id + " new: " + newDistance + " old: " + oldDistance);
                    }
                }
                else
                {
                    // enqueue new target
                    Console.WriteLine("enqueue " + target.node.id);
                    dijkstraPriorityQueue.Enqueue(target, newDistance);
                }
            }

            recursiveDijkstra(source, maxCost, numberOfTargets, maxSettledNodes, middleNodeID, costToMiddleNode,
                simulate, shortcutsAdded, dijkstraPriorityQueue);

            u.node.settled = false;

            return shortcutsAdded;
        }

        private void relaxEdges(CurrentNode parent, SimplePriorityQueue<CurrentNode, float> dijkstraPriorityQueue)
        {
            float parentDistance = parent.distance;
            int firstEdge = parent.node.startIndex;
            int lastEdge = parent.node.lastIndex;

            parent.node.settled = true;

            for (int i = firstEdge; i <= lastEdge; i++)
            {
                Node targetNode = processGraph.nodes[processGraph.edges[i].target];
                if (targetNode.contracted || targetNode.settled)
                {
                    continue;
                }

                // check if wrong direction
                if (!processGraph.edges[i].forward)
                {
                    continue;
                }
                
                // new distance from source of search to target of edge
                float newDistance = parentDistance + processGraph.edges[i].weight;
                CurrentNode target = new CurrentNode(targetNode, newDistance, true);
                // if new target already in queue remove to update its cost
                if (dijkstraPriorityQueue.Contains(target))
                {
                    float oldDistance = dijkstraPriorityQueue.GetPriority(target);
                    if (oldDistance > newDistance)
                    {
                        dijkstraPriorityQueue.Remove(target);
                        dijkstraPriorityQueue.Enqueue(target, newDistance);
                        Console.WriteLine("enqueue: " + target.node.id + " new: " + newDistance + " old: " + oldDistance);
                    }
                } 
                else
                {
                    // enqueue new target
                    Console.WriteLine("enqueue " + target.node.id);
                    dijkstraPriorityQueue.Enqueue(target, newDistance);
                }
            }
        }

        public void createSearchGraph() 
        { 
            searchGraph = new SearchGraph(processGraph);
        }

        public float query(int source, int target)
        {
            return biDirDijkstra(searchGraph.nodes[source], searchGraph.nodes[target]);
        }

        private float biDirDijkstra(Node source, Node target)
        {
            Console.WriteLine("\n\nfrom: " + source.id + " to: " + target.id);
            CurrentNodeEqualityComparer currentNodeEqualityComparer = new CurrentNodeEqualityComparer();
            SimplePriorityQueue<CurrentNode, float> forwardQueue = new SimplePriorityQueue<CurrentNode, float>(currentNodeEqualityComparer);
            SimplePriorityQueue<CurrentNode, float> backwardQueue = new SimplePriorityQueue<CurrentNode, float>(currentNodeEqualityComparer);

            source.settled = true; 
            target.settled = true;
            forwardQueue.Enqueue(new CurrentNode(source, 0, true), 0);
            backwardQueue.Enqueue(new CurrentNode(target, 0, false), 0);

            CurrentNode u;
            List<Tuple<Node, float>> settledBothDir = new List<Tuple<Node, float>>();

            while (forwardQueue.Count != 0 || backwardQueue.Count != 0)
            {
                // choose node from one of the priority queues
                if (forwardQueue.Count == 0)
                {
                    u = backwardQueue.Dequeue();
                    biDirRelaxEdges(u, false, backwardQueue, settledBothDir);
                } 
                else if (backwardQueue.Count == 0)
                { 
                    u = forwardQueue.Dequeue();
                    biDirRelaxEdges(u, true, forwardQueue, settledBothDir);
                }
                else if (forwardQueue.First().distance <= backwardQueue.First().distance)
                {
                    u = forwardQueue.Dequeue();
                    biDirRelaxEdges(u, true, forwardQueue, settledBothDir);
                } 
                else
                {
                    u = backwardQueue.Dequeue();
                    biDirRelaxEdges(u, false, backwardQueue, settledBothDir);
                }
            }
            if (settledBothDir.Count == 0)
            {
                return -1;
            }
            return settledBothDir.Min(t => t.Item2);
        }

        private void biDirRelaxEdges(CurrentNode parent, bool forward, SimplePriorityQueue<CurrentNode, float> PriorityQueue, List<Tuple<Node, float>> settledBothDir)
        {
            Console.WriteLine("parent: " + parent.node.id + " forward: " + forward);
            float parentDistance = parent.distance;
            int firstEdge = parent.node.startIndex;
            int lastEdge = parent.node.lastIndex;

            for (int i = firstEdge; i <= lastEdge; i++)
            {
                Node targetNode = searchGraph.nodes[searchGraph.edges[i].target];
                Console.WriteLine("targetNode: " + targetNode.id + " forward: " + searchGraph.edges[i].forward + " backward: " + searchGraph.edges[i].backward);
                if (!(searchGraph.edges[i].forward == forward || searchGraph.edges[i].backward != forward))
                {
                    continue;
                }
                
                if (targetNode.settled)
                {
                    float totalDistance = parentDistance + searchGraph.edges[i].weight + targetNode.distance; // total distance from source to target found
                    Console.WriteLine("found " + "A: " + parent.node.id + " distance: " + parentDistance + " B: " + targetNode.id + " distance: " + targetNode.distance + " total: " + totalDistance);
                    settledBothDir.Add(new Tuple<Node, float>(targetNode, totalDistance));
                    continue;
                }

                // new distance from source of search to target of edge
                float newDistance = parentDistance + searchGraph.edges[i].weight;
                targetNode.distance = newDistance;
                targetNode.settled = true;
                CurrentNode target = new CurrentNode(targetNode, newDistance, forward);
                Console.WriteLine("settled: " + target.node.id + " distance: " + newDistance);
                // if new target already in queue remove to update its cost
                if (PriorityQueue.Contains(target))
                {
                    float oldDistance = PriorityQueue.GetPriority(target);
                    if (oldDistance > newDistance)
                    {
                        PriorityQueue.Remove(target);
                        PriorityQueue.Enqueue(target, newDistance);
                        Console.WriteLine("enqueue: " + target.node.id + " new: " + newDistance + " old: " + oldDistance);
                    }
                }
                else
                {
                    // enqueue new target
                    Console.WriteLine("enqueue " + target.node.id);
                    PriorityQueue.Enqueue(target, newDistance);
                }
            }
        }
    }
}
