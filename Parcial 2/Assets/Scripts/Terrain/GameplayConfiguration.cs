using System;
using System.Collections.Generic;
using UnityEngine;
using IA.Gameplay;
using IA.Managers;
using Random = UnityEngine.Random;

namespace IA.Configurations
{
    [CreateAssetMenu(fileName = "Gameplay Configuration", menuName = "ScriptableObjects/Gameplay Configuration", order = 1)]
    public class GameplayConfiguration : ScriptableObject
    {
        [Header("Terrain")]
        [SerializeField] private GameObject terrainPrefab = default;
        [SerializeField] private Vector2Int terrainCount = Vector2Int.zero;
        [SerializeField] private Vector3 eachTerrainSize = Vector3.one;

        [Header("Agents")] 
        [SerializeField] private Agent topAgentPrefabs = default;
        [SerializeField] private Agent botAgentPrefabs = default;
        [SerializeField] private Vector3 agentSpawnOffset = Vector3.zero;

        [Header("Food")] 
        [SerializeField] private Food foodPrefab = default;
        [SerializeField] private Vector3 foodSpawnOffset = Vector3.zero;
        
        [Header("General")]
        [SerializeField] private int turnsPerGeneration = 500;
        [SerializeField] private int generationsBeforeEvolutionStart = 20;

        public List<Agent> RedAgentsList { get; private set; } = new List<Agent>();
        public List<Agent> GreenAgentsList { get; private set; }= new List<Agent>();

        public float GetRedAgentsCurrentFitness()
        {
            float fitness = 0;
            foreach (var agent in RedAgentsList)
            {
                fitness += agent.Fitness;
            }
            return fitness;
        }

        public float GetGreenAgentsCurrentFitness()
        {
            float fitness = 0;
            foreach (var agent in GreenAgentsList)
            {
                fitness += agent.Fitness;
            }
            return fitness;
        }
        
        private int _greenAgentsAmount = 0;
        private int _redAgentsAmount = 0;
        
        private List<Food> _foodList = new List<Food>();
        public Vector2Int TerrainCount => terrainCount;
        public int TurnsPerGeneration
        {
            get => turnsPerGeneration;
            set => turnsPerGeneration = value;
        }

        public int GenerationsBeforeEvolutionStart
        {
            get => generationsBeforeEvolutionStart;
            set => generationsBeforeEvolutionStart = value;
        }
        
        public int StartingFood { get; set; }

        public void CreateTerrain(Transform parent)
        {
            for (int i = 0; i < terrainCount.x; i++)
            {
                for (int j = 0; j < terrainCount.y; j++)
                {
                    Vector3 position = Vector3.zero;
                    position.x = eachTerrainSize.x * i;
                    position.z = eachTerrainSize.z * j;
                    Instantiate(terrainPrefab, position, Quaternion.identity, parent);
                }
            }
        }

        public void CreateAgents(int greenAgentAmount, int redAgentAmount)
        {
            _greenAgentsAmount = greenAgentAmount;
            GreenAgentsList.Clear();
            GreenAgentsList = new List<Agent>(greenAgentAmount);
            
            List<int> possiblePositions = new List<int>(terrainCount.x);
            for (int i = 0; i < terrainCount.x; i++)
            {
                possiblePositions.Add(i);
            }
            ShuffleList(possiblePositions);

            for (int i = 0; i < greenAgentAmount; i++)
            {
                Vector3 position = Vector3.zero;
                position.x = possiblePositions[i];
                position.z = 0;
                position += agentSpawnOffset;
                
                var agent = Instantiate(botAgentPrefabs, position, Quaternion.identity);

                Vector2Int positionInt = new Vector2Int
                {
                    x = possiblePositions[i],
                    y = 0
                };

                agent.SetPosition(positionInt);
                agent.SetTeam(Agent.Team.Green);
                
                GreenAgentsList.Add(agent);
            }
            
            _redAgentsAmount = redAgentAmount;
            RedAgentsList.Clear();
            RedAgentsList = new List<Agent>(redAgentAmount);
            
            possiblePositions.Clear();
            for (int i = 0; i < terrainCount.x; i++)
            {
                possiblePositions.Add(i);
            }
            ShuffleList(possiblePositions);
            
            for (int i = 0; i < redAgentAmount; i++)
            {
                Vector3 position = Vector3.zero;
                position.x = possiblePositions[i];
                position.z = (terrainCount.y - 1) * eachTerrainSize.z;
                position += agentSpawnOffset;
                
                var agent = Instantiate(topAgentPrefabs, position, Quaternion.identity);
                
                Vector2Int positionInt = new Vector2Int
                {
                    x = possiblePositions[i],
                    y = terrainCount.y - 1
                };
                
                agent.SetPosition(positionInt);
                agent.SetTeam(Agent.Team.Red);
                
                RedAgentsList.Add(agent);
            }
            
        }

