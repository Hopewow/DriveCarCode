using UnityEngine;
using Random = UnityEngine.Random;
using System.ComponentModel.DataAnnotations;

public class GeneticManager : MonoBehaviour
{
    [Header("References")]
    public CarController CarController;

    [Header("Controls")]
    public int startPopulation = 85;
    [Range(0.0f, 1.0f)]
    public float mutationRate = 0.25f;

    // How the two parents are combined.
    [Header("Crossover Controls")]
    public int bestAgentSelection = 4;
    public int numberToCrossOver = 16;

    private List<int> genePool = new();

    private int naturallySelected;

    private List<NNet> population = new();

    [Header("Public View")]
    public int currentGeneration;
    public int currentGenome = 0;

    private void Start()
    {
        CreatePopulation();
    }

    private void CreatePopulation()
    {
        population = new();
        FillPopulationRandom(population, 0); // Fills out population with random values
        ResetToCurrentGenome();
    }

    private void FillPopulationRandom(List<NNet> newPop, int startingIndex)
    {
        while (startingIndex < startPopulation)
        {
            int next = newPop.Count;
            newPop.Add(new NNet());
            newPop[next].Initialise(CarController.layers); // Initialises random neural network
            startingIndex++;
        }
    }

    private void ResetToCurrentGenome()
    {
        CarController.ResetWithNetwork(population[currentGenome]);
    }

    public void Death(float fitness, NNet network)
    {
        if (currentGenome < population.Count - 1)
        {
            // To keep track of all genomes fitness.
            population[currentGenome].fitness = fitness;
            currentGenome++;
            ResetToCurrentGenome();
        }
        else
        {
            RePopulation();
        }
    }

    public void RePopulation()
    {
        genePool.Clear(); // Every generation clears the genepool to start over.
        currentGeneration++;
        naturallySelected = 0;

        SortPopulation();

        List<NNet> newPop = Selection();
        CrossOver(newPop);
        Mutate(newPop);

        FillPopulationRandom(newPop, naturallySelected);

        population = newPop;
        currentGenome = 0;

        ResetToCurrentGenome();
    }

    /* Every cross over done between best and worst
     * two children will be created out from it.
     */
    private void CrossOver(List<NNet> newPop)
    {
        for (int i = 0; i < numberToCrossOver; i += 2)
        {
            int AIndex = i; // Parent 1
            int BIndex = i + 1; // Parent 2

            if (genePool.Count >= 1)
            {
                bool SelectedRandomGenePool = false;
                while (!SelectedRandomGenePool)
                {
                    AIndex = genePool[Random.Range(0, genePool.Count)];
                    BIndex = genePool[Random.Range(0, genePool.Count)];

                    if (AIndex != BIndex)
                    {
                        SelectedRandomGenePool = true;
                    }
                }
            }

            NNet Child1 = new();
            NNet Child2 = new();

            Child1.Initialise(CarController.layers);
            Child2.Initialise(CarController.layers);

            Child1.fitness = 0;
            Child2.fitness = 0;

            // Randomise each weight from parents
            for (int w = 0; w < Child1.weights.Length; w++)
            {
                Child1.weights[w] = (Random.Range(0.0f, 1.0f) < 0.5f) ? population[AIndex].weights[w] : population[BIndex].weights[w];
                Child2.weights[w] = (Random.Range(0.0f, 1.0f) < 0.5f) ? population[AIndex].weights[w] : population[BIndex].weights[w];

                for (int w2 = 0; w2 < Child1.weights[w].Length - 1; w2++)
                {
                    Child1.weights[w2] = (Random.Range(0.0f, 1.0f) < 0.5f) ? population[AIndex].weights[w2] : population[BIndex].weights[w2];
                    Child2.weights[w2] = (Random.Range(0.0f, 1.0f) < 0.5f) ? population[AIndex].weights[w2] : population[BIndex].weights[w2];

                    for (int w3 = 0; w3 < Child1.weights[w2].Length - 1; w3++)
                    {
                        Child1.weights[w3] = (Random.Range(0.0f, 1.0f) < 0.5f) ? population[AIndex].weights[w3] : population[BIndex].weights[w3];
                        Child2.weights[w3] = (Random.Range(0.0f, 1.0f) < 0.5f) ? population[AIndex].weights[w3] : population[BIndex].weights[w3];
                    }
                }
            }

            // Randomise each biases from parents
            for (int b = 0; b < Child1.biases.Length; b++)
            {
                Child1.biases[b] = (Random.Range(0.0f, 1.0f) < 0.5f) ? population[AIndex].biases[b] : population[BIndex].biases[b];
                Child2.biases[b] = (Random.Range(0.0f, 1.0f) < 0.5f) ? population[AIndex].biases[b] : population[BIndex].biases[b];

                for (int b2 = 0; b2 < Child1.biases[b].Length - 1; b2++)
                {
                    Child1.biases[b2] = (Random.Range(0.0f, 1.0f) < 0.5f) ? population[AIndex].biases[b2] : population[BIndex].biases[b2];
                    Child2.biases[b2] = (Random.Range(0.0f, 1.0f) < 0.5f) ? population[AIndex].biases[b2] : population[BIndex].biases[b2];
                }
            }

            // Important to increment naturallySelected again so we do not override anything.
            newPop.Add(Child1);
            naturallySelected++;

            newPop.Add(Child2);
            naturallySelected++;
        }
    }

    private void Mutate(List<NNet> newPop)
    {
        for (int i = 0; i < naturallySelected; i++)
        {
            if (i > bestAgentSelection)
            {
                newPop[i].Mutate(mutationRate, 0.1f);
            }
        }
    }

    private List<NNet> Selection()
    {
        List<NNet> newPopulation = new();

        // Select best from the top of population and reset fitness.
        for (int i = 0; i < bestAgentSelection; i++)
        {
            newPopulation.Add(population[i]);
            newPopulation[naturallySelected].fitness = 0;
            naturallySelected++;

            /*
             * How many times we are going to add it into the genePool.
             * Means that the higher the fitness the more likely it is to be selected 
             * from the gene pool for the children.
             */
            int f = Mathf.RoundToInt(population[i].fitness * 10);

            for (int c = 0; c < f; c++)
            {
                genePool.Add(i);
            }
        }

        return newPopulation;
    }

    private void SortPopulation()
    {
        population = population.OrderByDescending(x => x.fitness).ToList();
        print(population[0].fitness);
    }
}
