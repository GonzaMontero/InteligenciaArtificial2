using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using IA.Gameplay;

namespace IA.Managers
{
    public class GenerationManager
    {
	    
        private const int MaxGenerationsPerAgent = 3;
        private const int MinFoodEatenToReproduce = 2;

        public void CreateAgentBrains(Agent agent, GenomeData data)
        {
            NeuralNetwork brain = new NeuralNetwork();
            brain.AddFirstNeuronLayer(data.inputsCount, data.bias, data.sigmoid);
            for (int i = 0; i < data.hiddenLayers; i++)
            {
                brain.AddNeuronLayer(data.neuronCountPerHiddenLayer, data.bias, data.sigmoid);
            }
            brain.AddNeuronLayer(data.outputsCount, data.bias, data.sigmoid);

            Genome genome = new Genome(brain.GetTotalWeightsCount());
            if (data.genome is { Length: > 0 })
            {
                genome.genome = data.genome;
            }
            
            agent.SetIntelligence(genome, brain);
        }

        public float[] GetBestGenomeOfAgentsList(List<Agent> agentsList)
        {
	        agentsList.Sort((a, b) => a.Fitness > b.Fitness ? 0 : 1);
	        return agentsList[0].AgentGenome.genome;
        }
        
        public List<AgentGenerationData> GetNewGenerationData(GenomeData genomeData, List<Agent> agentsList)
        {

	        List<AgentGenerationData> previousGenerationDataList = new List<AgentGenerationData>();

	        foreach (var agent in agentsList)
	        {
		        previousGenerationDataList.Add(new AgentGenerationData
		        {
					brain = agent.AgentBrain,
					fitness = agent.Fitness,
					foodEaten = agent.FoodEaten,
					generationsLived = agent.GenerationsLived,
					genome = agent.AgentGenome
		        });
	        }

            List<AgentGenerationData> newGenerationDataList = new List<AgentGenerationData>();

            foreach (var agent in agentsList)
            {
	            agent.GenerationsLived++;
	            if (agent.GenerationsLived <= MaxGenerationsPerAgent && agent.FoodEaten > 0)
	            {
		            AgentGenerationData data = new AgentGenerationData
		            {
			            brain = agent.AgentBrain,
			            genome = agent.AgentGenome,
			            generationsLived = agent.GenerationsLived,
			            foodEaten = 0,
			            fitness = 0
		            };
		            newGenerationDataList.Add(data);
	            }
            }

            List<AgentGenerationData> parentsDataList = new List<AgentGenerationData>();
            parentsDataList.AddRange(previousGenerationDataList);
	        parentsDataList.RemoveAll(i => i.foodEaten < MinFoodEatenToReproduce);
	            
	        int parentsCount = parentsDataList.Count % 2 == 0
				? parentsDataList.Count
		        : parentsDataList.Count - 1;

	        if (parentsCount < 2) return newGenerationDataList;
	        
	        List<AgentGenerationData> childrenDataList = new List<AgentGenerationData>();
                
	        for (int i = 0; i < parentsCount; i += 2)
	        {
		        Crossover(parentsDataList[i].genome, parentsDataList[i + 1].genome, out var child1Genome, out var child2Genome,
			        genomeData.mutationChance, genomeData.mutationRate);

		        AgentGenerationData child1Data = new AgentGenerationData
		        {
			        brain = CreateBaseBrain(genomeData, child1Genome),
			        genome = child1Genome,
			        generationsLived = 0,
			        foodEaten = 0,
			        fitness = 0
		        };
		        
		        AgentGenerationData child2Data = new AgentGenerationData
		        {
			        brain = CreateBaseBrain(genomeData, child2Genome),
			        genome = child2Genome,
			        generationsLived = 0,
			        foodEaten = 0,
			        fitness = 0
		        };
		        
		        childrenDataList.Add(child1Data);
		        childrenDataList.Add(child2Data);
	        }
	            
	        newGenerationDataList.AddRange(childrenDataList);

            return newGenerationDataList;
        }

