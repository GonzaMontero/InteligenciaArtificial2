using System.Collections.Generic;
using UnityEngine;
using Configurations;
using Handlers;
using UnityEngine.Windows;
using System.Linq;

namespace Entities.Agents
{
    public class Agent : AgentBase
    {
        public enum Team
        {
            One,
            Two
        }

        [Header("Animation")]
        [SerializeField] private float movementSpeed = 1;
        [SerializeField] private Renderer visualRenderer;
        [SerializeField] private float timeForDeath;

        [Header("Fitness")]
        [SerializeField] private AnimationCurve fitnessCurve;

        public Vector2Int CurrentPosition { get; set; }

        public Genome agentGenome {  get; private set; }
        public NeuralNetwork agentBrain { get; private set; }
        public int GenerationsSurvived { get; set; }
        public int FoodEaten { get; private set; }
        public float Fitness { get; private set; }

        public bool ActionPositive() => _actionInput > .5f;

        private float[] inputs;

        private SimulationConfiguration simulationConfiguration;
        private Movement.MoveDirection previousPositionDirection;
        private List<float> _moveInput = new List<float>();
        private float _actionInput;
        private Team team;

        public void SetPosition(Vector2Int newPosition)
        {
            CurrentPosition = newPosition;
        }

        public void SetTeam(Team team)
        {
            this.team = team;
        }

        private void Start()
        {
            
        }

        private void OnDestroy()
        {
            
        }

        public override void StartMoving()
        {
            Think();
            Move(_moveInput);
        }

        public override void StartActing()
        {
            Act();
            OnAgentStopActing?.Invoke();
        }

        public void SetMind(Genome genome, NeuralNetwork brain)
        {
            agentGenome = genome;
            agentBrain = brain;
            inputs = new float[brain.InputsCount];
        }

        public override void Think()
        {
            inputs = SimulationManager.Instance.GetInputs(this);

            float[] output = agentBrain.Synapsis(inputs);

            _moveInput.Clear();
            _moveInput.Add(output[0]);
            _moveInput.Add(output[1]);
            _moveInput.Add(output[2]);
            _moveInput.Add(output[3]);
            _actionInput = output[4];
        }

        public override void Die()
        {
            visualRenderer.enabled = false;
            OnAgentDie?.Invoke();
            Destroy(gameObject, timeForDeath);
        }

        public override void Flee()
        {
            OnAgentFlee?.Invoke();
            ReturnToPreviousPosition();
        }

        private void Move(List<float> output)
        {
            Vector3 newPosition = transform.position;

            int index = output.IndexOf(output.Max());

            if (index == 0)
            {
                newPosition = _gameplayConfiguration.GetPostMovementPosition(this, Movement.MoveDirection.Down);
                _previousPositionDirection = Movement.MoveDirection.Up;
            }
            else if (index == 1)
            {
                newPosition = _gameplayConfiguration.GetPostMovementPosition(this, Movement.MoveDirection.Right);
                _previousPositionDirection = Movement.MoveDirection.Left;
            }
            else if (index == 2)
            {
                newPosition = _gameplayConfiguration.GetPostMovementPosition(this, Movement.MoveDirection.Left);
                _previousPositionDirection = Movement.MoveDirection.Right;
            }
            else if (index == 3)
            {
                newPosition = _gameplayConfiguration.GetPostMovementPosition(this, Movement.MoveDirection.Up);
                _previousPositionDirection = Movement.MoveDirection.Down;
            }

            transform.position = newPosition;
            OnAgentStopMoving?.Invoke();
        }

        private void Act()
        {
            _gameplayConfiguration.AgentAct(this, _team);
        }

        private void ReturnToPreviousPosition()
        {
            transform.position = _gameplayConfiguration.GetPostMovementPosition(this, _previousPositionDirection);

            switch (_previousPositionDirection)
            {
                case Movement.MoveDirection.Down:
                    _previousPositionDirection = Movement.MoveDirection.Up;
                    break;

                case Movement.MoveDirection.Left:
                    _previousPositionDirection = Movement.MoveDirection.Right;
                    break;

                case Movement.MoveDirection.Right:
                    _previousPositionDirection = Movement.MoveDirection.Left;
                    break;

                case Movement.MoveDirection.Up:
                    _previousPositionDirection = Movement.MoveDirection.Down;
                    break;
            }
        }

        public void Eat(int bonusFitness)
        {
            FoodEaten++;
            Fitness += bonusFitness * fitnessCurve.Evaluate(FoodEaten);
        }
    }
}