using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.Pulse
{
    [ExecuteAlways]
    public class Singleton_PulseCommander : Singleton.BehaniourBase, IPEGI, IPEGI_Handles
    {
        [SerializeField] private PulseCommanderData _testStateData;

        internal static bool DrawCurves;

        internal PulseCommanderData Data => Singleton.TryGetValue<Singleton_GameController, PulseCommanderData>(s => s.PersistentProgressData.User.Campaign.PulseCommander, defaultValue: _testStateData, logOnServiceMissing: false);


        void Update() 
        {
            if (Application.isPlaying)
                Data.Update(Time.deltaTime);
        }

        #region Inspector

        public void OnSceneDraw()
        {
            Data.OnSceneDraw();

            if (pegi.Handle.Button(transform.position, offset: (Vector3.up + Vector3.right) * 1.2f, label: DrawCurves ? "Hide Curves" : "Show Curves"))
                DrawCurves = !DrawCurves;
        }

        public void OnDrawGizmos() => OnSceneDraw();
        
        public override void Inspect()
        {
            pegi.Nl();
            Data.Nested_Inspect();
        }
        #endregion
    }

    [PEGI_Inspector_Override(typeof(Singleton_PulseCommander))] internal class PulseCommanderServiceDrawer : PEGI_Inspector_Override { }
}
