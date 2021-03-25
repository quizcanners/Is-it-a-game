using QuizCanners.Inspect;
using QuizCanners.IsItGame.Develop;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QuizCanners.IsItGame.StateMachine
{
    public class Manager : IPEGI, IGotVersion
    {
        public int Version { get; private set; }

        private readonly List<BaseState> _stateStack = new List<BaseState>();

        internal void SetDirty() => Version++;

        public List<V> Get<V>() 
        {
            List<V> vals = new List<V>();

            for (int i = 0; i < _stateStack.Count; i++)
            {
                if (_stateStack[i] is IStateDataAdditive<V> st)
                {
                    try
                    {
                        var result = st.Get();
                        vals.Add(result);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }

            if (TryGetFallbackData(out V newValue))
                vals.Add(newValue);

            return vals;
        }
        
        public bool TryChangeFallback<V>(ref V value, V fallbackValue)
        {
            if (!TryGetFallbackData(out V newValue))
            {
                newValue = fallbackValue;
            }

            if (value != null && value.Equals(newValue))
            {
                return false;
            }

            value = newValue;

            return true;
        }

        public bool IsCurrent(BaseState state) => state == _stateStack.TryGetLast();

        public void Enter<T>() where T : BaseState, new() => Enter(new T());

        public void Enter(BaseState state) 
        {
            SetDirty();

            var type = state.GetType();
            if (_stateStack.Any(st => st.GetType() == type))
            {
              //  Debug.LogError("Type {0} is already in the list. Returning.".F(type));
                ReturnToState(type);
                return;
            }

            _stateStack.Add(state);
            Current(cur => cur.OnIsCurrentChange());
            Previous(prev => prev.OnIsCurrentChange());
            Current(state => state.OnEnter());
        }

        public void Exit(Type type)
        {
            var index = _stateStack.FindLastIndex(state => state.GetType() == type);

            if (index == -1) 
            {
                Debug.LogWarning("State {0} not found".F(type));
                return;
            }

            Exit(_stateStack[index]);
        }

        public void Exit(BaseState closedState)
        {
            bool isLast = _stateStack.IndexOf(closedState) == _stateStack.Count - 1;

            if (!isLast) 
            {
                ReturnToState(closedState.GetType());
            }
            ExitLast();
        }

        public void ReturnToState(Type type)
        {
            while (_stateStack.Count > 0 && _stateStack.Last().GetType() != type)
                ExitLast();

            if (_stateStack.Count == 0)
                Debug.LogError("State {0} was never found".F(type.ToPegiStringType()));

            SetDirty();
        }

        private bool TryGetFallbackData<V>(out V result)
        {
            for (int i = _stateStack.Count - 1; i >= 0; i--)
            {
                if (_stateStack[i] is IStateDataFallback<V> st)
                {
                    try
                    {
                        result = st.Get();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }

            result = default(V);

            return false;
        }

        private void ExitLast()
        {
            var closedState = _stateStack.Last();
            DoInternal(state => state.OnExit(), closedState);
            _stateStack.Remove(closedState);
            DoInternal(state => state.OnIsCurrentChange(), closedState);
            Current(cur => cur.OnIsCurrentChange());

            SetDirty();
        }

        private void Current(Action<BaseState> action) 
        {
            var last = _stateStack.TryGetLast();
            if (last != null)
            {
                DoInternal(action, last);
            }
        }

        private void Previous(Action<BaseState> action)
        {
            var previous = _stateStack.TryGet(_stateStack.Count - 2);
            if (previous != null)
            {
                DoInternal(action, previous);
            }
        }

        private void DoInternal(Action<BaseState> action, BaseState state) 
        {
            try
            {
                action?.Invoke(state);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            SetDirty();
        }

        public void ManagedOnEnable()
        {
            if (Application.isPlaying)
            {
                Enter<BootstrapState>();
            }
        }
        public void ManagedOnDisable()
        {
            while (_stateStack.Count > 0)
            {
                Exit(_stateStack[_stateStack.Count - 1].GetType());
            }
        }

        public void ManagedUpdate()
        {
            ProcessFallbacks(state => state.Update());
            
            if (_stateStack.Count > 0) 
            {
                _stateStack.Last().UpdateIfCurrent();
            }
        }

        public void ManagedLateUpdate() => ProcessFallbacks(state => state.LateUpdate());

        private void ProcessFallbacks(Action<BaseState> stackOperator) 
        {
            for (int i = _stateStack.Count - 1; i >= 0; i--)
            {
                try
                {
                    stackOperator(_stateStack[i]);
                } catch (Exception ex) 
                {
                    Debug.LogException(ex);
                    return;
                }
            }
        }

        #region Inspector

        private bool _showTest;
        private int _inspectedState = -1;

        private IigEnum_GameState _debugState = IigEnum_GameState.Bootstrap;
        public void Inspect()
        {

            if ("Version ++ ({0})".F(Version).PegiLabel().Click().nl())
                SetDirty();

            if (_inspectedState >= _stateStack.Count)
                _inspectedState = -1;

            if (_inspectedState > -1)
            {
                if (icon.Back.Click() | _stateStack[_inspectedState].GetNameForInspector().PegiLabel().ClickLabel())
                    _inspectedState = -1;
                else
                    _stateStack[_inspectedState].Nested_Inspect();
            }
            
            if (_inspectedState == -1)
            {
                for (int i = 0; i < _stateStack.Count; i++)
                {
                    if (icon.Close.ClickConfirm("ExitState"+i, "Force Exit State"))
                        Exit(_stateStack[i]);
                    else
                        _stateStack[i].Inspect_AsInListNested(ref _inspectedState, i);
                }
            }
            
            pegi.nl();

            if (_inspectedState == -1 && "Debug & Test".PegiLabel().isFoldout(ref _showTest).nl())
            {
                "Debug State".PegiLabel().editEnum(ref _debugState);

                if (icon.Play.Click())
                    _debugState.Enter();
                if (icon.Exit.Click())
                    _debugState.Exit();

                pegi.nl();

                if (_stateStack.Count > 0 && "Close All".PegiLabel().Click())
                    ManagedOnDisable();

                if (_stateStack.Count == 0 && "Enter".PegiLabel().Click())
                    ManagedOnEnable();
            }
        }
        #endregion
    }

    public interface IStateDataAdditive<T> 
    {
        T Get();
    }

    public interface IStateDataFallback<T>
    {
        T Get();
    }
}