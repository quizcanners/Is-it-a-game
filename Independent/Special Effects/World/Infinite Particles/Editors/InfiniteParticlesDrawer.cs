using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.SpecialEffects
{
    public class InfiniteParticlesDrawerGUI : PEGI_Inspector_Material
    {
        public const string FadeOutTag = "_FADEOUT";

        public override bool Inspect(Material mat)
        {

            var changed = pegi.ChangeTrackStart();

            mat.toggle("SCREENSPACE").nl();
            mat.toggle("DYNAMIC_SPEED").nl();
            mat.toggle(FadeOutTag).nl();

            var fo = mat.HasTag(FadeOutTag);

            if (fo)
                "When alpha is one, the graphic will be invisible.".PegiLabel().writeHint();

            pegi.nl();

            var dynamicSpeed = mat.GetKeyword("DYNAMIC_SPEED");

            pegi.nl();

            if (!dynamicSpeed)
                mat.edit(speed, "speed", 0, 60).nl();
            else
            {
                mat.edit(time, "Time").nl();
                "It is expected that time Float will be set via script. Parameter name is _CustomTime. ".PegiLabel().writeHint();
                pegi.nl();
            }

            mat.edit(tiling, "Tiling", 0.1f, 20f).nl();

            mat.edit(upscale, "Scale", 0.1f, 1).nl();

            mat.editTexture("_MainTex").nl();
            mat.editTexture("_MainTex2").nl();
            mat.edit(color, "Color fo the Particles").nl();


            return changed;
        }

        private static readonly ShaderProperty.ColorFloat4Value color = new ShaderProperty.ColorFloat4Value("_Color");
        private static readonly ShaderProperty.FloatValue speed = new ShaderProperty.FloatValue("_Speed");
        private static readonly ShaderProperty.FloatValue time = new ShaderProperty.FloatValue("_CustomTime");
        private static readonly ShaderProperty.FloatValue tiling = new ShaderProperty.FloatValue("_Tiling");
        private static readonly ShaderProperty.FloatValue upscale = new ShaderProperty.FloatValue("_Upscale");

    }
}
