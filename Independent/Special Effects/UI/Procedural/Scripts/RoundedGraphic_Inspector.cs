using static QuizCanners.Inspect.pegi;
using QuizCanners.Utils;
using System.Collections.Generic;
using UnityEngine;
using QuizCanners.Inspect;

namespace QuizCanners.SpecialEffects
{
    public partial class RoundedGraphic : IPEGI
    {

        private static List<Shader> _compatibleShaders;

        private static List<Shader> CompatibleShaders
        {
            get
            {
                if (_compatibleShaders == null)
                {
                    _compatibleShaders = new List<Shader>()
                        .TryAdd(Shader.Find("Quiz cAnners/UI/Rounded/Lit Button"))
                        .TryAdd(Shader.Find("Quiz cAnners/UI/Rounded/Box"))
                        .TryAdd(Shader.Find("Quiz cAnners/UI/Rounded/Unlinked/Box Unlinked"))
                        .TryAdd(Shader.Find("Quiz cAnners/UI/Rounded/Pixel Perfect"))
                        .TryAdd(Shader.Find("Quiz cAnners/UI/Rounded/Outline"))
                        .TryAdd(Shader.Find("Quiz cAnners/UI/Rounded/Unlinked/Outline Unlinked"))
                        .TryAdd(Shader.Find("Quiz cAnners/UI/Rounded/Button With Shadow"))
                        .TryAdd(Shader.Find("Quiz cAnners/UI/Rounded/Shadow"))
                        .TryAdd(Shader.Find("Quiz cAnners/UI/Rounded/Glow"))
                        .TryAdd(Shader.Find("Quiz cAnners/UI/Rounded/Gradient"))
                        .TryAdd(Shader.Find("Quiz cAnners/UI/Rounded/Unlinked/Gradient Unlinked"))
                        .TryAdd(Shader.Find("Quiz cAnners/UI/Rounded/Preserve Aspect"))
                        .TryAdd(Shader.Find("Quiz cAnners/UI/Rounded/SubtractiveGraphic"))
                        .TryAdd(Shader.Find("Quiz cAnners/UI/Rounded/Image"))
                        .TryAdd(Shader.Find("Quiz cAnners/UI/Primitives/Pixel Line"))
                        .TryAdd(Shader.Find("Quiz cAnners/UI/Rounded/Pixel Perfect Screen Space"));
                }

                return _compatibleShaders;
            }
        }

        private static List<Material> _compatibleMaterials = new List<Material>();

        [SerializeField] private int _inspectedModule;
        public static RoundedGraphic inspected;

        private const string info =
            "Rounded Graphic component provides additional data to pixel perfect UI shaders. Those shaders will often not display correctly in the scene view. " +
            "Also they may be tricky at times so take note of all the warnings and hints that my show in this inspector. " +
            "When Canvas is set To ScreenSpace-Camera it will also provide adjustive softening when scaled";

        internal static void ClickDuplicate(ref Material mat, string newName = null, string folder = "Materials") =>
            ClickDuplicate(ref mat, folder, ".mat", newName);

        internal static void ClickDuplicate<T>(ref T obj, string folder, string extension, string newName = null) where T : Object
        {

            if (!obj) 
                return;

#if UNITY_EDITOR
            var path = UnityEditor.AssetDatabase.GetAssetPath(obj);
            if (icon.Copy.ClickConfirm("dpl" + obj + "|" + path, "{0} Duplicate at {1}".F(obj, path)))
            {
                obj = QcUnity.Duplicate(obj, folder, extension: extension, newName: newName);
            }
#else
             if (icon.Copy.Click("Create Instance of {0}".F(obj)))
                obj = GameObject.Instantiate(obj);

#endif
        }

        private pegi.CollectionInspectorMeta _modulesInspectorMeta = new pegi.CollectionInspectorMeta("Modules");

        [SerializeField] private EnterExitContext _enteredContent = new EnterExitContext();

