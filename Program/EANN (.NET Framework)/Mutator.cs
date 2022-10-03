using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace EANN
{
    static class Mutator
    {
        static private Random random = new Random();

        // Returns a float value between two given values
        public static float GetRandomFloat(float minValue = 0f, float maxValue = 1f)
        {
            lock (random)
                return (float)(random.NextDouble() * (maxValue - minValue) + minValue);
        }

        // Returns a integer value between zero and the given value
        public static int GetRandomInt(int maxValue)
        {
            lock (random)
                return random.Next(maxValue);
        }


        // DeepClone changes serializable objects into bitstrings, copies it and deserializes it.
        // Can be used to copy very complex objects.
        public static T DeepClone<T>(this T a)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, a);
                stream.Position = 0;
                return (T)formatter.Deserialize(stream);
            }
        }

        // DeepClone method for copying an object more than 1 time.
        // Returns an array.
        public static T[] DeepClone<T>(this T a, int n)
        {
            T[] allCopies = new T[n];
            for (int i = 0; i < n; i++)
            {
                allCopies[i] = DeepClone(a);
            }
            return allCopies;
        }

        // ChangeWeight, given a network, selects a random connection and modifies it.
        // Modifications changes a value by adding a real number between +10 and -10.
        public static void ChangeWeight(Network network)
        {
            if (network.linkCount == 0)
                return;
            float maxValue = 10f;
            float minValue = -10f;
            Neuron randomNeuron;
            List<Neuron> allNeurons = network.allNeurons;
            do
            {
                int randomIndex = GetRandomInt(allNeurons.Count);
                randomNeuron = allNeurons[randomIndex];
            } while (randomNeuron.neuronClass == 0);
            
            float weightChange = GetRandomFloat(minValue, maxValue);
            int randomIndex2 = GetRandomInt(randomNeuron.incomingConnectionWeights.Count + 1);
            if (randomIndex2 == randomNeuron.incomingConnectionWeights.Count)
                randomNeuron.bias += weightChange;
            else
                randomNeuron.incomingConnectionWeights[randomIndex2] += weightChange;
        }

        // Adds a link to a given network
        public static void AddLink(Network network)
        {
            int nodeCount = network.allNeurons.Count;

            // Select two nodes
            Neuron neuron1;
            Neuron neuron2;
            do
            {
                int randomIndex1 = GetRandomInt(nodeCount);
                int randomIndex2 = GetRandomInt(nodeCount);
                neuron1 = network.allNeurons[randomIndex1];
                neuron2 = network.allNeurons[randomIndex2];
            } while ((neuron1.neuronClass == 0 && neuron2.neuronClass == 0) || (neuron1.neuronClass == network.maxClass && neuron2.neuronClass == network.maxClass) || (neuron1 == neuron2));

            // Determine which node is deeper and if needed, swap
            if(neuron1.neuronClass > neuron2.neuronClass)
            {
                Neuron neuron3 = neuron1;
                neuron1 = neuron2;
                neuron2 = neuron3;
            }

            // If there is no connection yet, add the connection
            if (!neuron2.incomingConnections.Contains(neuron1))
            {
                neuron2.incomingConnections.Add(neuron1);
                neuron2.incomingConnectionWeights.Add(GetRandomFloat(-3f, 3f));
                neuron2.RecalculateClass();
                neuron1.outgoingConnections.Add(neuron2);
            }
            network.linkCount++;
        }

        // Removes a random link given a network
        public static void RemoveLink(Network network)
        {
            int nodeCount = network.allNeurons.Count;
            if (network.linkCount == 0)
                return;

            // Select a random network
            Neuron neuron;
            do
            {
                int nodeIndex = GetRandomInt(nodeCount);
                neuron = network.allNeurons[nodeIndex];
            }
            while (neuron.neuronClass == 0 || neuron.incomingConnections.Count == 0);

            // Select a random connection and remove it
            int connectionIndex = GetRandomInt(neuron.incomingConnections.Count);
            Neuron prev = neuron.incomingConnections[connectionIndex];
            prev.outgoingConnections.Remove(neuron);
            neuron.incomingConnections.Remove(prev);
            neuron.incomingConnectionWeights.RemoveAt(connectionIndex);
            neuron.RecalculateClass();
            network.linkCount--;
        }

        // Adds a node to a given network
        public static void AddNeuron(Network network)
        {
            // Select two random nodes
            int nodeCount = network.allNeurons.Count;
            if (network.linkCount == 0)
                return;
            Neuron neuron1;
            Neuron neuron2;
            do
            {
                int randomIndex2 = GetRandomInt(nodeCount);
                neuron2 = network.allNeurons[randomIndex2];
            }
            while (neuron2.neuronClass == 0 || neuron2.incomingConnections.Count == 0);

            int randomIndex1 = GetRandomInt(neuron2.incomingConnections.Count);
            neuron1 = neuron2.incomingConnections[randomIndex1];

            // Determine new weight values and add/remove connections
            float oldSignal = neuron1.output * neuron2.incomingConnectionWeights[randomIndex1];
            float newWeight = GetRandomFloat();

            Neuron newNeuron = new Neuron(neuron1.neuronClass + 1);
            network.allNeurons.Add(newNeuron);
            newNeuron.incomingConnections.Add(neuron1);
            newNeuron.incomingConnectionWeights.Add(newWeight);
            newNeuron.outgoingConnections.Add(neuron2);

            neuron1.outgoingConnections.Remove(neuron2);
            neuron1.outgoingConnections.Add(newNeuron);

            int indexOfLostConnection = neuron2.incomingConnections.IndexOf(neuron1);
            neuron2.incomingConnections.RemoveAt(indexOfLostConnection);
            neuron2.incomingConnectionWeights.RemoveAt(indexOfLostConnection);
            neuron2.incomingConnections.Add(newNeuron);

            float newSignal = newNeuron.CalculateOutput();
            float newIncomingWeight = oldSignal / newSignal;
            neuron2.incomingConnectionWeights.Add(newIncomingWeight);

            neuron2.RecalculateClass();
            network.neuronCount++;
        }

        // Removes a neuron of a given network
        public static void RemoveNeuron(Network network)
        {
            int nodeCount = network.allNeurons.Count;
            if (nodeCount == network.inputLayer.Count + network.outputLayer.Count)
                return;

            // Select a random node
            Neuron neuronToRemove;
            do
            {
                int randomIndex = GetRandomInt(nodeCount);
                neuronToRemove = network.allNeurons[randomIndex];
            }
            while (neuronToRemove.neuronClass == 0 || neuronToRemove.neuronClass == network.maxClass);

            // Fill lists with outgoing nodes, incoming nodes and weight values of these incoming nodes
            int connectionCount = neuronToRemove.incomingConnections.Count * neuronToRemove.outgoingConnections.Count;
            Neuron[] connectionDestinations = new Neuron[connectionCount];
            Neuron[] connectionSources = new Neuron[connectionCount];
            float[] connectionWeights = new float[connectionCount];
            float sumOfOutputsOfIncomingNodes = 0;

            // Determine sum of outputs of input nodes
            for (int i = 0; i < neuronToRemove.incomingConnections.Count; i++)
                sumOfOutputsOfIncomingNodes += neuronToRemove.incomingConnections[i].CalculateOutput();

            // Calculate outputn of node to remove
            float neuronToRemoveOutput = 0;
            float signal = 0;
            for (int i = 0; i < neuronToRemove.incomingConnections.Count; i++)
            {
                signal += neuronToRemove.incomingConnections[i].CalculateOutput() * neuronToRemove.incomingConnectionWeights[i];
                neuronToRemoveOutput = (float)((Math.Exp(signal) - Math.Exp(-signal)) / (Math.Exp(signal) + Math.Exp(-signal)));
            }

            // Determine new connections and weights
            for (int i = 0; i < neuronToRemove.incomingConnections.Count; i++)
            {
                for (int j = 0; j < neuronToRemove.outgoingConnections.Count; j++)
                {
                    int index = i * neuronToRemove.outgoingConnections.Count + j;
                    Neuron next = neuronToRemove.outgoingConnections[j];
                    int indexOfNeuronToRemoveInNext = next.incomingConnections.IndexOf(neuronToRemove);
                    Neuron prev = neuronToRemove.incomingConnections[i];
                    connectionDestinations[index] = next;
                    connectionSources[index] = prev;
                    float newWeight = (neuronToRemoveOutput * next.incomingConnectionWeights[indexOfNeuronToRemoveInNext]) / sumOfOutputsOfIncomingNodes;
                    connectionWeights[index] = newWeight;
                }
            }

            // Remove the neuron
            foreach(Neuron incoming in neuronToRemove.incomingConnections)
            {
                incoming.outgoingConnections.Remove(neuronToRemove);
            }

            foreach(Neuron outgoing in neuronToRemove.outgoingConnections)
            {
                int index = outgoing.incomingConnections.IndexOf(neuronToRemove);
                outgoing.incomingConnections.RemoveAt(index);
                outgoing.incomingConnectionWeights.RemoveAt(index);
            }

            // Add the new connections and apply weights
            for (int i = 0; i < connectionCount; i++)
            {
                Neuron next = connectionDestinations[i];
                Neuron prev = connectionSources[i];
                float newWeight = connectionWeights[i];

                if (next.incomingConnections.Contains(prev))
                {
                    int index = next.incomingConnections.IndexOf(prev);
                    next.incomingConnectionWeights[index] += newWeight;
                }
                else
                {
                    next.incomingConnections.Add(prev);
                    next.incomingConnectionWeights.Add(newWeight);
                    prev.outgoingConnections.Add(next);
                }
                next.RecalculateClass();
            }

            network.allNeurons.Remove(neuronToRemove);
            network.neuronCount--;
        }
    } 
}