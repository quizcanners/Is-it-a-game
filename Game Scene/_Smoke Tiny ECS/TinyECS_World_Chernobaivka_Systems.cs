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

        public void UpdateSystems(float deltaTime, SO_ECSParticlesConfig config) 
        {
            var world = this.GetWorld();

            int count = 0;

            float heatDissolve = Mathf.Clamp01(1 - deltaTime * 10);

            Vector3 displace = deltaTime * (0.5f * Vector3.up);
            Vector3 wind = deltaTime * config.Wind; 

            Measure("Pos Smoke", () =>
               world.RunSystem((ref PositionData m, ref SmokeData s)
                   => 
               {
                   count++;
                   m.Position += displace + wind * s.Buoyancy * QcMath.SmoothStep(0.5f, 5, m.Position.y + s.Dissolve);
                   s.Dissolve += deltaTime * (dissolveSpeed);
                   s.Temperature *= heatDissolve;
               }));

            dissolveSpeed = count / 500f;


            Measure("Upward Push", ()=> 
            {
                world.WithAll<UpwardImpulse>().Run( (ref UpwardImpulse impulse, IEntity e) =>
                {
                    var outPut = deltaTime * impulse.EnergyLeft;

                    var pos = impulse.Position;

                    world.RunSystem<PositionData, AffectedByWind>((ref PositionData position) =>
                    {
                        var vector = position.Position - pos;
                        float distance = vector.magnitude;

                        float hor = vector.XZ().magnitude;

                        float isAbove = vector.y / hor;

                        float swirl = vector.y - hor;

                        isAbove = Mathf.Clamp01(isAbove);

                        vector.Normalize();

                        Vector3 pushVector = Vector3.Lerp((-vector).Y(0) * 2f, vector * 0.25f, isAbove); // Below the mushroom suck in

                        vector = (pushVector  + Vector3.up * isAbove * isAbove * 8);
                        


                        vector *= outPut / (1 + distance);
                        position.Position += vector ;
                    });

                    impulse.EnergyLeft -= Time.deltaTime * 30;
                    if (impulse.EnergyLeft <= 0)
                        e.Destroy();
                });
            });


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

            /*Measure("Sm Pos", () =>
            world.RunSystem((SmokeData smoke, PositionData smokePos) =>
            {
                float accumulatedTemperature = 0;

                world.RunSystem((ref HeatSource heat, PositionData heatPos) =>
                {
                    accumulatedTemperature += deltaTime * heat.Temperature;
                });
            })
            );*/

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
