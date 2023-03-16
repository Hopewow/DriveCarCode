using System.Collections.Generic;
using UnityEngine;
using System;

public class NNet : MonoBehaviour
{

    private int[] layers;//layers    
    private float[][] neurons;//neurons    
    public float[][] biases;//biasses    
    public float[][][] weights;//weights    
    public float fitness = 0;//fitness

    public void Initialise(int[] layers)
    {
        this.layers = new int[layers.Length];
        for (int i = 0; i < layers.Length; i++)
        {
            this.layers[i] = layers[i];
        }
        InitNeurons();
        InitBiases();
        InitWeights();
    }

    public NNet InitialiseCopy()
    {
        NNet N = new();

        N.layers = layers;
        N.neurons = neurons;
        N.biases = biases;
        N.weights = weights;
        N.fitness = fitness;

        return N;
    }

    //create empty storage array for the neurons in the network.
    private void InitNeurons()
    {
        List<float[]> neuronsList = new List<float[]>();
        for (int i = 0; i < layers.Length; i++)
        {
            neuronsList.Add(new float[layers[i]]);
        }
        neurons = neuronsList.ToArray();
    }

    //initializes and populates array for the biases being held within the network.
    private void InitBiases()
    {
        List<float[]> biasList = new List<float[]>();
        for (int i = 0; i < layers.Length; i++)
        {
            float[] bias = new float[layers[i]];
            for (int j = 0; j < layers[i]; j++)
            {
                bias[j] = UnityEngine.Random.Range(-0.5f, 0.5f);
            }
            biasList.Add(bias);
        }
        biases = biasList.ToArray();
    }

    //initializes random array for the weights being held in the network.
    private void InitWeights()
    {
        List<float[][]> weightsList = new List<float[][]>();
        for (int i = 1; i < layers.Length; i++)
        {
            List<float[]> layerWeightsList = new List<float[]>();
            int neuronsInPreviousLayer = layers[i - 1];
            for (int j = 0; j < neurons[i].Length; j++)
            {
                float[] neuronWeights = new float[neuronsInPreviousLayer];
                for (int k = 0; k < neuronsInPreviousLayer; k++)
                {
                    neuronWeights[k] = UnityEngine.Random.Range(-1.0f, 1.0f);
                }
                layerWeightsList.Add(neuronWeights);
            }
            weightsList.Add(layerWeightsList.ToArray());
        }
        weights = weightsList.ToArray();
    }

    public float activate(float value)
    {
        return (float)Math.Tanh(value);
    }

    //feed forward, inputs >==> outputs.
    public float[] FeedForward(float[] inputs)
    {
        for (int i = 0; i < inputs.Length; i++)
        {
            neurons[0][i] = inputs[i]; // Setting input value
        }
        for (int i = 1; i < layers.Length; i++)
        {
            int layer = i - 1;
            for (int j = 0; j < neurons[i].Length; j++)
            {
                float value = 0f;
                for (int k = 0; k < neurons[i - 1].Length; k++)
                {
                    value += weights[i - 1][j][k] * neurons[i - 1][k]; // Runs through all the weight calculation to the neuron value
                }
                neurons[i][j] = activate(value + biases[i][j]); // Adds the bias afterwards.
            }
        }
        return neurons[neurons.Length - 1];
    }

    //used as a simple mutation function for any genetic implementations.
    public void Mutate(float chance, float val)
    {
        for (int i = 0; i < biases.Length; i++)
        {
            for (int j = 0; j < biases[i].Length; j++)
            {
                biases[i][j] = (UnityEngine.Random.Range(0.0f, 1.0f) < chance) ? biases[i][j] += UnityEngine.Random.Range(-val, val) : biases[i][j];
            }
        }

        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    weights[i][j][k] = (UnityEngine.Random.Range(0.0f, 1.0f) < chance) ? weights[i][j][k] += UnityEngine.Random.Range(-val, val) : weights[i][j][k];

                }
            }
        }
    }

    public (float, float) RunNetwork(float a, float b, float c)
    {
        float[] input = { a, b, c };
        return (Sigmoid(FeedForward(input)[0]), FeedForward(input)[1]);
    }

    private float Sigmoid(float s)
    {
        return (1 / (1 + Mathf.Exp(-s)));
    }

}
