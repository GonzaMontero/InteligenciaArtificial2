using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.Agents
{
    public abstract class AgentBase : MonoBehaviour
    {
        #region Methods
        public virtual void StartMoving() { }
        public virtual void StartActing() { }
        public virtual void Think() { }
        public virtual void Die() { }
        public virtual void Flee() { }
        #endregion

        #region Actions
        public Action OnAgentStopMoving {  get; set; }
        public Action OnAgentStopActing { get; set; }
        #endregion
    }
}

