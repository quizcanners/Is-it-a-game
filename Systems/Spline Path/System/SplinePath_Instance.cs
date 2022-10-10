using QuizCanners.Inspect;
using QuizCanners.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.SplinePath
{
    [ExecuteAlways]
    public class SplinePath_Instance : MonoBehaviour, IPEGI, IPEGI_Handles
    {
        internal static List<SplinePath_Instance> s_Instances = new List<SplinePath_Instance>();

        [SerializeField] internal SO_SplinePath config;



        #region Inspector

        public void OnSceneDraw()
        {
            if (config)
            {
                var changes = pegi.ChangeTrackStart();

                config.OnSceneDraw_Nested();
                
                if (changes)
                    config.SetToDirty();
            }
        }

        public void Inspect()
        {
            "Config".PegiLabel(60).Edit(ref config).Nl();

            if (config)
                config.Nested_Inspect();
        }
        #endregion

        void OnEnable() 
        {
            s_Instances.Add(this);
        }

        void OnDisable() 
        {
            s_Instances.Remove(this);
        }

    }

    [PEGI_Inspector_Override(typeof(SplinePath_Instance))]
    internal class SplinePath_InstanceDrawer : PEGI_Inspector_Override { }
}