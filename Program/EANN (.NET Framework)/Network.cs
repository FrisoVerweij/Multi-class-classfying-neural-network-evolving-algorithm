using System;
using System.Collections.Generic;
using System.Linq;

namespace EANN
{
    [Serializable]
    class Network
    {
        int inputCount;
        int outputCount;
        public int neuronCount;
        public int linkCount;
        public int maxClass = 1000;

        public List<Neuron> outputLayer = new List<Neuron>();
        public List<Neuron> inputLayer = new List<Neuron>();
        public List<Neuron> allNeurons = new List<Neuron>();
        List<string> outputLabels;
        
        public Network(Dataset dataset)
        {
            inputCount = dataset.inputCount;
            outputCount = dataset.outputCount;
            outputLabels = dataset.labels;
            Initialize();
        }

    // The method Initialize creates a starting point based on the kind of data given.
    // It creates X inputnodes and Y outputnodes, where X is the amount of inputvariables and Y the amount of possible labels.
    // For every Y, there is a connection between every X and Y, initialised with weight 0.
    void Initialize()
        {
            Random random = new Random();
            for (int i = 0; i < inputCount; i++)
            {
                Neuron inputNeuron = new Neuron(0);
                allNeurons.Add(inputNeuron);
                inputLayer.Add(inputNeuron);
            }
            for (int i = 0; i < outputCount; i++)
            {
                Neuron outputNeuron = new Neuron(maxClass);
                allNeurons.Add(outputNeuron);
                outputLayer.Add(outputNeuron);
            }
            foreach(Neuron n in outputLayer)
            {
                foreach (Neuron i in inputLayer)
                {
                    n.incomingConnections.Add(i);
                }
                for (int i = 0; i < inputLayer.Count; i++)
                {
                    n.incomingConnectionWeights.Add(0f);
                }
            }
            foreach(Neuron n in inputLayer)
            {
                foreach(Neuron o in outputLayer)
                {
                    n.outgoingConnections.Add(o);
                }
            }
            neuronCount = allNeurons.Count;
            linkCount = inputCount * outputCount;
        }

        // Calculate the output of the network given a data point
        public string Calculate(float[] input)
        {
            if(input.Length != inputLayer.Count)
                throw new ArgumentException("Input-length and inputnode-count do not match!");

            FlushAll();

            PrepareInput(input);

            List<float> outputValues = new List<float>();
            foreach(Neuron n in outputLayer)
            {
                float signal = n.CalculateOutput();
                outputValues.Add(signal);
            }
            int indexOfAnswer = outputValues.IndexOf(outputValues.Max());
            string result = outputLabels[indexOfAnswer];
            return result;
        }

        // Clear all saved output values
        public void FlushAll()
        {
            foreach (Neuron n in allNeurons)
                n.FlushOutput();
        }

        // Load input into network
        public void PrepareInput(float[] input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                inputLayer[i].output = input[i];
                inputLayer[i].isCalculated = true;
            }
        }

        // Evaluate the network using a set of data points
        public float Evaluate(List<Sample> sampleSet)
        {
            float successes = 0;
            float total = sampleSet.Count;        
            foreach (Sample s in sampleSet)
            {
                string result = Calculate(s.input);
                if (result == s.output)
                {
                    successes++;
                }
            }
            return (successes / total) * 100;
        }
    }
}
