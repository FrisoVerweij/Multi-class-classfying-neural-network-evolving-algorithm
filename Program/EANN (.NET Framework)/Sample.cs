namespace EANN
{
    // Is a data point. Contains input values and an output value
    class Sample
    {
        public float[] input;
        public string output;
        public Sample(float[] _input, string _output)
        {
            input = _input;
            output = _output;
        }
    }
}
