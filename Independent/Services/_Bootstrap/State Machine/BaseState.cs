using QuizCanners.Inspect;
using QuizCanners.Utils;

namespace QuizCanners.IsItGame.StateMachine
{
    public abstract class BaseState : IsItGameClassBase, IPEGI, IPEGI_ListInspect
    {
        public bool IsCurrent => GameState.IsCurrent(this);
        protected void SetNextState<T>() where T : BaseState, new()
        {
            var myType = GetType();
            GameState.ReturnToState(myType);
            if (typeof(T) != myType) 
            {
                GameState.Enter<T>();
            }
        }

        internal virtual void OnIsCurrentChange() {}
        internal virtual void OnEnter() { }
        internal virtual void OnExit() {  }
        internal virtual void Update() {}
        internal virtual void LateUpdate() {}
        internal virtual void UpdateIfCurrent() {}
        internal void Exit() => GameState.Exit(this);

        #region Inspector
        public void Inspect()
        {
            GetType().ToPegiStringType().PegiLabel().nl();
        }

        public void InspectInList(ref int edited, int ind)
        {
            if (icon.Enter.Click() | (GetType().ToPegiStringType()).PegiLabel().ClickLabel())
                edited = ind;

            if (icon.Copy.Click())
                pegi.CopyPasteBuffer = GetType().ToPegiStringType();
        }
        #endregion
    }
}