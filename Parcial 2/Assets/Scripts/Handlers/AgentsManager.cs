using System.Collections.Generic;
using IA.Gameplay;
using UnityEngine;

namespace IA.Managers
{
    public class AgentsManager
    {
        private readonly List<IAgent> _agentsList = new();
        private int _movedAgentsCount = 0;
        private int _actedAgentsCount = 0;

        public System.Action onAllAgentsStoppedMoving;
        public System.Action onAllAgentsStoppedActing;

        public void StartAllAgentsMoving()
        {
            _movedAgentsCount = 0;
            foreach (var agent in _agentsList)
            {
                agent.StartMoving();
            }
        }
        
        private void AgentStoppedMoving()
        {
            _movedAgentsCount++;
            if (_movedAgentsCount < _agentsList.Count) return;
            onAllAgentsStoppedMoving?.Invoke();
        }

        public void StartAllAgentsActing()
        {
            _actedAgentsCount = 0;
            foreach (var agent in _agentsList)
            {
                agent.StartActing();
            }
        }
        
        private void AgentStoppedActing()
        {
            _actedAgentsCount++;
            if (_actedAgentsCount < _agentsList.Count) return;
            onAllAgentsStoppedActing?.Invoke();
        }
        
        public void RegisterAgent(IAgent agent)
        {
            _agentsList.Add(agent);
            agent.OnAgentStopMoving += AgentStoppedMoving;
            agent.OnAgentStopActing += AgentStoppedActing;
        }

        public void UnRegisterAgent(IAgent agent)
        {
            agent.OnAgentStopMoving -= AgentStoppedMoving;
            agent.OnAgentStopActing -= AgentStoppedActing;
            _agentsList.Remove(agent);
        }
    }



}
