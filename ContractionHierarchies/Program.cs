using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContractionHierarchies.DataStructures;
using Priority_Queue;


class Program
{
     static void Main(string[] args)
     {
        //var file = "C:\\Users\\Valentijn\\source\\repos\\ContractionHierarchies\\ContractionHierarchies\\Data\\roads.csv";
        //var testGraph = new ProcessGraph(file, 6);
        CurrentNodeEqualityComparer currentNodeEqualityComparer = new CurrentNodeEqualityComparer();
        SimplePriorityQueue<CurrentNode> priorityQueue = new SimplePriorityQueue<CurrentNode>(currentNodeEqualityComparer);

        Node nul = new Node(0);
        CurrentNode curNul = new CurrentNode(nul, 0);
        Node een = new Node(1);
        CurrentNode curEen = new CurrentNode(een, 1);
        Node twee = new Node(2);
        CurrentNode curTwee = new CurrentNode(twee, 2);

        CurrentNode test = new CurrentNode(een, 3);

        priorityQueue.Enqueue(curNul, 0);
        priorityQueue.Enqueue(curEen, 0);
        priorityQueue.Enqueue(curTwee, 0);
        Console.WriteLine(priorityQueue.Contains(test));

    }
}