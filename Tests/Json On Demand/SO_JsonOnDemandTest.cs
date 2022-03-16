using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame
{

    [CreateAssetMenu(fileName = nameof(SO_JsonOnDemandTest), menuName = QcUnity.SO_CREATE_MENU + Singleton_GameController.PROJECT_NAME + "/Test/Json")]
    public class SO_JsonOnDemandTest : ScriptableObject, IPEGI
    {
        [SerializeField] private List<TutorialStepPrototypeTypedJson> _tutorialStepPrototypeJsons = new();

        #region Test Class


        // **************** Wrappers
        [Serializable] private class TutorialStepPrototypeTypedJson : TypedInstance.JsonSerializable<TutorialStepAbstract> { }
        [Serializable] private class ConditionTypedJson : TypedInstance.JsonSerializable<ITutorialCondition> { }


        // *************** Tutorial Step
        private abstract class TutorialStepAbstract : IPEGI
        {
            [SerializeField] private string _text;
            [SerializeField] private TestEnum _enum;
            [SerializeField] private List<ConditionTypedJson> _conditionJson = new();

            protected enum TestEnum { EnumValueA, EnumValueB, EnumValueC, EnumValueD}

            #region Inspector
            protected readonly pegi.CollectionInspectorMeta subclassesMeta = new();
            public virtual void Inspect()
            {
                if (subclassesMeta.IsAnyEntered == false)
                {
                    "Test".PegiLabel(50).Edit(ref _text).Nl();
                    "Enum Value".PegiLabel(80).Edit_Enum(ref _enum).Nl();
                }

                subclassesMeta.Edit_List(_conditionJson).Nl();
            }
            #endregion
        }

        private class TutorialStepA : TutorialStepAbstract 
        {
            [SerializeField] private string _textA;

            public override void Inspect()
            {
                base.Inspect();
                if (!subclassesMeta.IsAnyEntered)
                    "Test A".PegiLabel(50).Edit(ref _textA).Nl();
            }
        }

        private class TutorialStepB : TutorialStepAbstract 
        {
            [SerializeField] private string _textB;
            public override void Inspect()
            {
                base.Inspect();
                if (!subclassesMeta.IsAnyEntered)
                    "Test B".PegiLabel(50).Edit(ref _textB).Nl();
            }
        }

        private class TutorialStepC : TutorialStepB
        {
            [SerializeField] private string _textC;

            public override void Inspect()
            {
                base.Inspect(); 
                if (!subclassesMeta.IsAnyEntered)
                    "Test C".PegiLabel(50).Edit(ref _textC).Nl();
            }
        }


        // ************* Tutorial Conditions
        private interface ITutorialCondition 
        {
            bool CheckCondition(); 
        }

        private class ConditionA : ITutorialCondition, IPEGI_ListInspect
        {
            [SerializeField] private bool _isTrue;
            public bool CheckCondition() => _isTrue;

            public void InspectInList(ref int edited, int index)
            {
                "Is True".PegiLabel().ToggleIcon(ref _isTrue);
            }
        }

        private class ConditionB : ITutorialCondition
        {
            public bool CheckCondition() => Application.isPlaying;
        }

        private class ConditionC : ITutorialCondition 
        {
            
            public bool CheckCondition() => Application.isEditor;
        }

        #endregion

        #region Inspector
        private readonly pegi.CollectionInspectorMeta _listMeta = new("Derrived from {0}".F(nameof(TutorialStepAbstract)));

        public void Inspect()
        {
            _listMeta.Edit_List(_tutorialStepPrototypeJsons).Nl();

            if (_listMeta.IsAnyEntered == false)
            {
                if ("Serialize to Clipboard".PegiLabel().Click().Nl())
                {
                    var str = JsonUtility.ToJson(this);
                    Debug.Log(str);
                    pegi.SetCopyPasteBuffer(str);
                }

                if (pegi.CopyPasteBuffer.IsNullOrEmpty() == false)
                {
                    Icon.Clear.Click().OnChanged(()=> pegi.CopyPasteBuffer = null);
                    if ("Deserialize from Clipboard".PegiLabel().Click().Nl())
                    {
                        JsonUtility.FromJsonOverwrite(pegi.CopyPasteBuffer, this);
                        pegi.CopyPasteBuffer = null;
                    }
                }
            }
        }
        #endregion
    }

    [PEGI_Inspector_Override(typeof(SO_JsonOnDemandTest))]
    internal class SO_JsonOnDemandTestDrawer : PEGI_Inspector_Override { }
}