        public List<AgentGenerationData> GetEliteOfGeneration(GenomeData genomeData, List<Agent> agentsList)
        {
	        List<AgentGenerationData> generationDataList = new List<AgentGenerationData>();
	        
	        if (agentsList.Count == 0)
		        return generationDataList;
	        
	        Genome child1Genome;
	        Genome child2Genome;
	        
	        if (agentsList.Count > 1)
	        {
				agentsList.Sort((a, b) => a.Fitness > b.Fitness ? 0 : 1);
		        Crossover(agentsList[0].AgentGenome, agentsList[1].AgentGenome, out child1Genome, out child2Genome,
			        genomeData.mutationChance, genomeData.mutationRate);
	        }
	        else
	        {
		        child1Genome = agentsList[0].AgentGenome;
		        child2Genome = agentsList[0].AgentGenome;
	        }
	        
	        for (int i = 0; i < agentsList.Count / 2; i++)
	        {
		        AgentGenerationData child1Data = new AgentGenerationData
		        {
			        brain = CreateBaseBrain(genomeData, child1Genome),
			        genome = child1Genome,
			        generationsLived = 0,
			        foodEaten = 0,
			        fitness = 0
		        };
		        
		        AgentGenerationData child2Data = new AgentGenerationData
		        {
			        brain = CreateBaseBrain(genomeData, child2Genome),
			        genome = child2Genome,
			        generationsLived = 0,
			        foodEaten = 0,
			        fitness = 0
		        };
		        
		        generationDataList.Add(child1Data);
		        generationDataList.Add(child2Data);
	        }

	        if (agentsList.Count > generationDataList.Count)
	        {
		        AgentGenerationData child1Data = new AgentGenerationData
		        {
			        brain = CreateBaseBrain(genomeData, child1Genome),
			        genome = child1Genome,
			        generationsLived = 0,
			        foodEaten = 0,
			        fitness = 0
		        };
		        generationDataList.Add(child1Data);
	        }
	        
	        return generationDataList;
        }


        private void Crossover(Genome mom, Genome dad, out Genome child1, out Genome child2, float mutationChance, float mutationRate)
        {
	        child1 = new Genome();
	        child2 = new Genome();

	        child1.genome = new float[mom.genome.Length];
	        child2.genome = new float[mom.genome.Length];

	        int pivot = Random.Range(0, mom.genome.Length);

	        for (int i = 0; i < pivot; i++)
	        {
		        child1.genome[i] = mom.genome[i];

		        if (ShouldMutate(mutationChance))
			        child1.genome[i] += Random.Range(-mutationRate, mutationRate);

		        child2.genome[i] = dad.genome[i];

		        if (ShouldMutate(mutationChance))
			        child2.genome[i] += Random.Range(-mutationRate, mutationRate);
	        }

	        for (int i = pivot; i < mom.genome.Length; i++)
	        {
		        child2.genome[i] = mom.genome[i];

		        if (ShouldMutate(mutationChance))
			        child2.genome[i] += Random.Range(-mutationRate, mutationRate);

		        child1.genome[i] = dad.genome[i];

		        if (ShouldMutate(mutationChance))
			        child1.genome[i] += Random.Range(-mutationRate, mutationRate);
	        }
        }

        bool ShouldMutate(float mutationChance)
        {
	        return Random.Range(0.0f, 1.0f) < mutationChance;
        }


        private NeuralNetwork CreateBaseBrain(GenomeData data, Genome genomeData = null)
        {
	        NeuralNetwork brain = new NeuralNetwork();
	        brain.AddFirstNeuronLayer(data.inputsCount, data.bias, data.sigmoid);
	        for (int i = 0; i < data.hiddenLayers; i++)
	        {
		        brain.AddNeuronLayer(data.neuronCountPerHiddenLayer, data.bias, data.sigmoid);
	        }
	        brain.AddNeuronLayer(data.outputsCount, data.bias, data.sigmoid);

	        Genome genome = new Genome(brain.GetTotalWeightsCount());
	        if (genomeData != null)
	        {
		        genome.genome = genomeData.genome;
	        }

	        return brain;
        }

        public class AgentGenerationData
        {
            public Genome genome;
            public NeuralNetwork brain;
            public int generationsLived;
            public int foodEaten;
            public float fitness;
        }
        
    }
}
