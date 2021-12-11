using QuizCanners.Inspect;
using System;
using UnityEngine;

namespace QuizCanners.IsItGame.SpaceEffect
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
            "Type".PegiLabel().EditEnum(ref Type).Nl();
            "Size".PegiLabel().Edit(ref Size, 0, 5).Nl();
            "Star Position".PegiLabel().Edit(ref LightSourcePosition).Nl();
            "Star".PegiLabel(90).Edit(ref LightColor).Nl();
            "Background".PegiLabel(90).Edit(ref BackgroundColor).Nl();
            "Visibility".PegiLabel().Edit01(ref Visibility).Nl();

            "Dyson Sphere".PegiLabel().ToggleIcon(ref HasDysonSphere).Nl();
            "Fog".PegiLabel().ToggleIcon(ref HasFog).Nl();
            if (HasFog)
            {
                "Gyroid Fog".PegiLabel().ToggleIcon(ref GyroidFog).Nl();

                "Clouds".PegiLabel(90).Edit(ref CloudsColor).Nl();
                "Clouds 2".PegiLabel(90).Edit(ref CloudsColor2).Nl();

            }
        }

        public void InspectInList(ref int edited, int ind)
        {
            if (this == Selected)
                Icon.Active.Draw();
            else Icon.Play.Click(SetSelected);
            
            this.inspect_Name(delayedEdit: true);

            if (Icon.Enter.Click())
                edited = ind;
        }
    }
}