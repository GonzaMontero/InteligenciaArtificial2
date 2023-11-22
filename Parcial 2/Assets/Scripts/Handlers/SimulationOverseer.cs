using Entities.Agents;
using Entities.Food;
using Handlers.Cam;
using Handlers.Files;
using Handlers.Map;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Handlers.Simulation
{
    [System.Serializable]
    public class Team
    {
        public Population.PopulationManager PopulationManager;
        public StartConfig StartConfig;
    }

    public class SimulationOverseer : MonoBehaviour
    {
        [Header("Handlers and Crucial Items")]
        [SerializeField] private List<Team> teams = null;
        [SerializeField] private MapHandler mapHandler = null;
        [SerializeField] private FoodHandler foodHandler = null;
        [SerializeField] private CameraHandler cameraHandler = null;
        [SerializeField] private Button pauseButton = null;
        [SerializeField] private Button stopButton = null;

        [SerializeField] private TMP_Text turnAmountText = null;
        [SerializeField] private int maxTurnsAllowed = 0;
        [SerializeField] private bool saveBestAgentOfEachTeam = false;

        private bool simulationStarted = false;
        private int teamsNeededForBegin = 0;
        private int currentTurn = 0;
        private float delayForNextTurn = 0f;
        private float time = 0f;

        private int totalFood = 0;

        private List<(string,float)> lastAgentSaved = new List<(string,float)>();

        private void Start()
        {
            teamsNeededForBegin = teams.Count;
            Init();
        }

        private void Update()
        {
            InitializeSimulation();

            if (simulationStarted)
            {
                UpdateTurnWhenNeeded();
            }
        }

        private void FixedUpdate()
        {
            if (!simulationStarted)
                return;

            if (!CheckIfAllAgentsDone())
            {
                UpdateTeams();
            }
        }

        public void Init()
        {
            simulationStarted = false;
            lastAgentSaved.Clear();

            mapHandler.Init();

            pauseButton.onClick.AddListener(OnPauseButtonClick);
            stopButton.onClick.AddListener(OnStopButtonClick);

            pauseButton.gameObject.SetActive(false);
            stopButton.gameObject.SetActive(false);

            for(int i=0;i<teams.Count; i++)
            {
                if (teams[i] != null)
                {
                    teams[i].StartConfig.Init(mapHandler.gridSize.x, teams[i].PopulationManager);
                    teams[i].PopulationManager.onTeamWipe += OnPauseButtonClick;
                }
            }
        }

        private void InitializeSimulation()
        {
            if (simulationStarted)
                return;

            int teamsReady = 0;

            for(int i = 0; i < teams.Count; i++)
            {
                if (teams[i]!= null && teams[i].StartConfig.IsTeamReady)
                {
                    teamsReady++;
                }
            }

            if(teamsReady == teamsNeededForBegin)
            {
                simulationStarted = true;
                OnStartedSimulation();
            }
        }

        private void UpdateTeams()
        {
            for(int i = 0; i < teams.Count; i++)
            {
                if (teams[i] != null)
                {
                    if (teams[i].PopulationManager != null)
                    {
                        teams[i].PopulationManager.UpdatePopulation();
                    }
                }
            }
        }

        private void UpdateTurnWhenNeeded()
        {
            if (currentTurn < maxTurnsAllowed)
            {
                if (CheckIfAllAgentsDone())
                {
                    if (time < delayForNextTurn)
                    {
                        time += Time.deltaTime;
                    }
                    else 
                    {
                        time = 0;
                        currentTurn++;

                        turnAmountText.text = "Turn: " + currentTurn.ToString();

                        for(int i = 0;i < teams.Count; i++)
                        {
                            if (teams[i] != null && teams[i].PopulationManager != null)
                            {
                                teams[i].PopulationManager.UpdateTurn(currentTurn);
                                OnTurnEnded();
                            }
                        }
                    }
                }
            }
            else
            {
                currentTurn = maxTurnsAllowed;

                if (currentTurn == maxTurnsAllowed)
                {
                    turnAmountText.text = "Turns: Simulation Finished";
                    OnTurnEnded();
                    OnEndedAllTurns();
                }
            }
        }

        private bool CheckIfAllAgentsDone()
        {
            int completedTeams = 0;

            for(int i = 0; i < teams.Count; i++)
            {
                if (teams[i]!=null && teams[i].PopulationManager != null)
                {
                    if (teams[i].PopulationManager.AllAgentsDone())
                        completedTeams++;
                }
            }

            return completedTeams == teams.Count;
        }

        private void OnTurnEnded()
        {
            CheckAgentsOnSamePosition();
            AgentEatFood();
        }

        private void CheckAgentsOnSamePosition()
        {
            List<AgentMind> agentsOnPosition = new List<AgentMind>();
            int randomDecider = 0;

            for(int i = 0; i < teams[0].PopulationManager.agentsInTeam.Count; i++)
            {
                for (int j = 0; j < teams[1].PopulationManager.agentsInTeam.Count; j++)
                {
                    if (teams[0].PopulationManager.agentsInTeam[i].transform.position ==
                        teams[1].PopulationManager.agentsInTeam[j].transform.position)
                    {
                        agentsOnPosition.Add(teams[0].PopulationManager.agentsInTeam[i]);
                        agentsOnPosition.Add(teams[1].PopulationManager.agentsInTeam[j]);
                    }
                }

                if (agentsOnPosition.Count > 2)
                {
                    randomDecider = Random.Range(0, agentsOnPosition.Count);

                    for (short j = 0; j < agentsOnPosition.Count; j++)
                    {
                        if (j != randomDecider)
                        {
                            for (int k = 0; k < teams.Count; k++)
                            {
                                teams[k].PopulationManager.RemoveAgentOnTeam(agentsOnPosition[j]);
                            }
                        }
                    }
                }

                agentsOnPosition.Clear();
            }

            

            //for(short i = 0 ; i < mapHandler.Map.Count; i++)
            //{
            //    for(int j = 0 ; j < teams.Count; j++)
            //    {
            //        for(int k = 0; k < teams[j].PopulationManager.agentsInTeam.Count; k++)
            //        {
            //            if (teams[j].PopulationManager.agentsInTeam[k] != null &&
            //                mapHandler.Map.ContainsKey(new Vector2Int((int)teams[j].PopulationManager.agentsInTeam[k].transform.position.x,
            //                (int)teams[j].PopulationManager.agentsInTeam[k].transform.position.y)))
            //                    agentsOnPosition.Add(teams[j].PopulationManager.agentsInTeam[k]);
            //        }
            //    }

            //    if(agentsOnPosition.Count > 2)
            //    {
            //        randomDecider = Random.Range(0, agentsOnPosition.Count);

            //        for(short j = 0; j < agentsOnPosition.Count; j++)
            //        {
            //            if(j != randomDecider)
            //            {
            //                for(int k = 0; k < teams.Count; k++)
            //                {
            //                    teams[k].PopulationManager.RemoveAgentOnTeam(agentsOnPosition[j]);
            //                }
            //            }
            //        }
            //    }
            //}
        }

        private void AgentEatFood()
        {
            FoodItem foodItem;
            for (int j = 0; j < teams.Count; j++)
            {
                for (int k = 0; k < teams[j].PopulationManager.agentsInTeam.Count; k++)
                {
                    foodItem = foodHandler.MapContainsFood(new Vector2Int((int)teams[j].PopulationManager.agentsInTeam[k].transform.position.x,
                    (int)teams[j].PopulationManager.agentsInTeam[k].transform.position.y));

                    if (foodItem != null && teams[j].PopulationManager.agentsInTeam[k] != null)
                        teams[j].PopulationManager.agentsInTeam[k].AgentBehaviour.EatFood(foodItem);                      
                }
            }
        }

        private void OnStartedSimulation()
        {
            pauseButton.gameObject.SetActive(true);
            stopButton.gameObject.SetActive(true);

            totalFood = 0;

            for(int i = 0; i < teams.Count; i++)
            {
                if (teams[i] != null)
                {
                    totalFood += teams[i].PopulationManager.PopulationCount;
                    teams[i].StartConfig.gameObject.SetActive(false);

                    List<Vector2Int> finalTeamPositions = new List<Vector2Int>();

                    if (teams[i].PopulationManager.ID == "1")
                    {
                        List<Cell> leftToRightCells = mapHandler.GetLeftToRightCellsInColumn(0);

                        for(int j = 0; j < leftToRightCells.Count; j++)
                        {
                            if (leftToRightCells[i] != null)
                            {
                                finalTeamPositions.Add(leftToRightCells[j].Position);
                            }
                        }
                    }
                    else
                    {
                        List<Cell> rightToLeftCells = mapHandler.GetRightToLefttCellsInColumn(mapHandler.gridSize.y);

                        for(int j = 0; j < rightToLeftCells.Count; j++)
                        {
                            if (rightToLeftCells[j] != null)
                            {
                                finalTeamPositions.Add(rightToLeftCells[j].Position);
                            }
                        }
                    }

                    teams[i].PopulationManager.StartSimulation(finalTeamPositions, mapHandler, foodHandler, LoadBestAgentFromTeam(i));
                }
            }

            foodHandler.Init(mapHandler.GetRandomUniquePositions(totalFood));
            mapHandler.SetGeneratedFoodOnCells(foodHandler.FoodInMap);
        }

        private void OnEndedAllTurns()
        {
            currentTurn = 0;
            turnAmountText.text = "Turn: " + currentTurn.ToString();

            if (saveBestAgentOfEachTeam)
            {
                SaveBestAgentOfEachTeam();
            }

            for(int i = 0; i < teams.Count; i++)
            {
                if (teams[i] != null)
                    teams[i].PopulationManager.EndedGeneration();
            }

            foodHandler.Unload();
            mapHandler.ClearFoodOnCells();


            foodHandler.Init(mapHandler.GetRandomUniquePositions(totalFood));
            mapHandler.SetGeneratedFoodOnCells(foodHandler.FoodInMap);
        }

        private void OnPauseButtonClick()
        {
            for (int i = 0; i < teams.Count; i++)
            {
                if (teams[i] != null)
                {
                    teams[i].PopulationManager.PauseSimulation();
                }
            }
        }

        private void OnStopButtonClick()
        {
            if (saveBestAgentOfEachTeam)
            {
                SaveBestAgentOfEachTeam();
            }

            for (int i = 0; i < teams.Count; i++)
            {
                if (teams[i] != null)
                {
                    teams[i].StartConfig.gameObject.SetActive(true);
                    teams[i].StartConfig.OnStopSimulation();
                    teams[i].PopulationManager.StopSimulation();
                }
            }

            simulationStarted = false;

            pauseButton.gameObject.SetActive(false);
            stopButton.gameObject.SetActive(false);

            cameraHandler.ResetCamera();
            foodHandler.Unload();

            currentTurn = 0;
            turnAmountText.text = "Turn: " + currentTurn.ToString();
        }

        private void SaveBestAgentOfEachTeam()
        {
            for (int i = 0; i < teams.Count; i++)
            {
                if (teams[i] != null)
                {
                    bool needUpdateBestAgent = true;
                    AgentMind bestTeamAgent = teams[i].PopulationManager.GetBestAgent();

                    if (lastAgentSaved != null)
                    {
                        (string, float) correctSavedData = lastAgentSaved.Find(data => data.Item1 == teams[i].PopulationManager.ID);

                        if (bestTeamAgent != null && bestTeamAgent.AgentData.genome.fitness > correctSavedData.Item2)
                        {
                            lastAgentSaved.Remove(correctSavedData);
                            needUpdateBestAgent = true;
                        }
                        else
                        {
                            needUpdateBestAgent = false;
                        }
                    }

                    if (needUpdateBestAgent)
                    {
                        if (bestTeamAgent != null)
                        {
                            if (!lastAgentSaved.Contains((teams[i].PopulationManager.ID,
                                    bestTeamAgent.Genome.fitness)))
                            {
                                lastAgentSaved.Add((teams[i].PopulationManager.ID,
                                    bestTeamAgent.Genome.fitness));
                            }

                            bestTeamAgent.AgentData.generation = teams[i].PopulationManager.generation;

                            FileHandler<AgentData>.Save(bestTeamAgent.AgentData, teams[i].PopulationManager.ID, 
                                teams[i].PopulationManager.generation.ToString(), bestTeamAgent.Genome.fitness.ToString(), 
                                bestTeamAgent.Genome.foodEaten.ToString());
                        }
                    }
                }
            }
        }

        private AgentData LoadBestAgentFromTeam(int iterationTeam)
        {
            AgentData bestAgent = null;

            if (string.IsNullOrEmpty(teams[iterationTeam].StartConfig.FileNameToLoad.text))
                return null;

            bestAgent = FileHandler<AgentData>.Load(teams[iterationTeam].PopulationManager.ID,
                teams[iterationTeam].StartConfig.FileNameToLoad.text);

            return bestAgent;
        }
    }
}

