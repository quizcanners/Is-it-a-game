using QuizCanners.Inspect;
using QuizCanners.Utils;
using RayFire;
using System.Collections.Generic;
using UnityEngine;


namespace QuizCanners.IsItGame
{

    [DisallowMultipleComponent]
    public class C_RayFireRootHelper : MonoBehaviour, IPEGI, INeedAttention
    {
        [SerializeField] private RayfireRigid _rigid;
        [SerializeField] private List<MeshFilter> meshFilters;
        [SerializeField] private float avgVolume = 0;

        #region Inspector
        [SerializeField] private pegi.EnterExitContext _context = new pegi.EnterExitContext();
        [SerializeField] private pegi.CollectionInspectorMeta _meshesCollection = new pegi.CollectionInspectorMeta("Meshes");

        public void Inspect()
        {
            pegi.Nl();

            using (_context.StartContext())
            {

                "Rigid".PegiLabel().Edit_Enter_Inspect(ref _rigid).Nl();
            
                _meshesCollection.Enter_List(meshFilters).Nl();

                if (_context.IsCurrentEntered) 
                {
                    "Thold Volume".PegiLabel().Edit(ref avgVolume);

                    if (Icon.Refresh.Click())
                    {
                        float volume = 0;

                        foreach (var m in meshFilters)
                        {
                            volume += m.sharedMesh.CalculateVolume();
                        }

                        avgVolume = volume / meshFilters.Count;
                    }

                    pegi.Nl();

                    GameObject addToElements = null;

                    if ("Attach to bigger ones".PegiLabel().Edit(ref addToElements).Nl() && addToElements)
                    {
                        foreach (var m in meshFilters)
                        {
                            if (m.sharedMesh.CalculateVolume() > avgVolume) 
                            {
                                var go = Instantiate(addToElements, m.transform);
                                var tf = go.transform;
                                go.transform.localPosition = Vector3.zero;
                                go.transform.localScale = Vector3.one * 1.5f;
                            }
                        }
                    }

                    if ("Clear all children (Shadow)".PegiLabel().Click().Nl())
                    {
                        foreach (var m in meshFilters)
                        {
                            var tf = m.transform;
                            for (int i= tf.childCount-1; i>=0; i--) 
                            {
                                m.transform.GetChild(i).gameObject.DestroyWhatever();
                            }
                        }
                    }

                    if ("Find all Mesh Filters".PegiLabel().Click()) 
                    {
                        meshFilters.Clear();
                        meshFilters = new List<MeshFilter>(GetComponentsInChildren<MeshFilter>());
                    }
                }
            }
        }

     

        public string NeedAttention()
        {
            if (!_rigid)
                return "Rigid not assigned";

            return null;
        }

        #endregion

        void Reset()
        {
            _rigid = GetComponent<RayfireRigid>();
        }
    }

    [PEGI_Inspector_Override(typeof(C_RayFireRootHelper))] internal class C_RayFireRootHelperDrawer : PEGI_Inspector_Override { }
}
