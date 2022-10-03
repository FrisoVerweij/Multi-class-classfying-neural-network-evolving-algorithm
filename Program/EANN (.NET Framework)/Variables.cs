namespace EANN
{
    static public class Variables
    {
        public static string PATHTOTRAINSET = "../../../../Datasets/XorTrainSet.txt";
        public static string PATHTOTESTSET = "../../../../Datasets/XorTestSet.txt";
        public static string PATHTOREPORT = "../../../../Datasets";
        public static string RUNNAME = "Run1";

        //Mutation probabilities
        public static float ADDNEURON = 0.06f;
        public static float ADDLINK = 0.06f;
        public static float REMOVENEURON = 0.06f;
        public static float REMOVELINK = 0.02f;
        // The probability of the weight changing mutation is 1 subtracted by the above values.

        public static int POPULATIONSIZE = 100;
        public static int ITERATIONCOUNT = 1000;
        public static int ELITECOUNT = 10;
        public static int THREADCOUNT = 12;

        public static float NEURONPENALTY = 0.03f;
        public static float LINKPENALTY = 0.005f;
    }
}
