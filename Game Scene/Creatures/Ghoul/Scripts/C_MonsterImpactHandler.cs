using PainterTool;
using PainterTool.Examples;
using QuizCanners.Utils;
using UnityEngine;


namespace QuizCanners.IsItGame.Develop
{
    public class C_MonsterImpactHandler : MonoBehaviour
    {
        private C_MonsterEnemy monster;

        void OnCollisionEnter(Collision collision)
        {
            if (!Camera.main.IsInCameraViewArea(transform.position))
                return;

            var magn = collision.relativeVelocity.magnitude;

            if (magn > 3f)
            {
                Game.Enums.SoundEffects sound;

                if (magn > 13) 
                {
                    sound = Game.Enums.SoundEffects.BodyImpact;

                    if (!monster)
                        monster = GetComponentInParent<C_MonsterEnemy>();

                    if (monster && !monster.Kinematic) 
                    {
                        monster.Giblets(collision.relativeVelocity.normalized, pushForce01: 0.6f);
                    }
                }
                else
                if (magn > 8)
                {
                    sound = Game.Enums.SoundEffects.BodyImpact;

                    foreach (var c in collision.contacts)
                    {
                        Singleton.Try<Pool_BloodParticlesController>(s => s.TrySpawnIfVisible(transform.position, onInstanciate: inst =>
                        {
                            inst.Restart(transform.position, c.normal*4, scale: magn * 0.05f);
                        }));
                    }

                    Singleton.Try<Singleton_ChornobaivkaController>(s => 
                    {
                        var receivers = transform.GetComponentsInParent<C_PaintingReceiver>();

                        if (s)

                        if (receivers.Length > 0) 
                        {
                            C_PaintingReceiver receiver = receivers[0];
                            var cnt = collision.contacts[0];
                            var tex = receiver.GetTexture();
                            var st = receiver.CreateStroke(cnt);
                            BrushTypes.Sphere.Paint(receiver.CreatePaintCommandForSphereBrush(st, s.OnCollisionBrush.brush, 0));
                        }

                    });


                }
                else if (magn > 5)
                {
                    sound = Game.Enums.SoundEffects.ArmorImpact;

                    Singleton.Try<Pool_SmokeEffects>(s => s.TrySpawnIfVisible(transform.position, onInstanciate: inst =>
                    {
                        inst.PlayAnimateFromDot(density: magn * 0.01f);
                    }));

                }
                else
                    sound = Game.Enums.SoundEffects.Ice;

                //const float GAP = 0.03f;

                //if (!sound.CanPlay()) //minGap: GAP))
                  //  return;

              // Debug.Log("Collision: {0}".F(collision.relativeVelocity.magnitude));

                sound.PlayOneShotAt(transform.position, clipVolume: magn * 10);
            }
        }
    }
}