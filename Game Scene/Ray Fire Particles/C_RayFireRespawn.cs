using QuizCanners.Inspect;
using QuizCanners.RayTracing;
using QuizCanners.Utils;
using RayFire;
using UnityEngine;


namespace QuizCanners.IsItGame
{

    public class C_RayFireRespawn : C_RayT_EnvironmentElement, IPEGI
    {

        [SerializeField] private GameObject _undestroyed;
        [SerializeField] private RayfireRigid _prefab;
        [SerializeField] private RayfireRigid _instance;

        public void OnDamage()
        {
            if (_undestroyed && _undestroyed.gameObject.activeSelf)
            {
                _undestroyed.gameObject.SetActive(false);
                CheckSpawnDestroyed();
            }
        }

        void CheckSpawn()
        {
            if (_undestroyed)
            {
                _undestroyed.gameObject.SetActive(true);
                if (_instance)
                    _instance.gameObject.SetActive(false);
            }
            else
            {
                CheckSpawnDestroyed();
            }
            _needRespawn = false;
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
                CheckSpawn();
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
                    CheckSpawn();
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

        public override void Inspect()
        {
            base.Inspect();

            pegi.FullWindow.DocumentationClickOpen("This is a component to reinstanciate the Rayfire Prefab");

            pegi.Nl();

            "Prefab".PegiLabel("The original object", 60).Edit(ref _prefab, allowSceneObjects: false).Nl();

            "Undestroyed".PegiLabel("The Original version", 80).Edit(ref _undestroyed, allowSceneObjects: true).Nl();

            "Instance [Optional]".PegiLabel("The current instance", 120).Edit(ref _instance, allowSceneObjects: true).Nl();


            if (_instance)
            {
                if (Application.isPlaying)
                    "Refresh".PegiLabel().Click().OnChanged(() => { Clear(); CheckSpawn(); });

            }
            else
            {
                "Instanciate".PegiLabel().Click(CheckSpawn).Nl();
            }

            if (Application.isPlaying && _instance)
            {
                "Integrity: {0} %".F(Mathf.FloorToInt(_instance.AmountIntegrity)).PegiLabel().Nl();
            }


        }

        public override string NeedAttention()
        {
            if (!_prefab)
                return " No prefab";

            return base.NeedAttention();
        }
    }

    [PEGI_Inspector_Override(typeof(C_RayFireRespawn))] internal class C_RayFireRespawnDrawer : PEGI_Inspector_Override { }
}