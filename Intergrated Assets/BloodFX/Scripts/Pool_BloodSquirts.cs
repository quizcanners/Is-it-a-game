using QuizCanners.Inspect;
using QuizCanners.Utils;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{

    [ExecuteAlways]
    public class Pool_BloodSquirts : PoolSingletonBase<C_BloodSquirt>
    {
        [SerializeField] private  List<Material> _bloodMaterials = new List<Material>();
             
        public bool TryGetReplacementMaterial(Material original, out Material replacement) 
        {
            replacement = original;
            if (!original)
                return false;

            foreach (var m in _bloodMaterials) 
            {
                if (!m || original == m)
                    return false;

                if (original.mainTexture == m.mainTexture)
                {
                    replacement = m;
                    return true;
                }    
            }

            return false;
        }

        protected override void OnInstanciated(C_BloodSquirt inst)
        {
            inst.Play(GetScaleFactorFromDistance(inst.transform.position) * 2);

        }

        public override void Inspect()
        {
            base.Inspect();

            "Materials".PegiLabel().Edit_List_UObj(_bloodMaterials).Nl();
        }
    }

    //[CanEditMultipleObjects]
    [PEGI_Inspector_Override(typeof(Pool_BloodSquirts))] internal class Pool_BloodSquirtsDrawer : PEGI_Inspector_Override { }
}