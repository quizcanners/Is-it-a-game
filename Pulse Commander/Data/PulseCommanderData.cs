using QuizCanners.Inspect;
using System;


namespace QuizCanners.IsItGame.Pulse
{
    [Serializable]
    internal class PulseCommanderData : IPEGI, IGotReadOnlyName, IPEGI_Handles
    {
        public PulseArena Arena = new();

        public void Update(float deltaTime) 
        {
            Arena.Update(deltaTime);
        }

        #region Inspector

        public void OnSceneDraw()
        {
            Arena.OnSceneDraw();
        }

        public string GetReadOnlyName() => "Pulse Commander Data";
       
        public void Inspect()
        {
            "Skip Time 1s".PegiLabel().Click().Nl().OnChanged(()=> Update(1));

            Arena.Nested_Inspect();
        }

        #endregion
    }
}