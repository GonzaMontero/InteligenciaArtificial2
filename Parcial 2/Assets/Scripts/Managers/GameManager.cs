using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IA.Gameplay;
using IA.Configurations;

namespace IA.Managers
{
    public class GameManager : MonoBehaviour
    {

        public Action<int> OnTurnEnd;
        public Action<int> OnGenerationEnd;

        public static GameManager Instance { get; private set; } = null;
        
        [SerializeField] private GameplayConfiguration gameplayConfiguration = default;
        
        public GameplayConfiguration GameplayConfig => gameplayConfiguration;
        public GenomeData RedGenomeData { get; set; } = new GenomeData();
        public GenomeData GreenGenomeData { get; set; } = new GenomeData();
        public bool TextOn { get; set; } = true;
        public bool Started { get; private set; } = false;
        public bool Paused { get; set; } = true;
        public bool EvolutionStarted { get; private set; } = false;
        public bool AutoSaveGreen { get; set; } = false;
        public int AutoSaveGreenGenerationsCount { get; set; } = 200;
        
        public bool ReviveSimulation { get; set; } = false;
        
        public bool AutoSaveRed { get; set; } = false;
        public int AutoSaveRedGenerationsCount { get; set; } = 200;
        
        public float SimulationSpeed
        {
            get => _simulationSpeed;
            set
            {
                _simulationSpeed = value;
                _timeManager.TimeBetweenTicks = 1.0f / _simulationSpeed;
            }
        }
        
        private float _simulationSpeed = 1f;
        private bool _agentsTime = false;
        
        private TimeManager _timeManager;
        private AgentsManager _agentsManager;
        private GenerationManager _generationManager;
        
        private int _currentTurn = 0;
        private int _currentGeneration = 0;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            _timeManager = new TimeManager();
            _agentsManager = new AgentsManager();
            _generationManager = new GenerationManager();
        }

        private void Start()
        {
            GameObject terrainParent = new GameObject();
            terrainParent.transform.SetParent(transform);
            terrainParent.name = "Terrain Parent";
            
            gameplayConfiguration.CreateTerrain(terrainParent.transform);
            
            _timeManager.onActionTimeReached += StartAllAgentsMovingTime;
            _timeManager.SetCyclesAmount(gameplayConfiguration.TurnsPerGeneration);
            
            _agentsManager.onAllAgentsStoppedMoving += StartAllAgentsActionTime;
            _agentsManager.onAllAgentsStoppedActing += AllAgentsStoppedActing;
        }
        private void Update()
        {
            if (!Paused && !_agentsTime && Started) _timeManager.Update(Time.deltaTime);
        }
        private void OnDestroy()
        {
            _timeManager.onActionTimeReached -= StartAllAgentsMovingTime;
        }

        
        public void StartSimulation()
        {
            StartCoroutine(StartingSimulation());
        }

        private IEnumerator StartingSimulation()
        {
            yield return null;
            Started = true;
            Paused = false;
            _agentsTime = false;
            EvolutionStarted = _currentGeneration >= gameplayConfiguration.GenerationsBeforeEvolutionStart;
            
            gameplayConfiguration.CreateAgents(GreenGenomeData.populationCount, RedGenomeData.populationCount);
            gameplayConfiguration.CreateStartingFood();
            
            CreateBrainsWithSavedData();
        }

        private void CreateBrainsWithSavedData()
        {
            var botAgents = gameplayConfiguration.GreenAgentsList;
            foreach (var agent in botAgents)
            {
                _generationManager.CreateAgentBrains(agent, GreenGenomeData);
            }

            var topAgents = gameplayConfiguration.RedAgentsList;
            foreach (var agent in topAgents)
            {
                _generationManager.CreateAgentBrains(agent, RedGenomeData);
            }
        }
        public void StopSimulation()
        {
            Started = false;
            Paused = true;
            gameplayConfiguration.ClearAllAgentsAndFood();
            _timeManager.Reset();
            
            _currentTurn = 0;
            _currentGeneration = 0;
        }

        public float[] GetInputs(Agent agent)
        {
            var inputs = new float[44];

            int inputIndex = 0;
            inputs[inputIndex] = agent.CurrentPosition.x;
            inputIndex++;
            inputs[inputIndex] = agent.CurrentPosition.y;

            var closestFoodPosition = gameplayConfiguration.GetClosestFoods(agent.CurrentPosition, 10);

            for (int i = 0; i < closestFoodPosition.Count; i++)
            {
                inputIndex++;
                inputs[inputIndex] = closestFoodPosition[i].x;
                inputIndex++;
                inputs[inputIndex] = closestFoodPosition[i].y;
            }

            inputIndex++;
            inputs[inputIndex] = agent.FoodEaten;
            
            inputIndex++;
            inputs[inputIndex] = agent.Fitness;
            
            return inputs;
        }
        
        private void StartAllAgentsMovingTime()
        {
            _agentsTime = true;
            _agentsManager.StartAllAgentsMoving();
        }
        
        private void StartAllAgentsActionTime()
        {
            _agentsManager.StartAllAgentsActing();
        }
        
