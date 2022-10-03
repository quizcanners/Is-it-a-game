using Dungeons_and_Dragons;
using QuizCanners.Inspect;
using QuizCanners.IsItGame.Pulse;
using QuizCanners.Migration;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public class Pool_MonstersController : PoolSingletonBase<C_MonsterEnemy>, ITaggedCfg
    {
        public bool Autospawn;
        protected override int MAX_INSTANCES => 300;
        public string TagForConfig => "Monster Spawner";

        protected override void OnInstanciated(C_MonsterEnemy inst)
        {
            var unit = Singleton.TryGetValue<Singleton_PulsePath, PulsePath.Unit>(s => s.CreateUnit(isPlayer: false));
            var character = Singleton.TryGetValue<Singleton_DnD, CharacterSheet>(s => s.DnDPrototypes.Characters.GetRandom());
            inst.RestartMonster(unit: unit, character: new CharacterSheet.SmartId(character));
            inst.Update();
        }

        private readonly LogicWrappers.TimeFixedSegmenter _betweenSpawns = new(unscaledTime: false, segmentLength: 1f, 1);
        
        void Update() 
        {
            if (Autospawn && _betweenSpawns.GetSegmentsAndUpdate() > 0) 
            {
                TrySpawn();
                   
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

                if (!Autospawn && "Spawn".PegiLabel().Click().Nl())
                    TrySpawn();
            }
            base.Inspect();
        }

        #endregion


        #region Encode & Decode
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

    [PEGI_Inspector_Override(typeof(Pool_MonstersController))] internal class Pool_MonstersControllerDrawer : PEGI_Inspector_Override { }
}