        public void CreateAgents(List<GenerationManager.AgentGenerationData> greenGenerationData, List<GenerationManager.AgentGenerationData> redGenerationData)
        {
            _greenAgentsAmount = greenGenerationData.Count;
            GreenAgentsList.Clear();
            GreenAgentsList = new List<Agent>(greenGenerationData.Count);
            
            List<int> possiblePositions = new List<int>(terrainCount.x);
            for (int i = 0; i < terrainCount.x; i++)
            {
                possiblePositions.Add(i);
            }
            ShuffleList(possiblePositions);

            for (int i = 0; i < greenGenerationData.Count; i++)
            {
                Vector3 position = Vector3.zero;
                position.x = possiblePositions[i];
                position.z = 0;
                position += agentSpawnOffset;
                
                var agent = Instantiate(botAgentPrefabs, position, Quaternion.identity);

                Vector2Int positionInt = new Vector2Int
                {
                    x = possiblePositions[i],
                    y = 0
                };

                agent.SetPosition(positionInt);
                agent.SetTeam(Agent.Team.Green);
                
                GreenAgentsList.Add(agent);
                agent.SetIntelligence(greenGenerationData[i].genome, greenGenerationData[i].brain);
                agent.GenerationsLived = greenGenerationData[i].generationsLived;
            }
            
            _redAgentsAmount = redGenerationData.Count;
            RedAgentsList.Clear();
            RedAgentsList = new List<Agent>(redGenerationData.Count);
            
            possiblePositions.Clear();
            for (int i = 0; i < terrainCount.x; i++)
            {
                possiblePositions.Add(i);
            }
            ShuffleList(possiblePositions);
            
            for (int i = 0; i < redGenerationData.Count; i++)
            {
                Vector3 position = Vector3.zero;
                position.x = possiblePositions[i];
                position.z = (terrainCount.y - 1) * eachTerrainSize.z;
                position += agentSpawnOffset;
                
                var agent = Instantiate(topAgentPrefabs, position, Quaternion.identity);
                
                Vector2Int positionInt = new Vector2Int
                {
                    x = possiblePositions[i],
                    y = terrainCount.y - 1
                };
                
                agent.SetPosition(positionInt);
                agent.SetTeam(Agent.Team.Red);
                
                RedAgentsList.Add(agent);
                agent.SetIntelligence(redGenerationData[i].genome, redGenerationData[i].brain);
                agent.GenerationsLived = redGenerationData[i].generationsLived;
            }
            
        }
        
        public void CreateStartingFood()
        {
            StartingFood = _greenAgentsAmount + _redAgentsAmount;
            _foodList.Clear();
            
            for (int i = 0; i < StartingFood; i++)
            {
                Vector3 position = Vector3.zero;
                Vector2Int intPosition = new Vector2Int
                {
                    x = Random.Range(0, terrainCount.x),
                    y = Random.Range(1, terrainCount.y - 1)
                };

                if (!_foodList.Exists(food => food.CurrentPosition == intPosition))
                {
                    position.x = eachTerrainSize.x * intPosition.x;
                    position.z = eachTerrainSize.z * intPosition.y;
                    position += foodSpawnOffset;
                    var food = Instantiate(foodPrefab, position, Quaternion.identity);
                    food.CurrentPosition = intPosition;
                    _foodList.Add(food);
                }
                else
                {
                    i--;
                }
            }
        }

