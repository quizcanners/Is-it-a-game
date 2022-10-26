using QuizCanners.Inspect;
using QuizCanners.RayTracing;
using QuizCanners.Utils;
using RayFire;
using UnityEngine;


namespace QuizCanners.IsItGame
{
    [SelectionBase]
    [AddComponentMenu("PrimitiveTracing/Scene Prefab/Ray Fire Respawn")]
    public class C_RayFireRespawn : MonoBehaviour, IPEGI, INeedAttention
    {
        [SerializeField] private GameObject _undestroyed;
        [SerializeField] private RayfireRigid _prefab;
        [SerializeField] private RayfireRigid _instance;

        public void SetDamaged (bool isDamaged)
        {
            if (isDamaged) 
            {
                if (_undestroyed)
                    _undestroyed.SetActive(false);

                CheckSpawnDestroyed();
            } 
            else 
            {
                if (_undestroyed)
                    _undestroyed.SetActive(true);
                else
                {
                    if (_instance)
                        _instance.gameObject.SetActive(false);
                    else
                        CheckSpawnDestroyed();
                }

                _needRespawn = false;
            }
        }

        private void CheckSpawnDestroyed()
        {
            if (_instance)
            {
                _instance.gameObject.SetActive(true);
                return;
            }

            _instance = Instantiate(_prefab, transform);
            var tf = _instance.transform;

            tf.localPosition = Vector3.zero;
            tf.localRotation = Quaternion.identity;
            tf.localScale = Vector3.one;
        }

        void Clear()
        {
            if (_instance)
            {
                _instance.gameObject.DestroyWhatever();
                _instance = null;
            }
        }

        void OnDisable()
        {
            if (Application.isPlaying)
                Clear();
        }

        void OnEnable()
        {
            if (Application.isPlaying)
                SetDamaged(false);
        }

        private Gate.UnityTimeScaled _respawnDelay = new Gate.UnityTimeScaled();

        private bool _needRespawn;

        void Update()
        {
            if (!Application.isPlaying)
                return;

            if (_needRespawn)
            {
                if (_respawnDelay.TryUpdateIfTimePassed(8f))
                {
                    Clear();

                    if (Application.isEditor)
                        SetDamaged(false);
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

        #region Inspector
        public void Inspect()
        {
            pegi.FullWindow.DocumentationClickOpen("This is a component to reinstanciate the Rayfire Prefab");

            pegi.Nl();

            "Prefab".PegiLabel("The original object", 60).Edit(ref _prefab, allowSceneObjects: false).Nl();

            "Undestroyed".PegiLabel("The Original version", 80).Edit(ref _undestroyed, allowSceneObjects: true).Nl();

            "Instance [Optional]".PegiLabel("The current instance", 120).Edit(ref _instance, allowSceneObjects: true).Nl();

            if (_undestroyed) 
            {
                if (!_undestroyed.GetComponent<Collider>())
                {
                    "Attach Mesh Collider to trigger ".PegiLabel().WriteWarning();

                    if ("Add".PegiLabel().Click())
                        _undestroyed.AddComponent<MeshCollider>();

                    pegi.Nl();
                }
            }


            if (!_instance)
                "Instanciate".PegiLabel().Click(()=> SetDamaged(true)).Nl();
            else
            {
                if (Application.isPlaying)
                {
                    "Refresh".PegiLabel().Click().OnChanged(() => { Clear(); SetDamaged(false); });
                    "Integrity: {0} %".F(Mathf.FloorToInt(_instance.AmountIntegrity)).PegiLabel().Nl();
                }
                else
                {
                    GameObject copySource = null;

                    if ("Copy Components From".PegiLabel().Edit(ref copySource, gameObject).Nl())
                    {
                        var inGo = _instance.gameObject;

                        inGo.AddOrCopyComponent<RayfireRigid>(copySource);
                        inGo.AddOrCopyComponent<RayfireUnyielding>(copySource);
                        inGo.AddOrCopyComponent<RayfireDebris>(copySource);
                        inGo.AddOrCopyComponent<RayfireDust>(copySource);
                        inGo.AddOrCopyComponent<RayfireSound>(copySource);
                    }
                }
            }
        }

        public string NeedAttention()
        {
            if (!_prefab)
                return " No prefab";

            return null;
        }

        #endregion
    }

    [PEGI_Inspector_Override(typeof(C_RayFireRespawn))] internal class C_RayFireRespawnDrawer : PEGI_Inspector_Override { }
}