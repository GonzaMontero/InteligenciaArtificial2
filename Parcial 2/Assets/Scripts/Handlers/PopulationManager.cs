using System.Collections.Generic;
using UnityEngine;
using Entities.Agents;
using Utils;

namespace Handlers
{
    public class PopulationManager : MonoBehaviour
    {
        [Header("Agent Team Data")]
        [SerializeField] private Agent teamAgentPrefab = default;
        [SerializeField] private Vector3 agentSpawnOffset = Vector3.zero;
        [SerializeField] Agent.Team teamID;

        public List<Agent> agentsInTeam {  get; private set; } = new List<Agent>();

        private int totalAgents = 0;

        private int movedAgentsCount = 0;
        private int actedAgentsCount = 0;

        public System.Action OnAllAgentsStoppedMoving;
        public System.Action OnAllAgentsStoppedActing;

        public float GetTeamCurrentFitness()
        {
            float currentFitness = 0;
            for(short i = 0 ; i < agentsInTeam.Count; i++)
            {
                currentFitness += agentsInTeam[i].Fitness;
            }
            return currentFitness;
        }

        public void CreateAgents(int agentAmount, int rowToSpawn, Vector2Int terrainCount)
        {
            totalAgents = agentAmount; 

            List<int> possiblePositions = new List<int>(terrainCount.x);
            Calculations calculations = new Calculations();

            for (int i=0; i<terrainCount.x; i++)
            {
                possiblePositions.Add(i);
            }

            calculations.ShuffleList(possiblePositions);

            for(int i = 0; i < agentAmount; i++)
            {
                Vector3 position = Vector3.zero;
                position.x = possiblePositions[i];
                position.y = rowToSpawn;
                position.z = 0;
                position += agentSpawnOffset;

                var agent = Instantiate(teamAgentPrefab, position, Quaternion.identity);

                Vector2Int positionInt = new Vector2Int
                {
                    x = possiblePositions[i],
                    y = rowToSpawn
                };

                agent.SetPosition(positionInt);
                agent.SetTeam(teamID);

                agentsInTeam.Add(agent);
                
                RegisterAgentEvents(agent);
            }
        }

        #region Manager Functions

        public void StartAllAgentsMoving()
        {
            movedAgentsCount = 0;
            foreach(var agent in agentsInTeam)
            {
                agent.StartMoving();
            }
        }

        private void AgentStoppedMoving()
        {
            movedAgentsCount++;
            if (movedAgentsCount < agentsInTeam.Count) return;
            OnAllAgentsStoppedMoving?.Invoke();
        }

        public void StartAllAgentsActing()
        {
            actedAgentsCount = 0;
            foreach (var agent in agentsInTeam)
            {
                agent.StartActing();
            }
        }

        private void AgentStoppedActing()
        {
            actedAgentsCount++;
            if (actedAgentsCount < agentsInTeam.Count) return;
            OnAllAgentsStoppedActing?.Invoke();
        }

        public void RegisterAgentEvents(Agent agent)
        {
            agent.OnAgentStopMoving += AgentStoppedMoving;
            agent.OnAgentStopActing += AgentStoppedActing;
        }

        public void DeRegisterAgentEvents(Agent agent)
        {
            agent.OnAgentStopMoving -= AgentStoppedMoving;
            agent.OnAgentStopActing -= AgentStoppedActing;
        }
        #endregion
    }
}