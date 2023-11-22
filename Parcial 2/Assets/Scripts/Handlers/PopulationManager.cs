using Entities.Agents;
using Entities.Food;
using Handlers.Map;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Handlers.Population
{
    public class PopulationManager : MonoBehaviour
    {
        [Header("Team Configurations")]
        public string ID = null;
        public GameObject AgentInstance;
        public int PopulationCount = 40;
        public int IterationCount = 1;

        public int EliteCount = 4;
        public float MutationChance = 0.10f;
        public float MutationRate = 0.01f;

        public int InputsCount = 4;
        public int HiddenLayers = 1;
        public int OutputsCount = 2;
        public int NeuronsCountPerHL = 7;
        public float Bias = 1f;
        public float Sigmoid = 0.5f;

        [Header("Saving")]
        public bool saveDataOnNextEvolve = false;

        GeneticAlgorithm geneticAlgorithm;
        private const string idDataBestBrain = "bestBrainInGenerations";
        private Genome bestGenome = null;
        private int actualTurn = 1;
        private MapHandler mapHandler = null;
        public FoodHandler foodHandler = null;

        public List<AgentMind> agentsInTeam = new List<AgentMind>();
        public List<Genome> population = new List<Genome>();
        public List<NeuralNetwork> brains = new List<NeuralNetwork>();

        bool isRunning = false;

        public int generation
        {
            get; private set;
        }
        public float bestFitness
        {
            get; private set;
        }
        public float actualPopulation
        {
            get; private set;
        }
        public float avgFitness
        {
            get; private set;
        }
        public float worstFitness
        {
            get; private set;
        }

        public Action onTeamWipe = null;

        public float GetBestFitness()
        {
            float fitness = 0;
            foreach(Genome g in population)
            {
                if(fitness < g.fitness)
                {
                    fitness = g.fitness;
                }
            }

            return fitness;
        }

        public float GetAverageFitness()
        {
            float fitness = 0;
            foreach (Genome g in population)
            {
                fitness += g.fitness;
            }
            return fitness/population.Count;
        }

        public float GetWorstFitness()
        {
            float fitness = float.MaxValue;
            foreach (Genome g in population)
            {
                if (fitness > g.fitness)
                {
                    fitness = g.fitness;
                }
            }

            return fitness;
        }

        public AgentMind GetBestAgent()
        {
            if(agentsInTeam.Count == 0)
            {
                Debug.Log("No Agents Left in Team");
                return null;
            }

            AgentMind agent = agentsInTeam[0];
            Genome bestGenome = population[0];

            for(int i = 0; i < population.Count; i++)
            {
                if (agentsInTeam[i].state == State.Alive && population[i].fitness > bestGenome.fitness)
                {
                    bestGenome = population[i];
                    agent = agentsInTeam[i];
                }
            }

            return agent;
        }

        public List<AgentMind> SeatchForAgentsThatCanCross()
        {
            List<AgentMind> agents = new List<AgentMind>();
            return agents;
        }

        public bool AllAgentsDone()
        {
            List<AgentMind> agentsNotDone = agentsInTeam.Where(agent => agent.CurrentTurn != actualTurn).ToList();

            return agentsNotDone.Count < 1;
        }

        private void Awake()
        {
            Load();
        }

        public void Load()
        {
            if (ID == null)
                return;

            PopulationCount = PlayerPrefs.GetInt("PopulationCount_" + ID, 100);
            EliteCount = PlayerPrefs.GetInt("EliteCount_" + ID, 0);
            MutationChance = PlayerPrefs.GetFloat("MutationChance_" + ID, 5);
            MutationRate = PlayerPrefs.GetFloat("MutationRate_" + ID, 3);
            InputsCount = PlayerPrefs.GetInt("InputsCount_" + ID, 25);
            HiddenLayers = PlayerPrefs.GetInt("HiddenLayers_" + ID, 2);
            OutputsCount = PlayerPrefs.GetInt("OutputsCount_" + ID, 4);
            NeuronsCountPerHL = PlayerPrefs.GetInt("NeuronsCountPerHL_" + ID, 14);
            Bias = PlayerPrefs.GetFloat("Bias_" + ID, -2);
            Sigmoid = PlayerPrefs.GetFloat("P_" + ID, 0.27f);
        }

        public void Save()
        {
            if (ID == null)
                return;

            PlayerPrefs.SetFloat("PopulationCount_" + ID, 100);
            PlayerPrefs.SetFloat("EliteCount_" + ID, 0);
            PlayerPrefs.SetFloat("MutationChance_" + ID, 5);
            PlayerPrefs.SetFloat("MutationRate_" + ID, 3);
            PlayerPrefs.SetFloat("InputsCount_" + ID, 25);
            PlayerPrefs.SetFloat("HiddenLayers_" + ID, 2);
            PlayerPrefs.SetFloat("OutputsCount_" + ID, 4);
            PlayerPrefs.SetFloat("NeuronsCountPerHL_" + ID, 14);
            PlayerPrefs.SetFloat("Bias_" + ID, -2);
            PlayerPrefs.SetFloat("P_" + ID, 0.27f);
        }

        public void StartSimulation(List<Vector2Int> initialPositions, MapHandler map, FoodHandler food, AgentData loadedAgentData)
        {
            this.mapHandler = map;
            this.foodHandler = food;

            Save();

            geneticAlgorithm = new GeneticAlgorithm(EliteCount, MutationChance, MutationRate);

            GenerateInitialPopulation(initialPositions, loadedAgentData);

            isRunning = true;
        }

        public void EndedGeneration()
        {
            Epoch();
        }

        public void PauseSimulation()
        {
            isRunning = false;
        }

        public void StopSimulation()
        {
            Save();

            isRunning = false;

            generation = 0;

            DestroyBadAgents();
        }

        public void UpdateTurn(int currentTurn)
        {
            actualTurn = currentTurn;
        }

        private void DestroyBadAgents()
        {
            foreach (AgentMind go in agentsInTeam)
                Destroy(go.gameObject);

            agentsInTeam.Clear();
            population.Clear();
            brains.Clear();
        }

        void DestroyUselessAgents(int amount)
        {
            List<AgentMind> toDestroyAgents = new List<AgentMind>();

            int originalAmountAIs = agentsInTeam.Count;

            for (int i = 0; i < amount; i++)
            {
                if (amount < originalAmountAIs)
                {
                    toDestroyAgents.Add(agentsInTeam[i]);
                }
                else
                {
                    toDestroyAgents.AddRange(agentsInTeam);
                }
            }

            for (int i = 0; i < toDestroyAgents.Count; i++)
            {
                if (toDestroyAgents[i] != null)
                {
                    agentsInTeam.Remove(toDestroyAgents[i]);
                    Destroy(toDestroyAgents[i].gameObject);
                }
            }

            if (agentsInTeam.Count < 1)
            {
                onTeamWipe?.Invoke();
                population.Clear();
            }
        }

        private void Epoch()
        {
            generation++;

            bestFitness = GetBestFitness();
            avgFitness = GetAverageFitness();
            worstFitness = GetWorstFitness();

            List<Genome> genomesThatSurvived = new List<Genome>();

            for(int i=0;i < agentsInTeam.Count; i++)
            {
                if (agentsInTeam[i]!= null)
                {
                    agentsInTeam[i].OnGenerationEnded(out Genome genomeSurvived);

                    if(genomeSurvived != null)
                    {
                        genomeSurvived.generationsSurvived++;

                        if(genomeSurvived.generationsSurvived < 4)
                        {
                            genomesThatSurvived.Add(genomeSurvived);
                        }
                    }
                }
            }

            List<Genome> newGenomes = geneticAlgorithm.Epoch(genomesThatSurvived.ToArray(), PopulationCount, 2).ToList();

            population.Clear();

            for (int i = 0; i < newGenomes.Count; i++)
            {
                if (i < PopulationCount)
                {
                    population.Add(newGenomes[i]);
                }
            }

            if (population.Count < PopulationCount)
            {
                int difference = PopulationCount - population.Count;
                DestroyUselessAgents(difference);
            }

            actualPopulation = population.Count;

            // Set the new genomes as each NeuralNetwork weights 
            for (int i = 0; i < population.Count; i++)
            {
                NeuralNetwork brain = brains[i];
                brain.SetWeights(newGenomes[i].genome);
                agentsInTeam[i].SetBrain(newGenomes[i], brain);
            }
        }

        public void UpdatePopulation()
        {
            if (!isRunning)
                return;

            float dt = Time.fixedDeltaTime;

            for (int i = 0; i < Mathf.Clamp((float)IterationCount, 1, 100); i++)
            {
                foreach (AgentMind agent in agentsInTeam)
                {
                    // Think!! 
                    if (agent.state == State.Alive)
                    {
                        agent.Think(dt, actualTurn, IterationCount, mapHandler, foodHandler);
                    }
                }
            }
        }

        public void RemoveAgentOnTeam(AgentMind agent)
        {
            if (agentsInTeam.Contains(agent))
            {
                agentsInTeam.Remove(agent);
                brains.Remove(agent.NeuralNetwork);
                population.Remove(agent.Genome);
                Destroy(agent.gameObject);
            }
        }

        private void GenerateInitialPopulation(List<Vector2Int> initialPositions, AgentData loadedAgentData = null)
        {
            generation = 0;
            DestroyBadAgents();

            for(int i = 0; i < PopulationCount; i++)
            {
                NeuralNetwork brain = null;
                Genome genome = null;

                if(loadedAgentData != null)
                {
                    brain = CreateBrain(loadedAgentData.neuralNetwork);
                    genome = loadedAgentData.genome;
                }
                else
                {
                    brain = CreateBrain();
                    genome = new Genome(brain.GetTotalWeightsCount());
                    brain.SetWeights(genome.genome);
                }

                brains.Add(brain);
                population.Add(genome);

                AgentMind generatedAgent = CreateAgent(initialPositions[i], genome, brain);
                agentsInTeam.Add(generatedAgent);
            }

            bestFitness = GetBestFitness();
            avgFitness = GetAverageFitness();
            worstFitness = GetWorstFitness();
        }

        private AgentMind CreateAgent(Vector2Int position, Genome genome, NeuralNetwork brain)
        {
            Vector3 finalPosition = new Vector3(position.x, position.y, 0f);
            GameObject go = Instantiate<GameObject>(AgentInstance, finalPosition, Quaternion.identity);
            AgentMind b = go.GetComponent<AgentMind>();
            b.SetBrain(genome, brain, false);
            b.SetInitialPosition(finalPosition);
            return b;
        }

        private NeuralNetwork CreateBrain()
        {
            NeuralNetwork brain = new NeuralNetwork(NeuronsCountPerHL, OutputsCount);

            // Add first neuron layer that has as many neurons as inputs
            brain.AddFirstNeuronLayer(InputsCount, Bias, Sigmoid);

            for (int i = 0; i < HiddenLayers; i++)
            {
                // Add each hidden layer with custom neurons count
                brain.AddNeuronLayer(NeuronsCountPerHL, Bias, Sigmoid);
            }

            // Add the output layer with as many neurons as outputs
            brain.AddNeuronLayer(OutputsCount, Bias, Sigmoid);

            return brain;
        }

        private NeuralNetwork CreateBrain(NeuralNetwork neuralNetwork)
        {
            NeuralNetwork brain = new NeuralNetwork(neuralNetwork.neuronCountsPerLayer, neuralNetwork.outputsCount);

            if (neuralNetwork.layers.Count < 1)
                return null;

            Bias = neuralNetwork.layers[0].bias;
            Sigmoid = neuralNetwork.layers[0].p;
            NeuronsCountPerHL = neuralNetwork.neuronCountsPerLayer;

            // Add first neuron layer that has as many neurons as inputs
            brain.AddFirstNeuronLayer(neuralNetwork.inputsCount, Bias, Sigmoid);

            for (int i = 0; i < neuralNetwork.layers.Count; i++)
            {
                // Add each hidden layer with custom neurons count
                brain.AddNeuronLayer(NeuronsCountPerHL, neuralNetwork.layers[i].bias, neuralNetwork.layers[i].p);
            }

            // Add the output layer with as many neurons as outputs
            brain.AddNeuronLayer(neuralNetwork.outputsCount, Bias, Sigmoid);

            return brain;
        }
    }
}