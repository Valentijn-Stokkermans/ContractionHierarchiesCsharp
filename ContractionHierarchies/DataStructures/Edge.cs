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
            this.weight = weight; 
            this.target = target;
            this.forward = forward;
            this.backward = backward;
            contracted = false;
            shortcut = false;
            shortcutPointer = -1;
        }

        // empty edge
        public Edge()
        {
            weight = -1;
            target = -1;
            forward = false;
            backward = false;
            contracted = false;
            shortcut = false;
            shortcutPointer = -1;
        }

        // weight
        public float weight;
        // target node
        public int target;
        // forward edge
        public bool forward;
        // reverse edge for backward search
        public bool backward;
        // contracted
        public bool contracted;
        // shortcut
        public bool shortcut;
        // shortcut pointer to contracted node
        public int shortcutPointer;

    }
}
