using System;
using System.Collections.Generic;
using QuizCanners.Inspect;
using QuizCanners.Migration;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.Triggers.Dialogue
{

    [CreateAssetMenu(fileName = FILE_NAME, menuName = "Quiz Canners/" + TriggerValuesService.TRIGGERS + "/" + FILE_NAME)]
    public class Dialogue : CfgSelfSerializationBaseScriptableObject, IPEGI, ISerializationCallbackReceiver, ICfg
    {
        public const string FILE_NAME = "Dialogue";

        [NonSerialized] internal InteractionBranch interactionBranch = new();

        protected int LogicVersion => Singleton.TryGetValue<TriggerValuesService, int>(s => s.Version, defaultValue: _questVersion);

        private string SingleText
        {
            set
            {
                OptText.Clear();
                //TODO: This sets a Single Text
            }
        }

        [NonSerialized] private readonly List<string> OptText = new();
        private readonly List<Interaction> PossibleInteractions = new();
        private readonly List<DialogueChoice> PossibleOptions = new();

        private bool CheckOptions(Interaction ia)
        {
            ClearTexts();
            var cnt = 0;
            foreach (var dio in ia.choices)
                if (dio.conditions.CheckConditions())
                {
                    OptText.Add(dio.text.NameForInspector);
                    PossibleOptions.Add(dio);
                    cnt++;
                }

            _questVersion = LogicVersion;

            return cnt > 0;
        }

        private void CollectInteractions(LogicBranch<Interaction> gr)
        {

            if (!gr.IsTrue()) return;

            foreach (var si in gr.elements)
            {

                si.ResetSentences();

                if (!si.IsTrue())
                    continue;

                OptText.Add(si.texts.NameForInspector);
                PossibleInteractions.Add(si);
            }

            foreach (var sgr in gr.subBranches)
                CollectInteractions(sgr);
        }

        private void BackToInteractionSelection()
        {
            ClearTexts();

            CollectInteractions(interactionBranch);

            if (PossibleInteractions.Count != 0)
            {
                _questVersion = LogicVersion;

                _interactionStage = 0;

                if (!continuationReference.IsNullOrEmpty())
                {
                    foreach (var ie in PossibleInteractions)
                        if (ie.ReferenceName.SameAs(continuationReference))
                        {
                            _interaction = ie;
                            _interactionStage++;
                            SelectOption(0);
                            break;
                        }
                }

                var lst = new List<string>();
                
                if (_interactionStage != 0) return;
                
                foreach (var interaction in PossibleInteractions)
                    lst.Add(interaction.texts.NameForInspector);

                //View.Options = lst;

            }
            //else
            //  Exit();
        }

        private static InteractionStage _interactionStage;
        private static Interaction _interaction;
        private static DialogueChoice _option;

        private static int _questVersion;

        private void DistantUpdate()
        {

            if (_questVersion == LogicVersion) return;

            switch (_interactionStage)
            {

                case InteractionStage.Idle: BackToInteractionSelection(); break;
                case InteractionStage.SelectInteraction: SingleText = _interaction.texts.NameForInspector; break;
                case InteractionStage.SelectReply: CheckOptions(_interaction); break;
                case InteractionStage.PostText: SingleText = _option.text2.NameForInspector; break;
            }

            _questVersion = LogicVersion;
        }

        private void ClearTexts()
        {
            OptText.Clear();
            PossibleInteractions.Clear();
            PossibleOptions.Clear();
        }

        private string continuationReference;
        private enum InteractionStage { Idle, SelectInteraction, OnSelected, SelectReply, FinalizeInteraction, PostText, Done }

        public void SelectOption(int no)
        {
            switch (_interactionStage)
            {
                case InteractionStage.Idle:
                    _interactionStage++; _interaction = PossibleInteractions.TryGet(no);
                    goto case InteractionStage.SelectInteraction;
                case InteractionStage.SelectInteraction:
                    continuationReference = null;

                    if (_interaction == null)
                        SingleText = "No Possible Interactions.";
                    else
                    {
                        if (_interaction.texts.GotNextText)
                        {
                            SingleText = _interaction.texts.GetNext();
                            break;
                        }

                        _interactionStage++;


                        goto case InteractionStage.OnSelected;
                    }

                    break;
                case InteractionStage.OnSelected:
                    _interactionStage++;
                    if (!CheckOptions(_interaction)) goto case InteractionStage.FinalizeInteraction; break;
                case InteractionStage.SelectReply:
                    _option = PossibleOptions[no];
                    _option.results.Apply();
                    _interaction.finalResults.Apply();
                    continuationReference = _option.nextOne;
                    goto case InteractionStage.PostText;

                case InteractionStage.FinalizeInteraction:
                    _interaction.finalResults.Apply(); BackToInteractionSelection(); break;
                case InteractionStage.PostText:
                    if (_option.text2.GotNextText)
                    {
                        SingleText = _option.text2.GetNext();
                        _interactionStage = InteractionStage.PostText;
                    }
                    else
                        goto case InteractionStage.Done;

                    break;

                case InteractionStage.Done:
                    BackToInteractionSelection();
                    break;
            }
        }

        #region Encode & Decode

        public override CfgEncoder Encode() => new CfgEncoder()
            .Add("inBr", interactionBranch);
        
        public override void DecodeTag(string tg, CfgData data) {
            switch (tg) {
                case "inBr": data.Decode(out interactionBranch);  break;
            }
        }
        #endregion

        #region Inspector

        public static Dialogue inspected;
        private readonly pegi.EnterExitContext _context = new(); // _inspectdStuff = -1; 

        public void Inspect()
        {
            inspected = this;
            using (_context.StartContext())
            {
                pegi.nl();

                if ("Play In Inspector".PegiLabel().isEntered().nl())
                {
                    "Playing {0} Dialogue".F(name).PegiLabel().write();

                    if (icon.Refresh.Click("Restart dialogue", 20))
                        BackToInteractionSelection();
                    else
                    {
                        DistantUpdate();
                        pegi.nl();
                        for (var i = 0; i < OptText.Count; i++)
                            if (OptText[i].PegiLabel().ClickText(13).nl())
                            {
                                SelectOption(i);
                                DistantUpdate();
                            }
                    }
                }

                if ("Interactions tree [{0}]".F(interactionBranch.GetCount()).PegiLabel().isEntered().nl_ifNotEntered())
                    interactionBranch.Nested_Inspect().nl();

                "Interaction stage: {0}".F(_interactionStage).PegiLabel().nl();

                if (_context.IsAnyEntered == false)
                    "Reload".PegiLabel().Click(() =>
                    {
                        OnBeforeSerialize();
                        interactionBranch = new InteractionBranch();
                        OnAfterDeserialize();
                    });
            }

        }
        #endregion

    }

    [PEGI_Inspector_Override(typeof(Dialogue))] internal class DialogueDrawer : PEGI_Inspector_Override { }
}
