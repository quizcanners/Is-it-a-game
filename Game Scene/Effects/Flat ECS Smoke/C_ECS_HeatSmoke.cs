using QuizCanners.Inspect;
using QuizCanners.Lerp;
using QuizCanners.TinyECS;
using QuizCanners.Utils;
using UnityEngine;


namespace QuizCanners.IsItGame.Develop
{
    public class C_ECS_HeatSmoke : MonoBehaviour, IPEGI
    {
        [SerializeField] private MeshRenderer _meshRenderer;
        private World<ParticlePhisics>.Entity _entity;

        private ShaderProperty.FloatValue _heat = new("_Heat", 0.01f, 5f);
        private ShaderProperty.FloatValue _dissolve = new("_Dissolve", 0f, 1f);
        private ShaderProperty.FloatValue _seed = new("_Seed", 0f, 1f);
        MaterialPropertyBlock _block;

        LogicWrappers.Request dirty = new LogicWrappers.Request();

        float _visibility = 0;

        public void Restart(World<ParticlePhisics>.Entity entity)
        {
            _seed.SetOn(_block, Random.value);
             _entity = entity;
            _visibility = 0;
            dirty.CreateRequest();
            LateUpdate();
        }


        public float Heat 
        {
            get => _heat.latestValue;
            set 
            {
                _heat.SetOn(_block, value);
                dirty.CreateRequest();
            } 
        }

        public float Dissolve
        {
            get => _dissolve.latestValue;
            set
            {
                _dissolve.SetOn(_block, Mathf.Max(1 - _visibility, value));
                transform.localScale = Vector3.one *  ((0.5f + _seed.latestValue) + value * 3);
                dirty.CreateRequest();
            }
        }

        void LateUpdate() 
        {
            transform.LookAt(Camera.main.transform);

            if (_entity.IsAlive)
            {

                var smoke = _entity.GetComponent<ParticlePhisics.SmokeData>();

                if (smoke.Dissolve>=1)
                {
                    Pool_ECS_HeatSmoke.ReturnToPool(this);
                    return;
                }

                LerpUtils.IsLerpingBySpeed(ref _visibility, 1, 5f, unscaledTime: false);

                Dissolve = smoke.Dissolve;
                Heat = smoke.Temperature;

                transform.position = _entity.GetComponent<ParticlePhisics.PositionData>().Position;
            }

            if (dirty.TryUseRequest()) 
            {
                _meshRenderer.SetPropertyBlock(_block);
            }
        }

        void Awake() 
        {
            _block = new MaterialPropertyBlock();
        }

        void OnDisable() 
        {
            _entity.Destroy();
        }

   
        #region Inspector
        public void Inspect()
        {
            pegi.Nl();

            if (Icon.Delete.Click())
                _entity = new World<ParticlePhisics>.Entity();

            pegi.Nested_Inspect(ref _entity).Nl();

            var d = Dissolve;
            if ("Dissolve".PegiLabel(80).Edit_01(ref d).Nl())
                Dissolve = d;

            var h = Heat;
            if ("Heat".PegiLabel(40).Edit(ref h, 0, 5).Nl())
                Heat = h;
        }

        #endregion

        void Reset() 
        {
            _meshRenderer = GetComponent<MeshRenderer>();
        }
    }

    [PEGI_Inspector_Override(typeof(C_ECS_HeatSmoke))] internal class C_ExplosionSmokeDrawer : PEGI_Inspector_Override { }

}