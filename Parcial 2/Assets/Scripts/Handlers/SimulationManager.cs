using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Configurations;

namespace Handlers
{
    public class SimulationManager : MonoBehaviour
    {
        public Action<int> OnTurnEnd;
        public Action<int> OnGenerationStart;

        public static SimulationManager Instance { get; private set; } = null;

        [SerializeField] private SimulationConfiguration simulationConfiguration = default;
        public SimulationConfiguration SimulationConfiguration => simulationConfiguration;


    }
}

