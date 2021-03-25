using System;
using System.Collections.Generic;
using QuizCanners.Inspect;
using QuizCanners.Migration;
using QuizCanners.Utils;
using Random = UnityEngine.Random;

namespace QuizCanners.IsItGame.Triggers.Dialogue
{


    #pragma warning disable IDE0018 // Inline variable declaration

    public enum Languages { note = 0, en = 1, uk = 2, tr = 3, ru = 4 }
    
    public abstract class Sentence: IGotClassTag, ICfg, IGotName, IPEGI {

        #region Tagged Types MGMT
        public abstract string ClassTag { get; }
        public static TaggedTypes.DerrivedList DerrivedList => TaggedTypes<Sentence>.DerrivedList;//new TaggedTypesCfg(typeof(Sentence));
        #endregion

        public override string ToString() => NameForInspector;

        public abstract bool GotNextText { get; } // => options.Count > index;

        public abstract string NameForInspector { get; set; }

        public abstract string GetNext();// => NameForPEGI; // Will update all the options inside;

        public virtual void Reset() { }

        public virtual void Inspect() { }
        public virtual CfgEncoder Encode() => new CfgEncoder();


        public virtual void DecodeTag(string tg, CfgData data)
        {
        }
    }

    [TaggedTypes.Tag(CLASS_KEY, "String")]
    public class StringSentence : Sentence {
        private const string CLASS_KEY = "s";

        protected string text = "";

        protected bool sentOne;

        public override void Reset() => sentOne = false;

        public override bool GotNextText => !sentOne && !text.IsNullOrEmpty();

        public override string ClassTag => CLASS_KEY;

        public override string NameForInspector
        {
            get { return text; }
            set { text = value; }
        }

        public override string GetNext()
        {
            sentOne = true;
            return NameForInspector;
        }

        #region Encode & Decode
        public override CfgEncoder Encode() => base.Encode()//this.EncodeUnrecognized()
            .Add_String("t", text);

        public override void DecodeTag(string tg, CfgData data)
        {
            switch (tg)
            {
                case "t": text = data.ToString(); break;
            }
        }
        #endregion

        #region Inspector
        
       public override void Inspect()
        {
           pegi.edit(ref text).nl();
        }
        
        #endregion

        public StringSentence()
        {

        }

        public StringSentence(string newText)
        {
            text = newText;
        }

    }


    [TaggedTypes.Tag(CLASS_KEY, "Multi Language")]
    public class MultilanguageSentence : Sentence, IPEGI_ListInspect, INeedAttention {
        private const string CLASS_KEY = "ml";

        public override string ClassTag => CLASS_KEY;

        public override string NameForInspector { get { return this[currentLanguage]; } set { this[currentLanguage] = value; } }

        protected bool sentOne;

        public override void Reset() => sentOne = false;

        public override bool GotNextText => !sentOne;

        public override string GetNext()
        {
            sentOne = true;
            return NameForInspector;
        }

        #region Languages MGMT
        public static Languages currentLanguage = Languages.en;

        private static List<string> _languageCodes;

        public static List<string> LanguageCodes
        {
            get
            {
                if (_languageCodes != null) return _languageCodes;

                _languageCodes = new List<string>();
                var names = Enum.GetNames(typeof(Languages));
                var values = (int[])Enum.GetValues(typeof(Languages));
                for (var i = 0; i < values.Length; i++)
                    _languageCodes.ForceSet(values[i], names[i]);

                return _languageCodes;
            }
        }
        
        public string this[Languages lang]
        {
            get
            {
                string text;
                var ind = (int)lang;

                if (texts.TryGetValue(ind, out text))
                    return text;
                if (lang == Languages.en)
                {
                    text = "English Text";
                    texts[ind] = text;
                }
                else
                    text = this[Languages.en];

                return text;
            }
            set { texts[(int)lang] = value; }
        }

        public bool Contains(Languages lang) => texts.ContainsKey((int)lang);

        public bool Contains() => Contains(currentLanguage);
        
        #endregion
        
        // Change this to also use Sentence base
        public Dictionary<int, string> texts = new Dictionary<int, string>();

        private bool needsReview;

        public static bool singleView = true;

        #region Encode & Decode
        public override CfgEncoder Encode() => new CfgEncoder()//this.EncodeUnrecognized()
            .Add("txts", texts)
            .Add_IfTrue("na", needsReview);

        public override void DecodeTag(string tg, CfgData data){
            switch (tg) {
                case "t": NameForInspector = data.ToString(); break;
                case "txts": data.ToDictionary(out texts); break;
                case "na": needsReview = data.ToBool(); break;
            }
        }
        #endregion

        #region Inspector

        public static void LanguageSelector_PEGI() => pegi.editEnum(ref currentLanguage, 30);
        
        public string NeedAttention() {
            if (needsReview)
                return "Marked for review";
            return null;
        }
        
        public virtual void InspectInList(ref int edited, int ind) {
            this.inspect_Name();

            if (this.Click_Enter_Attention(hint: currentLanguage.GetNameForInspector()))
                edited = ind;
        }

