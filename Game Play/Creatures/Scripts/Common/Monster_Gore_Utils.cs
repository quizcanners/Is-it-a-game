using PainterTool.Examples;
using PainterTool;
using QuizCanners.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

namespace QuizCanners.IsItGame.Develop
{
    public static class Monster_Gore_Utils 
    {
        internal static void SprayBloodParticles(Vector3 origin, Vector3 pushVector, float pushForce01) 
        {
            SpawnSDFGore(origin, pushVector, pushForce01);

            Singleton.Try<Pool_BloodParticlesController>(s =>
            {
                int count = (int)(1 + 5 * s.VacancyPortion);

                for (int i = 0; i < count; i++)
                {
                    if (s.TrySpawn(origin, out var b))
                    {
                        Vector3 randomDirection = (0.5f + UnityEngine.Random.value * 0.5f) * UnityEngine.Random.insideUnitSphere;
                        Vector3 direction = Vector3.Lerp(randomDirection, pushVector.normalized * 0.5f, pushForce01 * 0.5f);
                        b.Restart(
                            position: origin + randomDirection * 0.5f,
                            direction: (1 + pushForce01) * 4 * direction,
                            scale: 1.5f);
                    }
                    else
                        break;
                }
            });
        }

        internal static void SpawnSDFGore(Vector3 origin, Vector3 pushVector, float pushForce01)
        {
            Singleton.Try<Pool_SdfGoreParticles>(s =>
            {
                var dir = pushVector * (1 + pushForce01);

                if (s.TrySpawn(origin - dir * 0.3f, out var gore))
                    gore.PushDirection = dir;
            }, logOnServiceMissing: false);
        }
    
        internal static void PaintBigBloodSplatter(Vector3 origin, Vector3 pushVector, float pushForce01) 
        {
            Singleton.Try<Singleton_ChornobaivkaController>(s =>
            {
                if (s.CastGeometry(new Ray(origin, Vector3.down), out var hit, maxDistance: 5))
                {
                    var receiver = hit.transform.gameObject.GetComponent<C_PaintingReceiver>();

                    if (receiver)
                    {
                        var brush = s._config.GibletsSplatterBrush.brush;

                        if (brush != null)
                        {
                            var st = new Stroke(origin);
                            receiver.CreatePaintCommandFor(st, brush, 0).Paint();
                        }
                    }
                }
            });

            Singleton.Try<Singleton_ZibraLiquidsBlood>(sb =>
            {
                if (!sb.TryEmitFrom(origin, pushVector, amountFraction: 3))
                    SpawnSDFGore(origin, pushVector, pushForce01);

            }, onFailed: () => SpawnSDFGore(origin, pushVector, pushForce01));
        }
    
      
    }
}