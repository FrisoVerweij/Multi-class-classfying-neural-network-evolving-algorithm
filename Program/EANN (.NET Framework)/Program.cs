using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

namespace EANN
{
    class Program
    {
        static Visualizer newForm = null;
        static List<Thread> threadList;
        static string[] report;

        static void Main()
        {
            // Read variables from Variables.cs
            // Initialise
            string pathToTrain = Variables.PATHTOTRAINSET;
            string pathToTest = Variables.PATHTOTESTSET;
            string runName = Variables.RUNNAME;
            string reportLocation = Variables.PATHTOREPORT;
            int iterationCount = Variables.ITERATIONCOUNT;
            int populationSize = Variables.POPULATIONSIZE;
            report = new string[iterationCount + 1];
            report[0] = "Iteration;BestScore;AverageScore;BestFitness;AverageFitness";
            Stopwatch stopwatch = new Stopwatch();
            threadList = new List<Thread>();

            // Create new forms application
            Thread newThread = new Thread(() =>
            {
                newForm = new Visualizer();
                Application.Run(newForm);
            });
            Thread.Sleep(200);
            newThread.Start();
            Thread.Sleep(200);
            // Parse the dataset
            Console.WriteLine("Parsing data...");
            Dataset trainData = Parser.ParseData(@pathToTrain);
            Dataset testData  = Parser.ParseData(@pathToTest);
            Console.WriteLine("Parsing complete!");

            // Create new initial network
            Network newNetwork = new Network(trainData);

            // Start iteration process
            stopwatch.Start();
            newNetwork = StartIterating(newNetwork, populationSize, iterationCount, trainData.sampleSet);
            double elapsedTime = stopwatch.Elapsed.TotalSeconds;
            // Save report to file
            System.IO.File.WriteAllLines(@reportLocation, report);

            // Compose summary
            string[] reportToScreen = new string[10 + trainData.labels.Count];
            int reportIndex = 0;
            reportToScreen[reportIndex] = "Training Complete."; reportIndex++;
            reportToScreen[reportIndex] = "Possible Labels:"; reportIndex++;
            for (int i = 0; i < trainData.labels.Count; i++)
            {
                reportToScreen[reportIndex] = trainData.labels[i].ToString();
                reportIndex++;
            }
            reportToScreen[reportIndex] = "\nInputcount: " + trainData.inputCount; reportIndex++;
            reportToScreen[reportIndex] = "Outputcount: " + trainData.outputCount; reportIndex++;
            reportToScreen[reportIndex] = "\nPerformance on testset: " + newNetwork.Evaluate(testData.sampleSet); reportIndex++;
            reportToScreen[reportIndex] = "Amount of nodes: " + newNetwork.allNeurons.Count; reportIndex++;
            reportToScreen[reportIndex] = "Amount of iterations: " + iterationCount; reportIndex++;
            reportToScreen[reportIndex] = "Population size: " + populationSize; reportIndex++;
            reportToScreen[reportIndex] = "Elite number: " + Variables.ELITECOUNT; reportIndex++;
            reportToScreen[reportIndex] = "Time Elapsed: " + elapsedTime; reportIndex++;

            // Post summary to screen
            foreach(string s in reportToScreen)
                Console.WriteLine(s);

            // Save summary
            System.IO.File.WriteAllLines(@reportLocation, reportToScreen);

            // Allow the user to ask questions
            StartConsulting(newNetwork);
        }

        // Starts the iteration process
        static Network StartIterating(Network network, int populationSize, int maxIterations, List<Sample> data)
        {
            // Create a initial population
            Network[] population = Mutator.DeepClone(network, populationSize);

            // Iterate on population
            for (int iteration = 1; iteration <= maxIterations; iteration++)
            {
                population = Iterate(population, populationSize, data, iteration);
            }

            return GetBestNetwork(population, data);
        }

