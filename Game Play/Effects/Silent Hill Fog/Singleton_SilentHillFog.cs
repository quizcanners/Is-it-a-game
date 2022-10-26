using KWS;
using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public class Singleton_SilentHillFog : Singleton.BehaniourBase
    {
        [SerializeField] private ParticleSystem _particleSystem;

        public override void Inspect()
        {
            base.Inspect();

            "Particle System".PegiLabel().Edit_IfNull(ref _particleSystem, gameObject).Nl();

            if (_particleSystem) 
            {
                var emission = _particleSystem.emission;
                var isOn = emission.enabled;

                "Emission".PegiLabel().ToggleIcon(ref isOn).Nl(()=> emission.enabled = isOn);

                ParticleSystem.MinMaxCurve amount = emission.rateOverTime;
                float rate = amount.constant;

                "Rate".PegiLabel().Edit(ref rate, 0, 100).Nl(()=> 
                {
                    amount.constant = rate;
                    emission.rateOverTime = amount;
                });
            }
        }
    }

    [PEGI_Inspector_Override(typeof(Singleton_SilentHillFog))] internal class Singleton_SilentHillFogDrawer : PEGI_Inspector_Override { }
}