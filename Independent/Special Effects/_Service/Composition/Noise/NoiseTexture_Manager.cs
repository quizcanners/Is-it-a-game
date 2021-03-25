using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.SpecialEffects
{
    public partial class SpecialEffectShadersService
    {
        [System.Serializable]
        public class NoiseTextureManager : IPEGI, INeedAttention, IPEGI_ListInspect, IGotReadOnlyName
        {
            private readonly ShaderProperty.Feature _noiseTexture = new ShaderProperty.Feature("USE_NOISE_TEXTURE");
            private readonly ShaderProperty.TextureValue _noiseTextureGlobal = new ShaderProperty.TextureValue("_Global_Noise_Lookup");

            [SerializeField] private bool _enableNoise = true;
            [SerializeField] private Texture2D _prerenderedNoiseTexture;

            public bool EnableNoise
            {
                get => _enableNoise;
                set
                {
                    if (_enableNoise != value)
                    {
                        _enableNoise = value;
                        UpdateShaderGlobal();
                    }
                }
            }

            private void UpdateShaderGlobal()
            {
                _noiseTexture.Enabled = _enableNoise && _prerenderedNoiseTexture;
                _noiseTextureGlobal.SetGlobal(_prerenderedNoiseTexture);
            }

            public void ManagedOnEnable() => UpdateShaderGlobal();
            

            #region Inspector

            public string NeedAttention()
            {
                if (!_prerenderedNoiseTexture)
                    return "No Texture";

#if UNITY_EDITOR
                var importer = _prerenderedNoiseTexture.GetTextureImporter_Editor();

                if (importer.alphaIsTransparency)
                    return "Texture Alpha shouldn't be transparency";
                if (importer.filterMode != FilterMode.Point)
                    return "Filter Mode should be Point";
                if (importer.wrapMode != TextureWrapMode.Repeat)
                    return "Wrap Mode should be repeat";
                if (importer.textureCompression != UnityEditor.TextureImporterCompression.Uncompressed)
                    return "Texture should be uncompressed";
                if (importer.sRGBTexture)
                    return "Texture shouldn't be an RGB Texture";

#endif

                return null;
            }

            public void Inspect()
            {
                var changed = pegi.ChangeTrackStart();

                pegi.FullWindow.DocumentationClickOpen("This component will set noise texture as a global parameter. Using texture is faster then generating noise in shader.", "About Noise Texture Manager");

                pegi.nl();

                "Noise Tex".PegiLabel(120).edit(ref _prerenderedNoiseTexture);

                if (_prerenderedNoiseTexture)
                    icon.Refresh.Click("Update value in shader");

                pegi.nl();

                if (_prerenderedNoiseTexture)
                    _noiseTexture.ToString().PegiLabel().toggleIcon(ref _enableNoise, hideTextWhenTrue: true);

                if (_enableNoise)
                {
                    "Compile Directive and Global Texture:".PegiLabel().nl();

                    _noiseTexture.ToString().PegiLabel().write_ForCopy(showCopyButton: true).nl();
                    _noiseTextureGlobal.ToString().PegiLabel().write_ForCopy(showCopyButton: true);
                }
                pegi.nl();

                if (changed)
                    UpdateShaderGlobal();
            }

            public void InspectInList(ref int edited, int index)
            {
                if (pegi.toggleIcon(ref _enableNoise))
                    EnableNoise = _enableNoise;


                if (GetReadOnlyName().PegiLabel().ClickLabel() | this.Click_Enter_Attention())
                    edited = index;
            }

            public string GetReadOnlyName() => "Noise Texture";
            #endregion
        }
    }
}