        public void CreateFood(int foodAmount)
        {
            _foodList.Clear();
            _foodList = new List<Food>(foodAmount);
            for (int i = 0; i < foodAmount; i++)
            {
                Vector3 position = Vector3.zero;
                Vector2Int intPosition = new Vector2Int
                {
                    x = Random.Range(0, terrainCount.x),
                    y = Random.Range(1, terrainCount.y - 1)
                };

                if (!_foodList.Exists(food => food.CurrentPosition == intPosition))
                {
                    position.x = eachTerrainSize.x * intPosition.x;
                    position.z = eachTerrainSize.z * intPosition.y;
                    position += foodSpawnOffset;
                    var food = Instantiate(foodPrefab, position, Quaternion.identity);
                    food.CurrentPosition = intPosition;
                    _foodList.Add(food);
                }
                else
                {
                    i--;
                }
            }
        }
        
        public void ClearAllAgentsAndFood()
        {
            foreach (var food in _foodList)
            {
                Destroy(food.gameObject);
            }
            _foodList.Clear();
            
            foreach (var botAgent in GreenAgentsList)
            {
                Destroy(botAgent.gameObject);
            }
            GreenAgentsList.Clear();
            
            foreach (var topAgent in RedAgentsList)
            {
                Destroy(topAgent.gameObject);
            }
            RedAgentsList.Clear();
        }
        
        public Vector3 GetPostMovementPosition(Agent agent, Movement.MoveDirection direction)
        {
            var positionInt = agent.CurrentPosition;
            
            switch (direction)
            {
                case Movement.MoveDirection.Down:
                    if (positionInt.y > 0)
                    {
                        positionInt.y--;
                    }
                    break;
                
                case Movement.MoveDirection.Up:
                    if (positionInt.y < terrainCount.y - 1)
                    {
                        positionInt.y++;
                    }
                    break;
                
                case Movement.MoveDirection.Left:
                    if (positionInt.x == 0) positionInt.x = terrainCount.x - 1;
                    else positionInt.x--;
                    break;
                
                case Movement.MoveDirection.Right:
                    if (positionInt.x == terrainCount.x - 1) positionInt.x = 0;
                    else positionInt.x++;
                    break;
            }
            
            Vector3 position = new Vector3
            {
                x = positionInt.x * eachTerrainSize.x,
                z = positionInt.y * eachTerrainSize.z
            };

            agent.CurrentPosition = positionInt;
            
            return position;
        }

        public List<Vector2Int> GetClosestFoods(Vector2Int agentPosition, int amount)
        {
            List<Vector2Int> closestFoodPositions = new List<Vector2Int>();

            List<Food> closestFoods = new List<Food>();
            closestFoods.AddRange(_foodList);
            closestFoods.Sort((a, b) => GetManhattanDistance(agentPosition, a.CurrentPosition) < GetManhattanDistance(agentPosition, b.CurrentPosition) ? 0 : 1);

            for (int i = 0; i < amount; i++)
            {
                if (i < closestFoods.Count)
                {
                    closestFoodPositions.Add(closestFoods[i].CurrentPosition);
                }
            }

            if (closestFoodPositions.Count == 0)
            {
                for (int i = 0; i < amount * 2; i++)
                {
                    closestFoodPositions.Add(Vector2Int.one * -1);
                }
            }
            else
            {
                if (closestFoodPositions.Count < amount)
                {
                    for (int i = 0; i < amount - closestFoodPositions.Count; i++)
                    {
                        closestFoodPositions.Add(closestFoodPositions[0]);
                    }
                }
                for (int i = 0; i < amount; i++)
                {
                    closestFoodPositions.Add(closestFoodPositions[0]);
                }
            }
            return closestFoodPositions;
        }