        public void Inspect()
        {
            using (_enteredContent.StartContext())
            {
                inspected = this;

                FullWindow.DocumentationClickOpen(info, "About Rounded Graphic").nl();

                var mat = material;

                var can = canvas;

                var shad = mat.shader;

                var changed = ChangeTrackStart();

                bool expectedScreenPosition = false;

                bool expectedAtlasedPosition = false;

                if (!_enteredContent.IsAnyEntered)
                {

                    bool gotPixPerfTag = false;

                    bool mayBeDefaultMaterial = true;

                    bool expectingPosition = false;

                    bool possiblePositionData = false;

                    bool possibleFadePosition = false;

                    bool needThirdUv;

                    #region Material Tags 
                    if (mat)
                    {
                        var pixPfTag = mat.Get(ShaderTags.PixelPerfectUi);

                        gotPixPerfTag = !pixPfTag.IsNullOrEmpty();

                        if (!gotPixPerfTag)
                            "{0} doesn't have {1} tag".F(shad.name, ShaderTags.PixelPerfectUi.GetReadOnlyName()).PegiLabel().writeWarning();
                        else
                        {

                            mayBeDefaultMaterial = false;

                            expectedScreenPosition = pixPfTag.Equals(ShaderTags.PixelPerfectUis.Position.GetReadOnlyName());

                            if (!expectedScreenPosition)
                            {

                                expectedAtlasedPosition = pixPfTag.Equals(ShaderTags.PixelPerfectUis.AtlasedPosition.GetReadOnlyName());

                                if (!expectedAtlasedPosition)
                                    possibleFadePosition = pixPfTag.Equals(ShaderTags.PixelPerfectUis.FadePosition.GetReadOnlyName());
                            }

                            needThirdUv = expectedAtlasedPosition || (possibleFadePosition && feedPositionData);

                            expectingPosition = expectedAtlasedPosition || expectedScreenPosition;

                            possiblePositionData = expectingPosition || possibleFadePosition;

                            if (!can)
                                "No Canvas".PegiLabel().writeWarning();
                            else
                            {
                                if ((can.additionalShaderChannels & AdditionalCanvasShaderChannels.TexCoord1) == 0)
                                {

                                    "Material requires Canvas to pass Edges data trough Texture Coordinate 1 data channel".PegiLabel()
                                        .writeWarning();
                                    if ("Fix Canvas Texture Coordinate 1".PegiLabel().Click().nl())
                                        can.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;

                                }

                                if (possiblePositionData && feedPositionData)
                                {
                                    if ((can.additionalShaderChannels & AdditionalCanvasShaderChannels.TexCoord2) == 0)
                                    {
                                        "Material requires Canvas to pass Position Data trough Texcoord2 channel".PegiLabel()
                                            .writeWarning();
                                        if ("Fix Canvas ".PegiLabel().Click().nl())
                                            can.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord2;
                                    }
                                    else if (needThirdUv && (can.additionalShaderChannels & AdditionalCanvasShaderChannels.TexCoord3) == 0)
                                    {

                                        "Material requires Canvas to pass Texoord3 channel".PegiLabel().writeWarning();
                                        if ("Fix Canvas".PegiLabel().Click().nl())
                                            can.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord3;
                                    }

                                }

                                if (can.renderMode == RenderMode.WorldSpace)
                                {
                                    "Rounded UI isn't always working on world space UI yet.".PegiLabel().writeWarning();
                                    if ("Change to Overlay".PegiLabel().Click())
                                        can.renderMode = RenderMode.ScreenSpaceOverlay;
                                    if ("Change to Camera".PegiLabel().Click())
                                        can.renderMode = RenderMode.ScreenSpaceCamera;
                                    pegi.nl();
                                }

                            }
                        }
                    }
                    #endregion

                    var linked = LinkedCorners;

                    if (mat && (linked == mat.IsKeywordEnabled(UNLINKED_VERTICES)))
                        mat.SetShaderKeyword(UNLINKED_VERTICES, !linked);

                    if (toggle(ref linked, icon.Link, icon.UnLinked))
                        LinkedCorners = linked;

                    for (var i = 0; i < _roundedCournersPixels.Length; i++)
                    {
                        var crn = _roundedCournersPixels[i];

                        if ("{0}".F(linked ? "Courners" : ((Corner)i).ToString()).PegiLabel(70).edit(ref crn, 0, MaxCourner).nl())
                            _roundedCournersPixels[i] = crn;
                    }

                    nl();

                    if (mat)
                    {
                        var needLink = ShaderTags.PerEdgeData.Get(mat);
                        if (!needLink.IsNullOrEmpty())
                        {
                            if (ShaderTags.PerEdgeRoles.LinkedCourners.Equals(needLink))
                            {
                                if (!linked)
                                {
                                    "Material expects edge data to be linked".PegiLabel().writeWarning();
                                    if ("FIX".PegiLabel().Click())
                                        LinkedCorners = true;
                                }
                            }
                            else
                            {
                                if (linked)
                                {
                                    "Material expects edge data to be Unlinked".PegiLabel().writeWarning();
                                    if ("FIX".PegiLabel().Click())
                                        LinkedCorners = false;
                                }
                            }
                        }
                    }

                    nl();

                    QcUnity.RemoveEmpty(_compatibleMaterials);

                    if (mat && gotPixPerfTag)
                        _compatibleMaterials.AddIfNew(mat);

                    bool showingSelection = false;

                    var cmpCnt = _compatibleMaterials.Count;
                    if (cmpCnt > 0 && ((cmpCnt > 1) || (!_compatibleMaterials[0].Equals(mat))))
                    {

                        showingSelection = true;

                        if (select(ref mat, _compatibleMaterials, allowInsert: !mayBeDefaultMaterial))
                            material = mat;
                    }

                    if (mat)
                    {

                        if (!Application.isPlaying)
                        {
                            var path = QcUnity.GetAssetFolder(mat);
                            if (path.IsNullOrEmpty())
                            {
                                nl();
                                "Material is not saved as asset. Click COPY next to it to save as asset. Or Click 'Refresh' to find compatible materials in your assets ".PegiLabel().writeHint();
                                nl();
                            }
                            else
                                mayBeDefaultMaterial = false;
                        }

                        if (!showingSelection && !mayBeDefaultMaterial)
                        {
                            var n = mat.name;
                            if ("Rename Material".PegiLabel("Press Enter to finish renaming.", 120).editDelayed(ref n))
                                QcUnity.RenameAsset(mat, n);
                        }
                    }

                    ChangesToken changedMaterial = edit_Property(() => m_Material, this, fieldWidth: 60);

                    if (!Application.isPlaying)
                        changedMaterial |= Nested_Inspect(() => ClickDuplicate(ref mat, gameObject.name)).OnChanged(() => material = mat);

                    if (changedMaterial)
                        _compatibleMaterials.AddIfNew(material);

                    if (!Application.isPlaying && icon.Refresh.Click("Find All Compatible Materials in Assets"))
                        _compatibleMaterials = ShaderTags.PixelPerfectUi.GetTaggedMaterialsFromAssets();

                    nl();

                    if (mat && !mayBeDefaultMaterial)
                    {

                        if ("Shader".PegiLabel(60).select(ref shad, CompatibleShaders, false, true))
                            mat.shader = shad;

                        var sTip = mat.Get(QuizCanners.Utils.ShaderTags.ShaderTip);

                        if (!sTip.IsNullOrEmpty())
                            FullWindow.DocumentationClickOpen(sTip, "Tip from shader tag");

                        if (shad)
                            shad.ClickHighlight();

                        if (icon.Refresh.Click("Refresh compatible Shaders list"))
                            _compatibleShaders = null;
                    }

                    nl();

                    "Color".PegiLabel(90).edit_Property(() => color, this).nl();

                    #region Position Data

                    if (possiblePositionData || feedPositionData)
                    {

                        "Position Data".PegiLabel().toggleIcon(ref feedPositionData, true);

                        if (feedPositionData)
                        {
                            "Position: ".PegiLabel(60).editEnum(ref _positionDataType);

                            FullWindow.DocumentationClickOpen("Shaders that use position data often don't look right in the scene view.", "Camera dependancy warning");

                            nl();
                        }
                        else if (expectingPosition)
                            "Shader expects Position data".PegiLabel().writeWarning();

                        if (gotPixPerfTag)
                        {

                            if (feedPositionData)
                            {

                                switch (_positionDataType)
                                {
                                    case PositionDataType.ScreenPosition:

                                        if (expectedAtlasedPosition)
                                            "Shader is expecting Atlased Position".PegiLabel().writeWarning();

                                        break;
                                    case PositionDataType.AtlasPosition:
                                        if (expectedScreenPosition)
                                            "Shader is expecting Screen Position".PegiLabel().writeWarning();
                                        else if (sprite && sprite.packed)
                                        {
                                            if (sprite.packingMode == SpritePackingMode.Tight)
                                                "Tight Packing is not supported by rounded UI".PegiLabel().writeWarning();
                                            else if (sprite.packingRotation != SpritePackingRotation.None)
                                                "Packing rotation is not supported by Rounded UI".PegiLabel().writeWarning();
                                        }

                                        break;
                                    case PositionDataType.FadeOutPosition:

                                        "Fade out at".PegiLabel().edit(ref faeOutUvPosition).nl();

                                        break;
                                }
                            }
                        }

                        nl();
                    }

                    if (gotPixPerfTag && feedPositionData && !possiblePositionData)
                        "Shader doesn't have any PixelPerfectUI Position Tags. Position updates may not be needed".PegiLabel().writeWarning();

                    nl();

                    #endregion

                    var spriteTag = mat ? mat.Get(ShaderTags.SpriteRole) : null;

                    var noTag = spriteTag.IsNullOrEmpty();

                    if (noTag || !spriteTag.SameAs(ShaderTags.SpriteRoles.Hide.GetReadOnlyName()))
                    {
                        if (noTag)
                            spriteTag = "Sprite";

                        spriteTag.PegiLabel(90).edit_Property(() => sprite, this).nl();

                        var sp = sprite;

                        if (sp)
                        {
                            var tex = sp.texture;

                            var rct = SpriteRect;

                            if (tex && (
                                !Mathf.Approximately(rct.width, rectTransform.rect.width)
                                || !Mathf.Approximately(rct.height, rectTransform.rect.height))
                                    && icon.Size.Click("Set Native Size").nl())
                            {
                                rectTransform.sizeDelta = SpriteRect.size;
                                this.SetToDirty();
                            }
                        }
                        nl();
                    }

                    "Maskable".PegiLabel(90).edit_Property(() => maskable, this, includeChildren: true).nl();

                    "Raycast Target".PegiLabel(90).edit_Property(() => raycastTarget, this).nl();
                }

                if (_modulesInspectorMeta.enter_List(_modules).nl())
                {
                    ConfigStd = Encode().CfgData;
                    this.SetToDirty();
                }

                if (changed)
                {
                    SetVerticesDirty();
                }
            }
        }
    }


    [PEGI_Inspector_Override(typeof(RoundedGraphic))] internal class PixelPerfectShaderDrawer : PEGI_Inspector_Override { }
}
