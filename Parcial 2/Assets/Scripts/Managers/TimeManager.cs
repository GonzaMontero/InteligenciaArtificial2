using System;

namespace IA.Managers
{
    public class TimeManager
    {
        public Action onActionTimeReached;
        public float TimeBetweenTicks { get; set; } = 1.0f;

        private float _currentTime = 0.0f;

        private int _currentCycleAmount = 0;
        private int _maxCycleAmount = 0;

        public void Reset() => _currentTime = 0.0f;
        
        public void SetCyclesAmount(int amount) => _maxCycleAmount = amount;
        
        public void Update(float deltaTime)
        {
            _currentTime += deltaTime;
            if (_currentTime > TimeBetweenTicks && !CycleEnded())
            {
                _currentTime = 0;
                _currentCycleAmount++;
                onActionTimeReached?.Invoke();
            }
        }

        private bool CycleEnded()
        {
            if (_currentCycleAmount == _maxCycleAmount)
                _currentCycleAmount = 0;
            return _currentCycleAmount == _maxCycleAmount;
        }
    }
}
