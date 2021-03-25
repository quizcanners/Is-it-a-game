using QuizCanners.Inspect;
using QuizCanners.Lerp;
using QuizCanners.Migration;
using QuizCanners.Utils;
using System.Collections.Generic;
using UnityEngine;
namespace QuizCanners.SpecialEffects
{
    public partial class RoundedGraphic
    {
        private List<RoundedButtonModuleBase> _modules = new List<RoundedButtonModuleBase>();

        [SerializeField] private CfgData _modulesStd;

        public CfgData ConfigStd
        {
            get { return _modulesStd; }
            set
            {
                _modulesStd = value;
                this.SetToDirty();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            this.Decode(ConfigStd);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (!Application.isPlaying)
                ConfigStd = Encode().CfgData;
        }

        public CfgEncoder Encode() =>
            new CfgEncoder()
            .Add_Abstract("mdls", _modules);

        public void DecodeTag(string key, CfgData data)
        {
            switch (key)
            {
                case "mdls": data.ToList(out _modules, RoundedButtonModuleBase.all); break;
            }
        }

        [TaggedTypes.Tag(CLASS_KEY, "Uniform offset for stretched graphic", false)]
        protected class RoundedButtonStretchedUniformOffset : RoundedButtonModuleBase, IPEGI, IPEGI_ListInspect
        {

            private const string CLASS_KEY = "StretchedOffset";

            public override string ClassTag => CLASS_KEY;

            private float size = 100;

            #region Encode & Decode

            public override void DecodeTag(string key, CfgData data)
            {

                switch (key)
                {
                    case "s": size = data.ToFloat(); break;
                }
            }

            public override CfgEncoder Encode() => new CfgEncoder()//this.EncodeUnrecognized()
                .Add("b", base.Encode())
                .Add("s", size);

            #endregion

            #region Inspect
            public void InspectInList(ref int edited, int ind)
            {
                // var tg = inspected;

                var rt = inspected.rectTransform;

                if (rt.anchorMin != Vector2.zero || rt.anchorMax != Vector2.one)
                {

                    if ("Stretch Anchors".PegiLabel().Click())
                    {
                        rt.anchorMin = Vector2.zero;
                        rt.anchorMax = Vector2.one;
                    }
                }
                else
                {

                    var offset = rt.offsetMin.x;

                    var rect = rt.rect;

                    if (icon.Refresh.Click("Refresh size ({0})".F(size)))
                        size = Mathf.Max(Mathf.Abs(rect.width), Mathf.Abs(rect.height));

                    if ("Offset".PegiLabel().edit(ref offset, -size, size))
                    {
                        rt.offsetMin = Vector2.one * offset;
                        rt.offsetMax = -Vector2.one * offset;
                    }
                }
            }
            #endregion

        }

        [TaggedTypes.Tag(CLASS_KEY, "Native Size from Tiled Texture", false)]
        protected class RoundedButtonNativeSizeForOverlayOffset : RoundedButtonModuleBase, IPEGI, IPEGI_ListInspect
        {

            private const string CLASS_KEY = "TiledNatSize";

            private ShaderProperty.TextureValue referenceTexture = new ShaderProperty.TextureValue("_MainTex");

            public override string ClassTag => CLASS_KEY;

            #region Inspect
            public void InspectInList(ref int edited, int ind)
            {

                var mat = inspected.material;
                if (mat)
                {

                    pegi.select_or_edit_TextureProperty(ref referenceTexture, mat);

                    var tex = referenceTexture.Get(mat);

                    if (tex)
                    {
                        if (icon.Size.Click("Set Native Size for Texture, using it's Tile/Offset"))
                        {

                            var size = new Vector2(tex.width, tex.height);
                            var til = referenceTexture.GetTiling(mat);
                            size *= til;

                            inspected.rectTransform.sizeDelta = size;

                        }
                    }
                }
                else "No Material".PegiLabel().write();

            }

            #endregion

            #region Encode & Decode

            public override void DecodeTag(string key, CfgData data)
            {
                switch (key)
                {
                    case "b": data.ToDelegate(base.DecodeTag); break;
                    case "mp": referenceTexture.Decode(data); break;
                }
            }

            public override CfgEncoder Encode() => new CfgEncoder()//this.EncodeUnrecognized()
                    .Add("b", base.Encode())
                    .Add("mp", referenceTexture);

            #endregion
        }

        protected abstract class RoundedButtonModuleBase : IGotClassTag, ICfg, IGotReadOnlyName
        {
            public static TaggedTypes.DerrivedList all = TaggedTypes<RoundedButtonModuleBase>.DerrivedList;//new TaggedTypesCfg(typeof(RoundedButtonModuleBase));
            public TaggedTypes.DerrivedList AllTypes => all;//all;
            public abstract string ClassTag { get; }

            #region Inspect
            public virtual string GetReadOnlyName() => ClassTag;

            public virtual void Inspect()
            {
            }
            #endregion

            #region Encode & Decode
            public virtual CfgEncoder Encode() => new CfgEncoder();//this.EncodeUnrecognized();

            public virtual void DecodeTag(string key, CfgData data)
            {

            }
            #endregion
        }



    }
}