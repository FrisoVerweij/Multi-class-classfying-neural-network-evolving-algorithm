using System;
using System.Collections.Generic;

namespace EANN
{
    static class Parser
    {
        // ParseData returns a array of Sample objects, given a file location. File must be in .mld format
        static public Dataset ParseData(string dataLocation)
        {
            string[] lines = System.IO.File.ReadAllLines(dataLocation);
            int sampleSize = lines.Length;
            List<Sample> sampleSet = new List<Sample>();
            for (int i = 0; i < sampleSize; i++)
            {
                sampleSet.Add(ExtractSample(lines[i]));
            }
            int inputCount = lines[0].Split(';').Length - 1;
            List<string> labels = ExtractClasses(sampleSet);
            int outputCount = labels.Count;
            return new Dataset(sampleSet, inputCount, outputCount, labels);
        }

        // Given a string from the .txt file, parse the string and return the sample 
        static Sample ExtractSample(string line)
        {
            string[] data = line.Split(';');
            int varCount = data.Length - 1;
            float[] input = new float[varCount];
            for (int j = 0; j < varCount; j++)
            {
                input[j] = (float)Convert.ToDouble(data[j]);
            }
            var output = data[varCount];
            return new Sample(input, output);
        }

        // Returns a list of all possible classes
        static List<string> ExtractClasses(List<Sample> sampleSet)
        {
            List<string> labels = new List<string>();
            foreach(Sample s in sampleSet)
            {
                if (!labels.Contains(s.output))
                    labels.Add(s.output);
            }
            return labels;
        }
    }
}
