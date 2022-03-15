using QuizCanners.Inspect;
using QuizCanners.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame
{
    public class Singleton_SmokeEffectController : PoolSingletonBase<C_SmokeEffectOnImpact> 
    {
        protected override int MAX_INSTANCES => 50;

        protected override void OnInstanciated(C_SmokeEffectOnImpact inst)
        {
            inst.PlayAnimateFromDot();

            for (int i = 0; i < instances.Count - 1; i++)
            {
                inst.TryConsume(instances[i]);
            }
        }

        
    }

    [PEGI_Inspector_Override(typeof(Singleton_SmokeEffectController))] internal class Singleton_SmokeEffectControllerDrawer : PEGI_Inspector_Override { }

}