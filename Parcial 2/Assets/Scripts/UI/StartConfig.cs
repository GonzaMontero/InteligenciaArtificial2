using Handlers.Population;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [System.Serializable]
    public class UIItem
    {
        public TMP_Text text;
        public Slider slider;
    }

    public class StartConfig : MonoBehaviour
    {
        public UIItem populationCountItem;
        public UIItem eliteCountItem;
        public UIItem mutationChanceItem;
        public UIItem mutationRateItem;
        public UIItem hiddenLayersItem;
        public UIItem neuronsPerLayerItem;
        public UIItem biasItem;
        public UIItem sigmoidSlopeItem;
        public UIItem inputsItem;
        public UIItem outputsItem;

        [SerializeField] private Button startButton;
        [SerializeField] private SimulationData simulationScreen;

        [SerializeField] private TMP_InputField fileNameToLoad;

        [SerializeField] private Toggle teamSettingState;

        private PopulationManager populationManager = null;

        public bool IsTeamReady { get { return teamSettingState.isOn; } }
        public TMP_InputField FileNameToLoad { get { return fileNameToLoad; } }

        public void Init(int maxGridX, PopulationManager populationManager)
        {
            this.populationManager = populationManager;

            populationCountItem.slider.onValueChanged.AddListener(OnPopulationCountChange);
            eliteCountItem.slider.onValueChanged.AddListener(OnEliteCountChange);
            mutationChanceItem.slider.onValueChanged.AddListener(OnMutationChanceChange);
            mutationRateItem.slider.onValueChanged.AddListener(OnMutationRateChange);
            hiddenLayersItem.slider.onValueChanged.AddListener(OnHiddenLayersCountChange);
            neuronsPerLayerItem.slider.onValueChanged.AddListener(OnNeuronsPerLayerChange);
            biasItem.slider.onValueChanged.AddListener(OnBiasChange);
            sigmoidSlopeItem.slider.onValueChanged.AddListener(OnSigmoidSlopeChange);
            inputsItem.slider.onValueChanged.AddListener(OnInputsChange);
            outputsItem.slider.onValueChanged.AddListener(OnOutputsChange);

            populationCountItem.slider.minValue = 0;
            populationCountItem.slider.maxValue = maxGridX;

            populationCountItem.slider.value = populationManager.PopulationCount;
            eliteCountItem.slider.value = populationManager.EliteCount;
            mutationChanceItem.slider.value = Mathf.Round(populationManager.MutationChance * 100.0f);
            mutationRateItem.slider.value = Mathf.Round(populationManager.MutationRate * 100.0f);
            hiddenLayersItem.slider.value = populationManager.HiddenLayers;
            neuronsPerLayerItem.slider.value = populationManager.NeuronsCountPerHL;
            biasItem.slider.value = populationManager.Bias;
            sigmoidSlopeItem.slider.value = populationManager.Sigmoid;
            inputsItem.slider.value = populationManager.InputsCount;
            outputsItem.slider.value = populationManager.OutputsCount;

            startButton.onClick.AddListener(() => { OnStartButtonClick(false); });

            Refresh();

            teamSettingState.isOn = false;

            fileNameToLoad.onEndEdit.AddListener((text) => { ToggleOptions(string.IsNullOrEmpty(text)); });
        }

        public void OnStopSimulation()
        {
            simulationScreen.gameObject.SetActive(false);
            teamSettingState.isOn = false;
        }

        public void ToggleOptions(bool state)
        {
            populationCountItem.slider.interactable = state;
            eliteCountItem.slider.interactable = state;
            mutationChanceItem.slider.interactable = state;
            mutationRateItem.slider.interactable = state;
            hiddenLayersItem.slider.interactable = state;
            neuronsPerLayerItem.slider.interactable = state;
            biasItem.slider.interactable = state;
            sigmoidSlopeItem.slider.interactable = state;
            inputsItem.slider.interactable = state;
            outputsItem.slider.interactable = state;
        }

        private void OnPopulationCountChange(float value)
        {
            populationManager.PopulationCount = (int)value;

            populationCountItem.text.text = string.Format(populationCountItem.text.text, populationManager.PopulationCount);
        }

        private void OnEliteCountChange(float value)
        {
            populationManager.EliteCount = (int)value;

            populationCountItem.text.text = string.Format(populationCountItem.text.text, populationManager.EliteCount);
        }

        private void OnMutationChanceChange(float value)
        {
            populationManager.MutationChance = value / 100.0f;

            mutationChanceItem.text.text = string.Format(mutationChanceItem.text.text, (int)(populationManager.MutationChance * 100));
        }

        private void OnMutationRateChange(float value)
        {
            populationManager.MutationRate = value / 100.0f;

            mutationRateItem.text.text = string.Format(mutationRateItem.text.text, (int)(populationManager.MutationRate * 100));
        }

        private void OnHiddenLayersCountChange(float value)
        {
            populationManager.HiddenLayers = (int)value;


            hiddenLayersItem.text.text = string.Format(hiddenLayersItem.text.text, populationManager.HiddenLayers);
        }

        private void OnNeuronsPerLayerChange(float value)
        {
            populationManager.NeuronsCountPerHL = (int)value;

            neuronsPerLayerItem.text.text = string.Format(neuronsPerLayerItem.text.text, populationManager.NeuronsCountPerHL);
        }

        private void OnBiasChange(float value)
        {
            populationManager.Bias = -value;

            biasItem.text.text = string.Format(biasItem.text.text, populationManager.Bias.ToString("0.00"));
        }

        private void OnSigmoidSlopeChange(float value)
        {
            populationManager.Sigmoid = value;

            sigmoidSlopeItem.text.text = string.Format(sigmoidSlopeItem.text.text, populationManager.Sigmoid.ToString("0.00"));
        }


        private void OnStartButtonClick(bool bestAI)
        {
            simulationScreen.Init(populationManager);
            simulationScreen.gameObject.SetActive(true);

            teamSettingState.isOn = true;
        }

        private void OnInputsChange(float value)
        {
            populationManager.InputsCount = (int)value;

            inputsItem.text.text = string.Format(inputsItem.text.text, populationManager.InputsCount);
        }

        private void OnOutputsChange(float value)
        {
            populationManager.OutputsCount = (int)value;

            outputsItem.text.text = string.Format(outputsItem.text.text, populationManager.OutputsCount);
        }

        private void Refresh()
        {
            populationCountItem.text.text = string.Format(populationCountItem.text.text, populationManager.PopulationCount);
            populationCountItem.text.text = string.Format(populationCountItem.text.text, populationManager.EliteCount);
            mutationChanceItem.text.text = string.Format(mutationChanceItem.text.text, (int)(populationManager.MutationChance * 100));
            mutationRateItem.text.text = string.Format(mutationRateItem.text.text, (int)(populationManager.MutationRate * 100));
            hiddenLayersItem.text.text = string.Format(hiddenLayersItem.text.text, populationManager.HiddenLayers);
            neuronsPerLayerItem.text.text = string.Format(neuronsPerLayerItem.text.text, populationManager.NeuronsCountPerHL);
            biasItem.text.text = string.Format(biasItem.text.text, populationManager.Bias.ToString("0.00"));
            sigmoidSlopeItem.text.text = string.Format(sigmoidSlopeItem.text.text, populationManager.Sigmoid.ToString("0.00"));
            inputsItem.text.text = string.Format(inputsItem.text.text, populationManager.InputsCount);
            outputsItem.text.text = string.Format(outputsItem.text.text, populationManager.OutputsCount);
        }
    }
}