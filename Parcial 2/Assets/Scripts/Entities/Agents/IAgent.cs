using System;

namespace IA.Gameplay
{
    public interface IAgent
    {
        void StartMoving();
        Action OnAgentStopMoving { get; set; }
        void StartActing();
        Action OnAgentStopActing { get; set; }
        void Think();
        void Die();
        void Flee();
    }
}
