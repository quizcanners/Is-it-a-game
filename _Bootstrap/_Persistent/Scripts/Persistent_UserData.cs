using QuizCanners.Inspect;
using System;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    [Serializable]
    public class Persistent_UserData : IsItGameClassBase, IPEGI
    {
        public InputFieldState InputFieldData = new InputFieldState();


        private pegi.EnterExitContext _context = new pegi.EnterExitContext();
        public void Inspect() 
        {
            using (_context.StartContext())
            {
                InputFieldData.Enter_Inspect().Nl();
            }
        }

        public class InputFieldState : IPEGI
        {
            public string Header = "Title";
            public string InputText;
            public bool Approved;
            public Action OnCloseAction;
            public Action<string> OnApprove;
            private Func<string, bool> Validator;

            public bool IsValid() => Validator == null || Validator(InputText);

            public void Approve()
            {
                try
                {
                    OnApprove?.Invoke(InputText);
                } catch (Exception ex) 
                {
                    Debug.LogException(ex);
                }

                Close();
            }
            public void Close() 
            {
                OnCloseAction?.Invoke();
            }
            public void Set(string header, Action<string> onValidate, Action onClose = null, Func<string, bool> validator = null) 
            {
                Header = header;
                InputText = "";
                Approved = false;
                OnCloseAction = onClose;
                OnApprove = onValidate;
                Validator = validator;
            }

            #region Inspector
            public void Inspect()
            {
                if (OnCloseAction != null)
                    pegi.Click(Close).Nl();

                Header.PegiLabel().Edit(ref InputText);

                if (IsValid())
                    Icon.Done.Click(Approve);
            }
            #endregion
        }
    }
}