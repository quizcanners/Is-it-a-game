using PainterTool;
using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame
{

    [DisallowMultipleComponent]
    public class Pool_BloodParticlesController : PoolSingletonBase<C_BloodParticle>, INeedAttention
    {
        [SerializeField] private PlaytimePainter_BrushConfigScriptableObject _brushContainer;
        [SerializeField] private int _maxBlood = 150;

        protected override int MAX_INSTANCES => _maxBlood;

        internal Brush Brush => _brushContainer ? _brushContainer.brush : null;

        pegi.EnterExitContext context = new();
        public override void Inspect()
        {
            pegi.Nl();

            using (context.StartContext())
            {
                "Brush".PegiLabel(70).Edit_Enter_Inspect(ref _brushContainer).Nl();

                if (!context.IsAnyEntered)
                {
                    base.Inspect();

                    "Max Blood".PegiLabel().Edit(ref _maxBlood).Nl();
                }
            }
        }

        public override string NeedAttention()
        {
            if (!_brushContainer)
                return "No container";

            var br = _brushContainer.brush;

            if (!br.Is3DBrush())
                return "Should be a Sphere Brush";

            if (br.FallbackTarget != TexTarget.RenderTexture)
                return "Should render to Render Texture";

            if (br.GetBlitMode(TexTarget.RenderTexture).GetType() != typeof(BlitModes.Add))
                return "Additive is preferred";

            return base.NeedAttention();
        }
    }

    [PEGI_Inspector_Override(typeof(Pool_BloodParticlesController))] internal class Pool_BloodParticlesControllerDrawer : PEGI_Inspector_Override { }
}