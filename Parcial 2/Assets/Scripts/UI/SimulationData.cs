using Handlers.Population;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SimulationData : MonoBehaviour
    {
        [SerializeField] private TMP_Text generationsCountTxt;
        [SerializeField] private TMP_Text bestFitnessTxt;
        [SerializeField] private TMP_Text avgFitnessTxt;
        [SerializeField] private TMP_Text worstFitnessTxt;
        [SerializeField] private TMP_Text actualPopulation;
        [SerializeField] private TMP_Text maxPopulation;
        [SerializeField] private TMP_Text timerTxt;
        [SerializeField] private Slider timerSlider;

        private int lastGeneration = 0;

        private PopulationManager populationManager = null;

        void OnEnable()
        {
            generationsCountTxt.text = "Generations - " + lastGeneration;
            bestFitnessTxt.text = "Best Fitness - " + populationManager.bestFitness;
            avgFitnessTxt.text = "Average Fitness - " + populationManager.avgFitness;
            worstFitnessTxt.text = "Worst Fitness - " + populationManager.worstFitness;

            actualPopulation.text = populationManager.PopulationCount.ToString();
            maxPopulation.text = "/ " + populationManager.PopulationCount.ToString();
        }

        void LateUpdate()
        {
            if (lastGeneration != populationManager.generation)
            {
                lastGeneration = populationManager.generation;
                generationsCountTxt.text = "Generations - " + lastGeneration;
                bestFitnessTxt.text = "Best Fitness - " + populationManager.bestFitness;
                avgFitnessTxt.text = "Average Fitness - " + populationManager.avgFitness;
                worstFitnessTxt.text = "Worst Fitness - " + populationManager.worstFitness;

                actualPopulation.text = populationManager.actualPopulation.ToString();
                maxPopulation.text = "/ " + populationManager.PopulationCount.ToString();
            }
        }

        public void Init(PopulationManager populationManager)
        {
            this.populationManager = populationManager;

            timerSlider.onValueChanged.AddListener(OnTimerChange);

            timerTxt.text = "Time - " + populationManager.IterationCount;
        }

        private void OnTimerChange(float value)
        {
            populationManager.IterationCount = (int)value;
            timerTxt.text = "Time - " + populationManager.IterationCount;
        }
    }
}

