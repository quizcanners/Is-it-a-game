using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.Pulse
{
    [ExecuteAlways]
    public class Singleton_PulsePath : Singleton.BehaniourBase, IPEGI, IPEGI_Handles
    {
        [SerializeField] private SO_PulseRoutesConfigurations _configs;
     
        internal static bool DrawCurves;

        public PulsePath.Unit CreateUnit(bool isPlayer) 
        {
            var unit = new PulsePath.Unit()
            {
                isPlayer = isPlayer
            };

            return unit;
        }

        internal PulsePath Data => _configs._pathConfigurations.TryGet(0);

        void Update() 
        {
            if (Application.isPlaying)
                Data.Update(Time.deltaTime);
        }

        #region Inspector

        public void OnSceneDraw()
        {
            var changes = pegi.ChangeTrackStart();
            Data.OnSceneDraw_Nested();
            if (changes)
                _configs.SetToDirty();

            if (pegi.Handle.Button(transform.position, offset: (Vector3.up + Vector3.right) * 1.2f, label: DrawCurves ? "Hide Curves" : "Show Curves"))
                DrawCurves = !DrawCurves;
        }

        public void OnDrawGizmos() => this.OnSceneDraw_Nested();
        
        public override void Inspect()
        {
            "Curves".PegiLabel(60).ToggleIcon(ref DrawCurves);

            pegi.Nl();
            "Configs".PegiLabel(60).Edit_Inspect(ref _configs);
        }
        #endregion
    }

    [PEGI_Inspector_Override(typeof(Singleton_PulsePath))] internal class PulseCommanderServiceDrawer : PEGI_Inspector_Override { }
}