        public void AgentAct(Agent agent, Agent.Team team)
        {

            List<Agent> ownTeam;
            List<Agent> enemyTeam;

            if (team == Agent.Team.Green)
            {
                enemyTeam = RedAgentsList;
                ownTeam = GreenAgentsList;
            }
            else
            {
                enemyTeam = GreenAgentsList;
                ownTeam = RedAgentsList;
            }

            Vector2Int agentCurrentPosition = agent.CurrentPosition; 
            var enemiesInThisPosition = enemyTeam.FindAll(i => i.CurrentPosition == agentCurrentPosition);
            var alliesInThisPosition = ownTeam.FindAll(i => i.CurrentPosition == agentCurrentPosition);
            
            if (_foodList.Exists(i => i.CurrentPosition == agentCurrentPosition))
            {
                if (GameManager.Instance.EvolutionStarted)
                {
                    if (enemyTeam.Exists(i => i.CurrentPosition == agentCurrentPosition))
                    {
                        foreach (var enemies in enemiesInThisPosition)
                        {
                            if (!enemies.ActionPositive())
                            {
                                enemies.Flee();
                            }
                        }

                        foreach (var allies in alliesInThisPosition)
                        {
                            if (!allies.ActionPositive())
                            {
                                allies.Flee();
                            }
                        }
                    }

                    while (enemyTeam.Exists(i => i.CurrentPosition == agentCurrentPosition))
                    {
                        if (agentCurrentPosition != agent.CurrentPosition)
                            return;

                        var enemy = enemyTeam.Find(i => i.CurrentPosition == agentCurrentPosition);
                        if (agent.ActionPositive() && enemy.ActionPositive())
                        {
                            int random = Random.Range(0, 2);
                            if (random == 0)
                            {
                                enemyTeam.Remove(enemy);
                                enemy.Die();
                            }
                            else
                            {
                                ownTeam.Remove(agent);
                                agent.Die();
                                return;
                            }
                        }
                    }
                }

                var food = _foodList.Find(i => i.CurrentPosition == agentCurrentPosition);
                _foodList.Remove(food);
                food.GetEaten(agent);
            }
            else
            {

                if (!GameManager.Instance.EvolutionStarted)
                    return;

                while (enemyTeam.Exists(i => i.CurrentPosition == agentCurrentPosition))
                {
                    var enemy = enemyTeam.Find(i => i.CurrentPosition == agentCurrentPosition);
                    if (!agent.ActionPositive() && !enemy.ActionPositive())
                    {
                        agent.Flee();
                        enemy.Flee();
                    }
                    else if (agent.ActionPositive() && enemy.ActionPositive())
                    {
                        int random = Random.Range(0, 2);
                        if (random == 0)
                        {
                            enemyTeam.Remove(enemy);
                            enemy.Die();
                        }
                        else
                        {
                            ownTeam.Remove(agent);
                            agent.Die();
                            return;
                        }
                    }
                    else if(!agent.ActionPositive())
                    {
                        int random = Random.Range(0, 4);
                        if (random == 0)
                        {
                            agent.Flee();
                        }
                        else
                        {
                            ownTeam.Remove(agent);
                            agent.Die();
                        }
                        return;
                    }
                    else
                    {
                        int random = Random.Range(0, 4);
                        if (random == 0)
                        {
                            enemy.Flee();
                        }
                        else
                        {
                            enemyTeam.Remove(enemy);
                            enemy.Die();
                        }
                        return;
                    }
                }
            }
        }
        
        private void ShuffleList<T> (List<T> list)
        {
            var count = list.Count;
            var last = count - 1;
            for (var i = 0; i < last; ++i)
            {
                var r = Random.Range(i, count);
                (list[i], list[r]) = (list[r], list[i]);
            }
        }

        private int GetManhattanDistance(Vector2Int from, Vector2Int to)
        {
            return Math.Abs(to.x - from.x) + Math.Abs(to.y - from.y);
        }
        
    }
}
