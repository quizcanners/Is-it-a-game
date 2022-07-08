using QuizCanners.Lerp;
using QuizCanners.TinyECS;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public partial class ParticlePhisics
    {

        public QcDebug.TimeProfiler.DictionaryOfParallelTimers Timer => QcDebug.TimeProfiler.Instance["SDF Phys"];

        private float dissolveSpeed = 0;

        public void UpdateSystems(float deltaTime) 
        {
            var world = this.GetWorld();

            int count = 0;

            float heatDissolve = Mathf.Clamp01(1 - deltaTime * 10);

            Measure("Pos Smoke", () =>
               world.RunSystem((ref PositionData m, ref SmokeData s)
                   => 
               {
                   count++;
                   m.Position += Vector3.up * ( 0.5f ) * deltaTime;
                   s.Dissolve += deltaTime * (dissolveSpeed);
                   s.Temperature *= heatDissolve;
               }));

            dissolveSpeed = count / 500f;

            Measure("Heat Imp Ent", () =>
            world.RunSystem((ref HeatSource source, ref HeatImpulse impulse, IEntity entity) =>
            {
                LerpUtils.IsLerpingBySpeed(ref impulse.ImpulseExpansion01,
                    to: impulse.IsExpanding ? 1 : 0,
                    speed: impulse.IsExpanding ? 5 : 1,
                    unscaledTime: false);

                source.Temperature = impulse.ImpulseExpansion01 * impulse.MaxHeat;
                if (impulse.ImpulseExpansion01 == 1)
                    impulse.IsExpanding = false;

                if (impulse.ImpulseExpansion01 == 0)
                    entity.Destroy();
            }));

            Measure("Sm Pos", () =>
            world.RunSystem((SmokeData smoke, PositionData smokePos) =>
            {
                float accumulatedTemperature = 0;

                world.RunSystem((ref HeatSource heat, PositionData heatPos) =>
                {
                    accumulatedTemperature += deltaTime * heat.Temperature;
                });
            })
            );

            void Measure(string name, Action action)
            {
                try
                {
                    using (Timer.Last(name).Start())
                    {
                        action.Invoke();
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(name + "Failed");
                    Debug.LogException(ex);
                }
            }
        }
    }
}
