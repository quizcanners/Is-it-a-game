using UnityEngine;
using UnityEngine.UI;
using QuizCanners.Inspect;

namespace QuizCanners.SpecialEffects
{
    public class InvisibleUIGraphic : Graphic, IPEGI
    {
        public override void SetMaterialDirty() { }
        public override void SetVerticesDirty() { }
        public override bool Raycast(Vector2 sp, Camera eventCamera) => true;
        protected override void OnPopulateMesh(VertexHelper vh) => vh.Clear();

        public void Inspect()
        {
            var ico = raycastTarget;
            if ("Raycast Target".PegiLabel().toggleIcon(ref ico))
                raycastTarget = ico;
        }

    }

    [PEGI_Inspector_Override(typeof(InvisibleUIGraphic))] internal class InvisibleUIGraphicDrawer : PEGI_Inspector_Override { }
}