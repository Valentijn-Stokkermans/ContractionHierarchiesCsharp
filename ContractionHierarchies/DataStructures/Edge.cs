using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContractionHierarchies.DataStructures
{
    public struct Edge
    {
        public Edge(float weight, int target, bool forward, bool backward) 
        { 
            Weight = weight; 
            Target = target;
            Forward = forward;
            Backward = backward;
        }

        // empty edge
        public Edge() { }

        // weight
        public float Weight { get; set; } = -1;
        // target node
        public int Target { get; set; } = -1;
        // forward edge
        public bool Forward { get; set; } = false;
        // reverse edge for backward search
        public bool Backward { get; set; } = false;
        // contracted
        public bool Contracted { get; set; } = false;
    }
}
