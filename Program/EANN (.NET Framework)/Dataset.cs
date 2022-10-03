using System.Collections.Generic;

namespace EANN
{
    class Dataset
    {
        public List<Sample> sampleSet;
        public int sampleSize;
        public int outputCount;
        public int inputCount;
        public List<string> labels;

        public Dataset(List<Sample> _sampleSet, int i, int o, List<string> l)
        {
            sampleSet = _sampleSet;
            outputCount = o;
            inputCount = i;
            labels = l;
            sampleSize = sampleSet.Count;
        }
    }
}