        public override void Inspect() {
            string tmp = NameForInspector;


            "Show only one language".PegiLabel().toggleIcon(ref singleView, true);
            if (singleView)  {
                LanguageSelector_PEGI();
                if (pegi.editBig(ref tmp)) {
                    NameForInspector = tmp;
                    return;
                }
            } else
            {

                pegi.nl();

                "Translations".PegiLabel().edit_Dictionary(texts, LanguageCodes);

                LanguageSelector_PEGI();
                if (!Contains() && icon.Add.Click("Add {0}".F(currentLanguage.GetNameForInspector())))
                    NameForInspector = this[currentLanguage];

                pegi.nl();
            }

            "Mark for review".PegiLabel("NEEDS REVIEW").toggleIcon(ref needsReview);
        }

        #endregion
    }

    [TaggedTypes.Tag(CLASS_KEY, "Random Sentence")]
    public class RandomSentence : ListOfSentences {
        private const string CLASS_KEY = "rnd";

        public override string ClassTag => CLASS_KEY;

        public bool pickedOne;

        public override bool GotNextText => !pickedOne || Current.GotNextText;

        public override void Reset() {
            base.Reset();
            pickedOne = false;
        }

        public override string GetNext() {
           
            if (!pickedOne)
                index = Random.Range(0, options.Count);

            pickedOne = true;

            return Current.GetNext();
        }

        #region Inspector

        public override void Inspect()
        {
            pegi.edit_List(options).nl();
        }

        public override void InspectInList(ref int edited, int ind)
        {
            "RND:".PegiLabel(25).write();
            base.InspectInList(ref edited, ind);
        }

        #endregion

    }
    
    [TaggedTypes.Tag(CLASS_KEY, "List")]
    public class ListOfSentences : Sentence, IPEGI_ListInspect, IGotCount
    {
        private const string CLASS_KEY = "lst";

        public override string ClassTag => CLASS_KEY;

        protected List<Sentence> options = new List<Sentence>();

        protected Sentence Current => options.TryGet(index);

        protected int index;

        public override void Reset() {
            foreach (var o in options)
                o.Reset();

            index = 0;
        }

        public override bool GotNextText => options.Count-1 > index || (index<options.Count && Current.GotNextText);

        public override string NameForInspector
        {
            get { return Current.NameForInspector; }
            set { Current.NameForInspector = value; }
        }

        public override string GetNext() {

            Current.GetNext();

            if (!Current.GotNextText)
                index = (index+1) % options.Count;

            return  Current.GetNext();
        }

        #region Encode & Decode

        public override CfgEncoder Encode() => new CfgEncoder()//this.EncodeUnrecognized()
            .Add("txs", options, DerrivedList)
            .Add("ins", inspectedSentence);

        public override void DecodeTag(string tg, CfgData data)
        {
            switch (tg)
            {
                case "txs": data.ToList(out options, DerrivedList); break;
                case "t": options.Add(new StringSentence(data.ToString())); break;
                case "ins": inspectedSentence = data.ToInt(); break;
            }
        }
        #endregion

        #region Inspector

        private int inspectedSentence = -1;

        public override void Inspect()
        {
            pegi.nl();

            "Sentences".PegiLabel().edit_List(options, ref inspectedSentence).nl();
        }

        public virtual void InspectInList(ref int edited, int ind) {

            if (options.Count>0)
                options[0].inspect_Name();
            else if ("Add Sentence".PegiLabel().Click())
                options.Add(new StringSentence(" "));

            if (icon.Enter.Click())
                edited = ind;
        }

        public int GetCount() => options.Count;

        #endregion

        public ListOfSentences() {
            options.Add(new StringSentence());
        }

    }

    [TaggedTypes.Tag(CLASS_KEY)]
    public class ConditionalSentence : MultilanguageSentence, IAmConditional
    {
        private const string CLASS_KEY = "cndSnt";

        public override string ClassTag => CLASS_KEY;

        private readonly ConditionBranch _condition = new ConditionBranch();

        public bool CheckConditions() => _condition.CheckConditions();

        #region Inspector
        
        public override void InspectInList(ref int edited, int ind)
        {
            this.inspect_Name();
            if (this.Click_Enter_Attention(_condition.IsTrue() ? icon.Active : icon.InActive,
                currentLanguage.GetNameForInspector()))
                edited = ind;
        }

        public override void Inspect()
        {
            _condition.Nested_Inspect().nl();
            base.Inspect();
        }

        #endregion

        #region Encode & Decode

        public override CfgEncoder Encode() => new CfgEncoder()
            .Add("b", base.Encode)
            .Add_IfNotDefault("cnd", _condition);

        public override void DecodeTag(string tg, CfgData data)
        {
            switch (tg)
            {
                case "b":data.ToDelegate(base.DecodeTag);break;
                case "cnd":_condition.Decode(data);break;
            }
        }

        #endregion

    }

}


