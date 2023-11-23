using IA.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IA.UI
{
    public class UIPreGameplay : MonoBehaviour
    {

        [SerializeField] private GameObject panelGameplay;
        
        [Header("Pre Gameplay UI")]
        [SerializeField] private Button simulateButton;
        [SerializeField] private TextMeshProUGUI turnsPerGenerationText;
        [SerializeField] private Slider turnsPerGenerationSlider;
        [SerializeField] private TextMeshProUGUI generationsBeforeEvolutionStartText;
        [SerializeField] private Slider generationsBeforeEvolutionStartSlider;
        [SerializeField] private Toggle reviveSimulationToggle;

        private void Awake()
        {
            simulateButton.onClick.AddListener(delegate
            {
                GameManager.Instance.StartSimulation();
                panelGameplay.SetActive(true);
                gameObject.SetActive(false);
            });
            
            reviveSimulationToggle.onValueChanged.AddListener(delegate(bool value)
            {
                GameManager.Instance.ReviveSimulation = value;
            });
            
        }

        private void Start()
        {
            Screen.SetResolution(1000, 650, false);
            GameManager.Instance.SimulationSpeed = 1f;
            SetPreGameplayUI();
        }

        private void SetPreGameplayUI()
        {
            turnsPerGenerationSlider.wholeNumbers = true;
            
            int currentTurnsPerGeneration = GameManager.Instance.GameplayConfig.TurnsPerGeneration;
            turnsPerGenerationSlider.SetValueWithoutNotify(currentTurnsPerGeneration);
            turnsPerGenerationText.text = "Turns Per Generation: " + currentTurnsPerGeneration;
            
            turnsPerGenerationSlider.onValueChanged.AddListener(delegate(float value)
            {
                GameManager.Instance.GameplayConfig.TurnsPerGeneration = (int)value;
                turnsPerGenerationText.text = "Turns Per Generation: " + value;
            });

            generationsBeforeEvolutionStartSlider.wholeNumbers = true;
            
            int currentGenerationsBeforeEvolutionStart = GameManager.Instance.GameplayConfig.GenerationsBeforeEvolutionStart;
            generationsBeforeEvolutionStartSlider.SetValueWithoutNotify(currentGenerationsBeforeEvolutionStart);
            generationsBeforeEvolutionStartText.text = "Generations before evolution start: " + currentGenerationsBeforeEvolutionStart;
            
            generationsBeforeEvolutionStartSlider.onValueChanged.AddListener(delegate(float value)
            {
                GameManager.Instance.GameplayConfig.GenerationsBeforeEvolutionStart = (int)value;
                generationsBeforeEvolutionStartText.text = "Generations before evolution start: " + value;
            });
        }
        
    }
}

