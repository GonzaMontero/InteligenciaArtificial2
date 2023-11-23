using System;
using UnityEngine;
using Handlers;
using System.Collections.Generic;

namespace Configurations
{
    [CreateAssetMenu(fileName = "Simulation", menuName = "Configurations/Simulation", order = 1)]
    public class SimulationConfiguration : ScriptableObject
    {
        [Header("Agents")]
        [SerializeField] private List<PopulationManager> populationManagers = new List<PopulationManager>();

        [Header("Food")]
        [SerializeField] private FoodHandler foodHandler;

        [Header("Terrain")]
        [SerializeField] TerrainHandler terrainHandler;

        [Header("Global Settings")]
        [SerializeField] private int turnsPerGeneration = 500;
        [SerializeField] private int generationBeforeEvolutionStart = 20;

        public int TurnsPerGeneration
        {
            get => turnsPerGeneration;
            set => turnsPerGeneration = value;
        }

        public int GenerationBeforeEvolutionStart
        {
            get => generationBeforeEvolutionStart;
            set => generationBeforeEvolutionStart = value;
        }
    }
}

