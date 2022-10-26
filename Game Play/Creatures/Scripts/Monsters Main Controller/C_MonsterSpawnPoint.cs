using QuizCanners.Inspect;
using QuizCanners.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public class C_MonsterSpawnPoint : MonoBehaviour, IPEGI
    {
        internal static List<C_MonsterSpawnPoint> s_spawnPoints = new();
     
        void OnEnable() 
        {
            s_spawnPoints.Add(this);
        }

        void OnDisable() 
        {
            s_spawnPoints.Remove(this);
        }

        public void Inspect()
        {
            if (Application.isPlaying)
            {
                if ("Spawn".PegiLabel().Click().Nl())
                    Singleton.Try<Pool_Monsters_Data>(s => s.TrySpawn(transform.position, inst =>
                    {
                        inst.GoTo(Singleton.Get<Singleton_Player>().transform.position);
                    }));
            } else 
            {
                "Can spawn during play".PegiLabel().Write_Hint().Nl();
            }
        }
    }

    [PEGI_Inspector_Override(typeof(C_MonsterSpawnPoint))] internal class C_MonsterSpawnPointDrawer : PEGI_Inspector_Override { }
}
