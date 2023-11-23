using IA.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IA.UI
{
    public class UIGameplay : MonoBehaviour
    {
        
        
        [Header("Simulation Flow")]
        [SerializeField] private GameObject panelPreGameplay;
        [SerializeField] private Button playPauseButton;
        [SerializeField] private Button stopSimulationButton;
        
        [Header("Simulation Speed")]
        [SerializeField] private Slider simulationSpeedSlider;
        [SerializeField] private TextMeshProUGUI simulationSpeedText;
        
        [Header("Simulation Information")]
        [SerializeField] private TextMeshProUGUI currentGenerationText;
        [SerializeField] private TextMeshProUGUI currentTurnText;
        [SerializeField] private TextMeshProUGUI currentGreenAgentsText;
        [SerializeField] private TextMeshProUGUI currentRedAgentsText;
        [SerializeField] private Button textsButton;
        
        private TextMeshProUGUI _playPauseButtonText;
        private TextMeshProUGUI _textsButtonText;

        private void Awake()
        {
            _playPauseButtonText = playPauseButton.GetComponentInChildren<TextMeshProUGUI>();
            _playPauseButtonText.text = "Pause Simulation";
            
            playPauseButton.onClick.AddListener(delegate
            {
                GameManager.Instance.Paused = !GameManager.Instance.Paused;
                _playPauseButtonText.text = GameManager.Instance.Paused ? "Resume Simulation" : "Pause Simulation";
            });
            
            _textsButtonText = textsButton.GetComponentInChildren<TextMeshProUGUI>();
            _textsButtonText.text = "Remove Text";
            
            textsButton.onClick.AddListener(delegate
            {
                GameManager.Instance.TextOn = !GameManager.Instance.TextOn;
                _textsButtonText.text = GameManager.Instance.TextOn ? "Remove Text" : "Add Text";
            });
            
            stopSimulationButton.onClick.AddListener(delegate
            {
                GameManager.Instance.StopSimulation();
                panelPreGameplay.gameObject.SetActive(true);
                gameObject.SetActive(false);
            });
            
            simulationSpeedText.text = "Simulation Speed: " + GameManager.Instance.SimulationSpeed;
            
            simulationSpeedSlider.onValueChanged.AddListener(delegate(float value)
            {
                GameManager.Instance.SimulationSpeed = value;
                simulationSpeedText.text = "Simulation Speed: " + GameManager.Instance.SimulationSpeed;
            });
            
            GameManager.Instance.OnTurnEnd += delegate(int i)
            {
                currentTurnText.text = "Current Turn: " + i;
                currentGreenAgentsText.text =
                    "Current Green Agents: " + GameManager.Instance.GameplayConfig.GreenAgentsList.Count;
                currentRedAgentsText.text =
                    "Current Red Agents: " + GameManager.Instance.GameplayConfig.RedAgentsList.Count;
            };
            
            GameManager.Instance.OnGenerationEnd += delegate(int i)
            {
                currentGenerationText.text = "Current Generation: " + i;
                currentGreenAgentsText.text =
                    "Current Green Agents: " + GameManager.Instance.GameplayConfig.GreenAgentsList.Count;
                currentRedAgentsText.text =
                    "Current Red Agents: " + GameManager.Instance.GameplayConfig.RedAgentsList.Count;
            };
            
        }
    }
}
