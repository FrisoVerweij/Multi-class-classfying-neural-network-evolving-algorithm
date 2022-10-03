using System;
using System.Collections.Generic;

namespace EANN
{
    [Serializable]
    class Neuron
    {
        public List<Neuron> incomingConnections = new List<Neuron>();
        public List<Neuron> outgoingConnections = new List<Neuron>();
        public List<float> incomingConnectionWeights = new List<float>();

        public int neuronClass;
        public float bias;

        public bool isCalculated = false;
        public float output;

        public Neuron(int classNumber)
        {
            neuronClass = classNumber;
            bias = 0;
        }

        // Clear saved output
        public void FlushOutput()
        {
            isCalculated = false;
        }

        // Calculate the output of this node
        public float CalculateOutput()
        {
            if (isCalculated)
                return output;

            output = bias;
            for (int i = 0; i < incomingConnections.Count; i++)
            {
                output += incomingConnections[i].CalculateOutput() * incomingConnectionWeights[i];
            }
            output = (float)((Math.Exp(output) - Math.Exp(-output)) / (Math.Exp(output) + Math.Exp(-output)));
            isCalculated = true;
            return output;
        }

        // Redetermine the nodeClass value of this node
        public void RecalculateClass()
        {
            if (neuronClass == 1000)
                return;
            int highest = 0;
            foreach(Neuron incoming in incomingConnections)
            {
                if (incoming.neuronClass > highest)
                    highest = incoming.neuronClass;
            }
            if(highest + 1 != neuronClass)
            {
                neuronClass = highest + 1;
                foreach (Neuron outgoing in outgoingConnections)
                {
                    outgoing.RecalculateClass();
                }
            }
        }
    }
}
