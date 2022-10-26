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
        private IEntity _entity;

        private readonly ShaderProperty.FloatValue _heat = new("_Heat", 0.01f, 5f);
        private readonly ShaderProperty.FloatValue _dissolve = new("_Dissolve", 0f, 1f);
        private readonly ShaderProperty.FloatValue _seed = new("_Seed", 0f, 1f);
       
        private readonly LogicWrappers.Request _dirty = new();

        MaterialPropertyBlock _block;
        float _visibility = 0;

        public void Restart(IEntity entity)
        {
            _seed.SetOn(_block, Random.value);
             _entity = entity;
            _visibility = 0;
            _dirty.CreateRequest();
            LateUpdate();
        }

        public void AddHeat(float value)
        {
            if (_entity.TryGetComponent<ParticlePhisics.HeatedSmokeData>(out var dta))
            {
                dta.Temperature += value;
                _entity.SetComponent(dta);
            }
        }

        private float Heat 
        {
            get => _heat.latestValue;
            set 
            {
                _heat.SetOn(_block, value);
                _dirty.CreateRequest();
            } 
        }

        private float Dissolve
        {
            get => _dissolve.latestValue;
            set
            {
                _dissolve.SetOn(_block, Mathf.Max(1 - Mathf.Pow(_visibility,3f), value));
                transform.localScale = Vector3.one *  ((1f + _seed.latestValue*2) + value * 5);
                _dirty.CreateRequest();
            }
        }

        void LateUpdate() 
        {
            transform.LookAt(Camera.main.transform);

            if (_entity.IsAlive)
            {
                if (_entity.TryGetComponent<ParticlePhisics.HeatedSmokeData>(out var smoke))
                {
                    if (smoke.Dissolve >= 1)
                    {
                        Pool.Return(this);
                        return;
                    }

                    LerpUtils.IsLerpingBySpeed(ref _visibility, 1, 5f, unscaledTime: false);

                    Dissolve = smoke.Dissolve;
                    Heat = smoke.Temperature;

                   if (_entity.TryGetComponent<ParticlePhisics.PositionData>(out var pos)) 
                   {
                        transform.position = pos.Position;
                   }
                }
            }

            if (_dirty.TryUseRequest()) 
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

            pegi.Nested_Inspect_Value(ref _entity).Nl();

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