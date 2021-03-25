using QuizCanners.Inspect;
using System;
using UnityEngine;

namespace Mushroom
{
    [Serializable]
    public class SpaceAndStarsConfiguration : IPEGI, IPEGI_ListInspect, IGotName
    {
        public static SpaceAndStarsConfiguration Selected;

        [SerializeField] private string _key;
        [SerializeField] public Vector2 LightSourcePosition = Vector2.zero;
        [SerializeField] public Color LightColor = Color.gray;
        [SerializeField] public Color CloudsColor = Color.gray;
        [SerializeField] public Color CloudsColor2 = Color.gray;
        [SerializeField] public Color BackgroundColor = Color.clear;
        [SerializeField] public float Size = 1;
        [SerializeField] public StarType Type;
        [SerializeField] public bool HasDysonSphere;
        [SerializeField] public bool HasFog;
        [SerializeField] public bool GyroidFog;
        [SerializeField] public float Visibility = 1;

        public enum StarType { BLACK_HOLE = 0, STAR = 1 }

        public string NameForInspector { get => _key; set => _key = value; }

        public void SetSelected() => Selected = this;

        public void Inspect()
        {
            "Type".PegiLabel().editEnum(ref Type).nl();
            "Size".PegiLabel().edit(ref Size, 0, 5).nl();
            "Star Position".PegiLabel().edit(ref LightSourcePosition).nl();
            "Star".PegiLabel(90).edit(ref LightColor).nl();
            "Background".PegiLabel(90).edit(ref BackgroundColor).nl();
            "Visibility".PegiLabel().edit01(ref Visibility).nl();

            "Dyson Sphere".PegiLabel().toggleIcon(ref HasDysonSphere).nl();
            "Fog".PegiLabel().toggleIcon(ref HasFog).nl();
            if (HasFog)
            {
                "Gyroid Fog".PegiLabel().toggleIcon(ref GyroidFog).nl();

                "Clouds".PegiLabel(90).edit(ref CloudsColor).nl();
                "Clouds 2".PegiLabel(90).edit(ref CloudsColor2).nl();

            }
        }

        public void InspectInList(ref int edited, int ind)
        {
            if (this == Selected)
                icon.Active.draw();
            else icon.Play.Click(SetSelected);
            
            this.inspect_Name(delayedEdit: true);

            if (icon.Enter.Click())
                edited = ind;
        }
    }
}