        static Network[] Iterate(Network[] population, int populationSize, List<Sample> data, int iteration)
        {
            // Initialise variables
            Network[] newPopulation = new Network[populationSize];
            float[] fitnessArray = new float[populationSize];
            float[] scoreArray = new float[populationSize];
            float[] chanceArray = new float[populationSize];
            float cumulativeFitness = 0;
            float cumulativeChance = 0;
            int threadCount = Variables.THREADCOUNT;
            int elite = Variables.ELITECOUNT;
            threadList.Clear();

            // Determine how much work each thread will receive
            float segmentSize = (float)populationSize / (float)threadCount;
            float[] cumFitnessArray = new float[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                int segment = i;
                Thread newThread = new Thread(() =>
                {
                    // Calculate for each individual its fitness
                    float cumFitnessTemp = 0;
                    for (int j = (int)Math.Round(segment * segmentSize); j < (int)Math.Round((segment + 1) * segmentSize); j++)
                    {
                        float score = population[j].Evaluate(data);
                        float fitness = score - GetPenalty(population[j]);
                        fitnessArray[j] = fitness;
                        cumFitnessTemp += fitness;
                    }
                    cumFitnessArray[segment] = cumFitnessTemp;
                });

                threadList.Add(newThread);
            }

            // Start threads and so the evalutation process
            foreach (Thread t in threadList)
                t.Start();

            WaitTillThreadsAreDone();
            foreach (float f in cumFitnessArray)
                cumulativeFitness += f;

            // Sort the population based on fitness
            Array.Sort(fitnessArray, population);

            // Determine score
            // This is not very pretty, but sorting 3 layers is unnecessarily tedious
            for (int i = 0; i < populationSize; i++)
            {
                scoreArray[i] = fitnessArray[i] + GetPenalty(population[i]);
            }

            // Calculate averages
            float averageScore = 0;
            float averageFitness = 0;
            for (int i = 0; i < populationSize; i++)
            {
                averageScore += scoreArray[i];
                averageFitness += fitnessArray[i];
            }
            averageScore /= populationSize;
            averageFitness /= populationSize;

            // Report iteration info to screen
            Console.WriteLine("----------------------------------------------------------------------------------------------------------------------\n" +
                "Iteration {0}\tBest score: {1}\tAverageScore: {2}\tBest fitness: {3}\tAverage fitness: {4}", iteration, scoreArray[scoreArray.Length - 1], averageScore, fitnessArray[fitnessArray.Length - 1], averageFitness);
            Console.WriteLine("----------------------------------------------------------------------------------------------------------------------\n");

            // Report information to file
            report[iteration] = iteration + ";" + scoreArray[scoreArray.Length - 1] + ";" + averageScore + ";" + fitnessArray[fitnessArray.Length - 1] + ";" + averageFitness;


            // Draw the best-performing network
            threadList.Clear();
            Thread drawThread = new Thread(() => newForm.DrawNetwork(population[population.Length - 1]));
            drawThread.Start();

            // Determine pick-chances for each individual
            for (int i = 0; i < populationSize; i++)
            {
                float chance = fitnessArray[i] / cumulativeFitness;
                cumulativeChance += chance;
                chanceArray[i] = cumulativeChance;
            }

            // Copy elite to new population
            Array.Copy(population, populationSize - elite, newPopulation, 0, elite);

            // Determine work for each thread
            segmentSize = (float)(populationSize - elite) / (float)threadCount;
            for (int i = 0; i < threadCount; i++)
            {
                int segment = i;
                Thread newThread = new Thread(() =>
                {
                    // Apply a mutation
                    for (int j = elite + (int)Math.Round(segmentSize * segment); j < elite + (int)Math.Round(segmentSize * (segment + 1)); j++)
                    {
                        Mutate(newPopulation, population, chanceArray, populationSize, j);
                    }
                });
                threadList.Add(newThread);
            }

            // Start mutation process
            foreach (Thread t in threadList)
                t.Start();

            threadList.Add(drawThread);

            WaitTillThreadsAreDone();
            return newPopulation;
        }

        static void WaitTillThreadsAreDone()
        {
            foreach (Thread thread in threadList)
                thread.Join();
        }

        // Returns penalty value
        static float GetPenalty(Network network)
        {
            return network.neuronCount * Variables.NEURONPENALTY + network.linkCount * Variables.LINKPENALTY; ;
        }

        // Returns best-performing network on the train set
        static Network GetBestNetwork(Network[] population, List<Sample> data)
        {
            float bestScore = 0;
            Network currentBest = null;

            foreach(Network n in population)
            {
                float score = n.Evaluate(data);
                if(score > bestScore)
                {
                    bestScore = score;
                    currentBest = n;
                }
            }
            return currentBest;
        }

        // Mutates an individual
        static void Mutate(Network[] newPopulation, Network[] population, float[] chanceArray, int populationSize, int i)
        {
            // Initialise and choose random individual
            float randomValue = Mutator.GetRandomFloat();
            int index = 0;
            while (randomValue > chanceArray[index] && index < populationSize - 1)
            {
                index++;
            }
            Network selectedNetwork = Mutator.DeepClone(population[index]);
            float addNeuronChance = Variables.ADDNEURON;
            float addLinkChance = Variables.ADDLINK;
            float removeNeuronChance = Variables.REMOVENEURON;
            float removeLinkChance = Variables.REMOVELINK;

            // Determine and apply mutation
            randomValue = Mutator.GetRandomFloat();
            if (randomValue < addNeuronChance)
                Mutator.AddNeuron(selectedNetwork);
            else if (randomValue < addNeuronChance + addLinkChance)
                Mutator.AddLink(selectedNetwork);
            else if (randomValue < addNeuronChance + addLinkChance + removeNeuronChance)
                Mutator.RemoveNeuron(selectedNetwork);
            else if (randomValue < addNeuronChance + addLinkChance + removeNeuronChance + removeLinkChance)
                Mutator.RemoveLink(selectedNetwork);
            else
                Mutator.ChangeWeight(selectedNetwork);

            // Add mutated network to new population
            newPopulation[i] = selectedNetwork;
        }

        // Allows the user to ask questions
        static void StartConsulting(Network network)
        {
            while (true)
            {
                Console.WriteLine("Please ask your query in the form of x1;x2;x3;....");
                string[] valueStrings = Console.ReadLine().Split(';');
                float[] values = new float[valueStrings.Length];
                for (int i = 0; i < valueStrings.Length; i++)
                {
                    values[i] = (float)Convert.ToDouble(valueStrings[i]);
                }
                Console.WriteLine(network.Calculate(values));
            }
        }
    }
}
