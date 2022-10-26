using Dungeons_and_Dragons;
using QuizCanners.Inspect;
using QuizCanners.Migration;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public class Pool_Monsters_Data : PoolSingletonBase<C_Monster_Data>, ITaggedCfg
    {

        public bool Autospawn;

        [SerializeField] private float spawnDelay = 0.1f;

        [NonSerialized] private int _randomSpawnPoint = -1;

        protected override int MAX_INSTANCES => 10000;
       
        protected override void OnInstanciated(C_Monster_Data inst)
        {
            var character = Singleton.TryGetValue<Singleton_DnD, CharacterSheet>(s => s.DnDPrototypes.Characters.GetRandom());
            inst.RestartMonster(character: new CharacterSheet.SmartId(character));

            Singleton.Try<Singleton_Player>(p => inst.GoTo(p.transform));

            if (inst.TrySetProxy(C_Monster_Data.Proxy.GPU_Instanced))
            {

            }
            else
            {
                inst.TrySetProxy(C_Monster_Data.Proxy.Detailed);
            }
        }

  
   


        private readonly LogicWrappers.TimeFixedSegmenter _betweenSpawns = new(unscaledTime: false, segmentLength: 0.1f, 1);


        void SpawnOnPoints() 
        {
            if (C_MonsterSpawnPoint.s_spawnPoints.Count > 0)
            {
                var origin = C_MonsterSpawnPoint.s_spawnPoints.GetRandom(ref _randomSpawnPoint);
                TrySpawn(origin.transform.position);
            }
            else
                QcLog.ChillLogger.LogWarningOnce("Monster Spawn Points not found", key: "NoMstrSpwnPnts", this);
        }

        void Update()
        {
            if (Autospawn && _betweenSpawns.GetSegmentsAndUpdate(segment: spawnDelay) > 0)
            {
                SpawnOnPoints();
            }
        }

        #region Inspector

        public override void InspectInList(ref int edited, int ind)
        {
            pegi.ToggleIcon(ref Autospawn);
            base.InspectInList(ref edited, ind);
        }

        public override void Inspect()
        {
            if (Application.isPlaying)
            {
                "Autospaw".PegiLabel().ToggleIcon(ref Autospawn).Nl();

                if (Autospawn) 
                {
                    var spawnsPerSecond = 1f/spawnDelay;

                    "Spawn/second".PegiLabel().Edit(ref spawnsPerSecond, 1, 50).Nl().OnChanged(()=> 
                    {
                        spawnDelay = 1f/spawnsPerSecond;
                    });
                }

                if (!Autospawn && "Spawn".PegiLabel().Click().Nl())
                    SpawnOnPoints();
            }
            base.Inspect();
        }

        #endregion

        #region Encode & Decode
        public string TagForConfig => "Monster Spawner";
        public CfgEncoder Encode() => new CfgEncoder().Add_Bool("sp", Autospawn);

        public void DecodeTag(string key, CfgData data)
        {
            switch (key)
            {
                case "sp": Autospawn = data.ToBool(); break;
            }
        }
        #endregion
    }

    [PEGI_Inspector_Override(typeof(Pool_Monsters_Data))] internal class Pool_Monsters_DataDrawer : PEGI_Inspector_Override { }
}
