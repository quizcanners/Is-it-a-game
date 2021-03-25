using QuizCanners.Lerp;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace QuizCanners.SpecialEffects
{
    [Serializable]
    public class AnimatedValue
    {
        [SerializeField] private bool _initialSet;
        [SerializeField] private double _currentValue;
        [SerializeField] private double _targetValue;
        [SerializeField] private double _valueChangeSpeed;
        [SerializeField] private readonly Gate.Frame _update = new Gate.Frame();
        [SerializeField] private readonly Gate.Frame _animation = new Gate.Frame();

        public bool AnimationIsAdditive { get; private set; }

        const int MAX_SECONDS_TO_ANIMATE = 2;
        const float MIN_TICK = 1;
        const float MIN_VALUE_CHANGE_SPEED = 10;

        public double GetWithoutUpdating() => _currentValue;

        public double UpdateAndGet()
        {
            CheckAnimation();
            return _currentValue;
        }

        private void OnValueChanged()
        {
            _update.DoneThisFrame = true;
            _animation.DoneThisFrame = true;
        }

        public void SetValue(double value)
        {
            _currentValue = value;
            OnValueChanged();
        }

        public double TargetValue
        {
            get => _targetValue;
            set
            {
                _targetValue = value;

                if (!_initialSet)
                {
                    _initialSet = true;
                    SetValue(_targetValue);
                    return;
                }

                OnValueChanged();

                _valueChangeSpeed = Math.Max(MIN_VALUE_CHANGE_SPEED, Math.Abs(_targetValue - _currentValue) / MAX_SECONDS_TO_ANIMATE);
            }
        }

        public bool IsAnimatingThisFrame
        {
            get
            {
                CheckAnimation();
                return _animation.DoneThisFrame;
            }
        }

        private void CheckAnimation()
        {

            if (!_initialSet || _update.DoneThisFrame)
                return;

            _update.DoneThisFrame = true;

            var diff = _targetValue - _currentValue;

            var absDiff = Math.Abs(diff);

            if (absDiff == 0)
            {
                _valueChangeSpeed = 0;
                return;
            }

            AnimationIsAdditive = diff > 0;

            _animation.DoneThisFrame = true;

            if (absDiff < MIN_TICK)
            {
                _currentValue = _targetValue;
                _valueChangeSpeed = 0;
            }
            else
            {
                _currentValue = LerpUtils.LerpBySpeed(_currentValue, _targetValue, _valueChangeSpeed);
            }
        }
    }
}