        private void AllAgentsStoppedActing()
        {
            _currentTurn++;
            if ( _currentTurn > gameplayConfiguration.TurnsPerGeneration)
            {
                _currentTurn = 0;
                _currentGeneration++;
                EvolutionStarted = _currentGeneration >= gameplayConfiguration.GenerationsBeforeEvolutionStart;

                List<GenerationManager.AgentGenerationData> greenGenerationData;
                List<GenerationManager.AgentGenerationData> redGenerationData;

                if (_currentGeneration < gameplayConfiguration.GenerationsBeforeEvolutionStart)
                {
                    greenGenerationData = _generationManager.GetEliteOfGeneration(GreenGenomeData, gameplayConfiguration.GreenAgentsList);
                    redGenerationData =  _generationManager.GetEliteOfGeneration(RedGenomeData, gameplayConfiguration.GreenAgentsList);
                }
                else
                {
                    greenGenerationData = _generationManager.GetNewGenerationData(GreenGenomeData, gameplayConfiguration.GreenAgentsList);
                    redGenerationData = _generationManager.GetNewGenerationData(RedGenomeData, gameplayConfiguration.RedAgentsList);
                }

                if (redGenerationData.Count == 0 && greenGenerationData.Count > 0)
                {
                    var customGenomeData = new GenomeData(GreenGenomeData);
                    customGenomeData.mutationRate *= 2; 
                    redGenerationData = _generationManager.GetNewGenerationData(customGenomeData, gameplayConfiguration.GreenAgentsList);
                }
                else if (greenGenerationData.Count == 0 && redGenerationData.Count > 0)
                {
                    var customGenomeData = new GenomeData(RedGenomeData);
                    customGenomeData.mutationRate *= 2; 
                    greenGenerationData = _generationManager.GetNewGenerationData(customGenomeData, gameplayConfiguration.RedAgentsList);
                }


                if (redGenerationData.Count == 0 && greenGenerationData.Count == 0)
                {
                    string greenTeamFilePath = "Dead Generation " + _currentGeneration + " ~ Team Green ~ " + DateTime.Now.Hour + "_" + DateTime.Now.Minute + "_" + DateTime.Now.Second + ".genomeData";
                    var bestGreenGenome = gameplayConfiguration.GreenAgentsList.Count > 0
                        ? _generationManager.GetBestGenomeOfAgentsList(gameplayConfiguration.GreenAgentsList)
                        : _generationManager.GetBestGenomeOfAgentsList(gameplayConfiguration.RedAgentsList);
                    
                    GreenGenomeData.genome = bestGreenGenome;
                    SaveSystem.SaveSystem.SaveToStreamingAssets(GreenGenomeData, greenTeamFilePath);
                    
                    string redTeamFilePath = "Dead Generation " + _currentGeneration + " ~ Team Red ~ " + DateTime.Now.Hour + "_" + DateTime.Now.Minute + "_" + DateTime.Now.Second + ".genomeData";
                    var bestRedGenome = gameplayConfiguration.RedAgentsList.Count > 0
                        ? _generationManager.GetBestGenomeOfAgentsList(gameplayConfiguration.RedAgentsList)
                        : _generationManager.GetBestGenomeOfAgentsList(gameplayConfiguration.GreenAgentsList);
                    RedGenomeData.genome = bestRedGenome;
                    SaveSystem.SaveSystem.SaveToStreamingAssets(RedGenomeData, redTeamFilePath);

                    if (ReviveSimulation)
                    {
                        gameplayConfiguration.ClearAllAgentsAndFood();
                        StartSimulation();
                        return;
                    }
                    
                }
                
                gameplayConfiguration.ClearAllAgentsAndFood();
                    
                gameplayConfiguration.CreateAgents(greenGenerationData, redGenerationData);
                gameplayConfiguration.CreateFood(gameplayConfiguration.StartingFood);
                
                OnGenerationEnd?.Invoke(_currentGeneration);

                if (AutoSaveGreen && _currentGeneration % AutoSaveGreenGenerationsCount == 0)
                {
                    string greenTeamFilePath = "Generation " + _currentGeneration + " ~ Team Green ~ " + DateTime.Now.Hour + "_" + DateTime.Now.Minute + "_" + DateTime.Now.Second;
                    var bestGreenGenome = _generationManager.GetBestGenomeOfAgentsList(gameplayConfiguration.GreenAgentsList);
                    GreenGenomeData.genome = bestGreenGenome;
                    SaveSystem.SaveSystem.SaveToStreamingAssets(GreenGenomeData, greenTeamFilePath);
                }

                if (AutoSaveRed && _currentGeneration % AutoSaveRedGenerationsCount == 0)
                {
                    string redTeamFilePath = "Generation " + _currentGeneration + " ~ Team Red ~ " + DateTime.Now.Hour + "_" + DateTime.Now.Minute + "_" + DateTime.Now.Second;
                    var bestRedGenome = _generationManager.GetBestGenomeOfAgentsList(gameplayConfiguration.RedAgentsList);
                    RedGenomeData.genome = bestRedGenome;
                    SaveSystem.SaveSystem.SaveToStreamingAssets(RedGenomeData, redTeamFilePath);
                }

                
            }
            _agentsTime = false;
            OnTurnEnd?.Invoke(_currentTurn);
        }
        
        public void RegisterAgent(IAgent agent) => _agentsManager.RegisterAgent(agent);
        public void UnRegisterAgent(IAgent agent) => _agentsManager.UnRegisterAgent(agent);

    }
}
