using ContractionHierarchies.DataStructures;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
            float maxCostToTarget = 0;

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
                    if (maxCostToTarget < processGraph.edges[i].weight)
                    {
                        maxCostToTarget = processGraph.edges[i].weight; 
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
                    float costFromSource = processGraph.edges[i].weight; // add cost from source to max cost to targets
                    float maxCost = costFromSource + maxCostToTarget; // add cost from source to max cost to targets
                    switch (chooseType)
                    {
                        case 0:
                            // use dijkstra
                            dijkstra(processGraph.nodes[sourceNode], maxCost, numberOfTargets, maxSettledNodes, node.id, costFromSource, simulate);
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
        }
        private int dijkstra(Node source, double maxCost, int numberOfTargets, int maxSettledNodes, int middleNodeID, float costToMiddleNode, bool simulate) 
        {
            CurrentNodeEqualityComparer currentNodeEqualityComparer = new CurrentNodeEqualityComparer();
            SimplePriorityQueue<CurrentNode, float> dijkstraPriorityQueue = new SimplePriorityQueue<CurrentNode, float>(currentNodeEqualityComparer);

            CurrentNode u = new CurrentNode(source, 0);
            dijkstraPriorityQueue.Enqueue(u, 0);

            int ShortcutsAdded = 0;

            while(priorityQueue.Count != 0)
            {
                // limit the search space, if targets have not been found add a shortcut to them
                if (maxSettledNodes == 0)
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
                        processGraph.addEdge(source.id, source, cost, targetNode.id, true, true); // add forward edge
                        processGraph.addEdge(targetNode.id, targetNode, cost, source.id, false, true); // add backward edge
                        targetNode.searchTarget = false;
                        ShortcutsAdded++;
                    }
                    return ShortcutsAdded;
                }

                u = dijkstraPriorityQueue.Dequeue();
                maxSettledNodes--;

                if (numberOfTargets == 0 || u.distance > maxCost)
                {
                    return ShortcutsAdded;
                }

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
                    if (u.distance < cost) 
                    {
                        if (!simulate)
                        {
                            // add shortcut
                            processGraph.addEdge(source.id, source, u.distance, u.node.id, true, true); // add forward edge
                            processGraph.addEdge(u.node.id, u.node, u.distance, source.id, false, true); // add backward edge
                        }
                        ShortcutsAdded++;
                    }
                    u.node.searchTarget = false;
                    numberOfTargets--;
                }
                relaxEdges(u, true, dijkstraPriorityQueue);
            }
            return ShortcutsAdded;
        }

        private void relaxEdges(CurrentNode parent, bool forward, SimplePriorityQueue<CurrentNode, float> dijkstraPriorityQueue)
        {
            float parentDistance = parent.distance;
            int firstEdge = parent.node.startIndex;
            int lastEdge = parent.node.lastIndex;

            for (int i = firstEdge; i <= lastEdge; i++)
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
                float newDistance = parentDistance + processGraph.edges[i].weight;
                CurrentNode target = new CurrentNode(targetNode, newDistance);
                // if new target already in queue remove to update its cost
                if (dijkstraPriorityQueue.Contains(target))
                {
                    dijkstraPriorityQueue.Remove(target);
                }
                // enqueue new target
                dijkstraPriorityQueue.Enqueue(target, newDistance);
            }
        }
        private void Astar(Node node) { }
    }
}
