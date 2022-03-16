using PainterTool;
using QuizCanners.Inspect;
using QuizCanners.IsItGame.StateMachine;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public class Singleton_ChornobaivkaController : Singleton.BehaniourBase
    {
        public LayerMask LayersToHit;
        public int Enemies;
        public int SoftLayerToPierce;

        public PlaytimePainter_BrushConfigScriptableObject OnCollisionBrush;

        const int MAX_DISTANCE = 10000;

        public bool CastAll(Ray ray, out RaycastHit hit, float maxDistance = MAX_DISTANCE)
         => Physics.Raycast(ray, out hit, maxDistance: maxDistance, LayersToHit | (1 << SoftLayerToPierce));

        public bool CastHardSurface(Ray ray, out RaycastHit hit, float maxDistance = MAX_DISTANCE)
          => Physics.Raycast(ray, out hit, maxDistance: maxDistance, LayersToHit);

        public bool CastPierce(Ray ray, RaycastHit hardHit, out RaycastHit softHit)
        {
            float dist = Vector3.Distance(hardHit.point, ray.origin) - 0.05f;
            if (dist <= 0)
            {
                softHit = default(RaycastHit);
                return false;
            }

            return Physics.Raycast(ray, out softHit, maxDistance: dist, (1 << SoftLayerToPierce));
        }



        private Gate.Integer _stateVersion = new Gate.Integer();

        private void Update()
        {
            if (_stateVersion.TryChange(GameState.Machine.Version)) 
            {
                switch (GameState.Machine.Get(defaultValue: Game.Enums.PhisicalSimulation.Active)) 
                {
                    case Game.Enums.PhisicalSimulation.Active:
                        Time.timeScale = 1;
                        break;
                    case Game.Enums.PhisicalSimulation.Paused:
                        Time.timeScale = 0;
                        break;
                }
            };
        }


        public override void Inspect()
        {
            base.Inspect();
            "Layers To Hit".PegiLabel(80).Edit_Property(() => LayersToHit, this).Nl();

            "Enemies".PegiLabel(80).Edit_Layer(ref Enemies).Nl();
            "Layer To Pierce".PegiLabel(80).Edit_Layer(ref SoftLayerToPierce).Nl();

            if (Application.isPlaying) 
            {
                
            }

        }




    }

    [PEGI_Inspector_Override(typeof(Singleton_ChornobaivkaController))] internal class Singleton_ChornobaivkaControllerDrawer : PEGI_Inspector_Override {}
}