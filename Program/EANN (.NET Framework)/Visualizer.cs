using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace EANN
{
    class Visualizer : Form
    {
        Graphics newGraphics;
        Pen pen;
        Panel panel;
        Network currentNetwork;

        // Sets up window
        public Visualizer()
        {
            pen = new Pen(Brushes.Red);
            Width = 1000;
            Height = 1000;
            pen = new Pen(Brushes.Black);
            panel = new Panel();
            panel.Width = 900;
            panel.Height = 900;
            panel.Location = new Point(50, 50);
            Controls.Add(panel);
            newGraphics = panel.CreateGraphics();
            Paint += new PaintEventHandler(PaintNetwork);
        }

        // Call to draw a given network
        public void DrawNetwork(Network network)
        {
            currentNetwork = network;
            Invalidate();
        }

        // Redraw the window
        private void PaintNetwork(object o, PaintEventArgs ea)
        {
            if (currentNetwork == null)
                return;
            newGraphics.Clear(Color.White);
            // Set up lists of nodes
            List<List<Neuron>> neuronInLayer = new List<List<Neuron>>();
            List<List<Point>> pointsInLayer = new List<List<Point>>();
            List<Neuron> outputLayer = new List<Neuron>();
            Dictionary<Neuron, Point> dictionary = new Dictionary<Neuron, Point>();

            // Fill lists of nodes
            foreach(Neuron n in currentNetwork.allNeurons)
            {
                if (n.neuronClass == currentNetwork.maxClass)
                    outputLayer.Add(n);
                else
                {
                    if(neuronInLayer.Count <= n.neuronClass)
                    {
                        for (int i = neuronInLayer.Count; i <= n.neuronClass; i++)
                        {
                            neuronInLayer.Add(new List<Neuron>());
                        }
                    }
                    neuronInLayer[n.neuronClass].Add(n);
                }
            }
            neuronInLayer.Add(outputLayer);

            // Determine node position on screen
            int verticalSegmentSize = panel.Height / (neuronInLayer.Count + 1);
            for (int i = 0; i < neuronInLayer.Count; i++)
            {
                pointsInLayer.Add(new List<Point>());
                int horizontalSegmentSize = panel.Width / (neuronInLayer[i].Count + 1);
                for (int j = 0; j < neuronInLayer[i].Count; j++)
                {
                    pointsInLayer[i].Add(new Point(horizontalSegmentSize * (j + 1), verticalSegmentSize * (i + 1)));
                }
            }

            // Couple node with position
            for (int i = 0; i < neuronInLayer.Count; i++)
            {
                for (int j = 0; j < neuronInLayer[i].Count; j++)
                {
                    dictionary.Add(neuronInLayer[i][j], pointsInLayer[i][j]);
                }
            }

            // Draw the network
            foreach(KeyValuePair<Neuron, Point> entry in dictionary)
            {
                foreach(Neuron n in entry.Key.outgoingConnections)
                    newGraphics.DrawLine(pen, entry.Value, dictionary[n]);
                newGraphics.FillEllipse(Brushes.White, entry.Value.X - 40, entry.Value.Y - 40, 80, 80);
                newGraphics.DrawEllipse(pen, entry.Value.X - 40, entry.Value.Y - 40, 80, 80);
            }
        }
    }
}
