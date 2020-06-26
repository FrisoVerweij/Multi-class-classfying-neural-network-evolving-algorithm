Made by: Friso Verweij
Date: 26-06-2020

This program, along with the two datasets provided in this .zip file are part of my bachelor thesis.
This thesis is the final assignment of my bachelor Kunstmatige Intelligentie (Artificial Intelligence) at Utrecht University.

What is it?
This program features an evolutionary algorithm made to generate and evolve multi-class neural networks.

How to use:
After opening the Program.sln file, open the file Variables.cs in the solution manager.
Here you can change certain variables.
After all variables are set, you can run the program.

Variables:
PATHTOTRAINSET		= Path to train set
PATHTOTESTSET		= Path to test set
PATHTOREPORT		= When the algorithm finishes, two files will be made, containing collected data. Set a path to a desired folder to drop these.
RUNNAME			= Name of run. Only affects report file names
ADDNEURON		= Probability of adding a neuron
ADDLINK			= Probability of adding a link
REMOVENEURON		= Probability of removing a neuron
REMOVELINK		= Probability of removing link
POPULATIONSIZE		= Indicates population size
ITERATIONCOUNT		= Indicates number of iterations
ELITECOUNT		= Indicates number of best-performing individuals without mutation applied to them
THREADCOUNT		= Indicates number of threads used for evalutation and mutation process
NEURONPENALTY		= Indicates penalty value for each neuron
LINKPENALTY		= Indicates penalty value for each link
