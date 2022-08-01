using QuizCanners.Inspect;
using QuizCanners.Utils;
using RayFire;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace QuizCanners.IsItGame
{

    public class C_RayFireRespawn : MonoBehaviour, IPEGI
    {

        [SerializeField] private RayfireRigid _prefab;
        [SerializeField] private RayfireRigid _instance;

        void Clear() 
        {
            if (_instance)
            {
                _instance.gameObject.DestroyWhatever();
                _instance = null;
            }    
        }

        void Spawn() 
        {
            _instance = Instantiate(_prefab, transform);
            var tf = _instance.transform;

            tf.localPosition = Vector3.zero;
            tf.localRotation = Quaternion.identity;
            tf.localScale = Vector3.one;
            _needRespawn = false;
        }

        void OnDisable() 
        {
            Clear();
        }

        void OnEnable() 
        {
            if (!_instance)
                Spawn();
        }

        private Gate.UnityTimeScaled _respawnDelay = new Gate.UnityTimeScaled();

        private bool _needRespawn;

        void Update() 
        {
            if (_needRespawn) 
            {
                if (_respawnDelay.TryUpdateIfTimePassed(3f)) 
                {
                    Clear();
                    Spawn();
                }

                return;
            }

            if (_instance) 
            {
                if (_instance.AmountIntegrity < 50) 
                {
                    _needRespawn = true;
                    _respawnDelay.Update();
                }
            }
        }

        public void Inspect()
        {
            pegi.FullWindow.DocumentationClickOpen("This is a component to reinstanciate the Rayfire Prefab");

            pegi.Nl();

            "Prefab".PegiLabel("The original object", 60).Edit(ref _prefab, allowSceneObjects: false).Nl();

            "Instance [Optional]".PegiLabel("The current instance", 120).Edit(ref _instance, allowSceneObjects: true).Nl();

           
            if (_instance)
            {
                if (Application.isPlaying)
                    "Refresh".PegiLabel().Click().OnChanged(()=> { Clear(); Spawn(); });

                pegi.Click(Clear).Nl();
                        
            } else 
            {
                "Instanciate".PegiLabel().Click(Spawn).Nl();
            }

            if (Application.isPlaying && _instance) 
            {
                "Integrity: {0} %".F(Mathf.FloorToInt(_instance.AmountIntegrity)).PegiLabel().Nl();
            }
            
        }
    }

    [PEGI_Inspector_Override(typeof(C_RayFireRespawn))] internal class C_RayFireRespawnDrawer : PEGI_Inspector_Override { }
}