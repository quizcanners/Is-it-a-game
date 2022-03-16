using Dungeons_and_Dragons;
using PainterTool.Examples;
using QuizCanners.Inspect;
using QuizCanners.IsItGame.Pulse;
using QuizCanners.RayTracing;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public partial class C_MonsterEnemy : MonoBehaviour, IPEGI, IPEGI_Handles, INeedAttention, IGotReadOnlyName, IPEGI_ListInspect
    {
        public C_RayRendering_ExplosionImactEffectController ImpactController;
        [SerializeField] private C_PaintingReceiver _paintingReciever;
        [SerializeField] protected AnimatedSkeleton skeleton;
        [SerializeField] protected List<Collider> colliders;
        [SerializeField] protected List<C_MonsterImpactHandler> impactHandlers;
        [SerializeField] protected List<GameObject> bones;
        [SerializeField] protected List<Rigidbody> rigidbodies;
        [SerializeField] protected List<C_MonsterDetachableChunk> detachments;
        [SerializeField] protected Transform weaponRoot;

        [NonSerialized] public int postDeathDamage;

        private CharacterState _state;
        private PulsePath.Unit _unit;
        private LimbsControllerState _limbsState;
        private bool _fallbackAlive;
        private bool _animationInvalidated;

        private readonly LogicWrappers.Timer _disruptMovementSeconds = new();
        private readonly LogicWrappers.TimeFixedSegmenter _turnTimer = new(segmentLength: DnDTime.SECONDS_PER_TURN);
        private readonly LogicWrappers.Timer _postDeathTimer = new();

        public bool IsAlive
        {
            get => _state != null ? _state.HealthState == CreatureStateBase.CreatureHealthState.Alive : _fallbackAlive;
            set
            {
                if (!value)
                {
                    if (_state != null)
                        _state.Kill();
                }

                skeleton.Down = !value;

                _fallbackAlive = value;

                Singleton.Try<Singleton_ChornobaivkaController>(s => 
                {
                    LayerMask targetMask = value ? s.Enemies : s.SoftLayerToPierce;

                    foreach (var c in colliders)
                        c.gameObject.layer = targetMask;
                });

                UpdateState_Internal();
            }
        }

        public LimbsControllerState LimbsState
        {
            get => _limbsState;
            protected set
            {
                if (value == _limbsState)
                    return;

                _limbsState = value;

                switch (value) 
                {
                    case LimbsControllerState.Ragdoll: _animationInvalidated = true; break;
                    case LimbsControllerState.Animation: _animationInvalidated = false; break;
                }

                UpdateState_Internal();
            }
        }

        public bool ShowDamage 
        {
            get => _paintingReciever.UsesDamageMaterial;

            set 
            {
                if (!value) 
                {
                    ImpactController.ResetEffect();
                    _paintingReciever.ResetEffect();
                } else 
                {
                    _paintingReciever.GetDamageMaterial();
                }

                UpdateState_Internal();
            }
        }

        public bool TryGetNearestRigidbody(Vector3 position, out Rigidbody rb, out float dist) 
        {
            rb = null;
            dist = float.MaxValue;
            
            foreach (var r in rigidbodies) 
            {
                var curDist = Vector3.Distance(r.transform.position, position);
                if (dist > curDist) 
                {
                    dist = curDist;
                    rb = r;
                }
            }

            return rb;
        }

        public Collider GetRandomCollider() => colliders.GetRandom();

        public void Push(float force, Vector3 origin, float radius) 
        {
            if (LimbsState == LimbsControllerState.Giblets) 
            {
                force = 0.1f;
            }

            foreach (var r in rigidbodies)
            {
                r.AddExplosionForce(explosionForce: force, origin, explosionRadius: radius);
            }
        }

        private void UpdateState_Internal()
        {
            var isGiblets = LimbsState == LimbsControllerState.Giblets;

            weaponRoot.gameObject.SetActive(!isGiblets);
            bool collider = IsAlive || LimbsState != LimbsControllerState.Giblets;
            SetColliders(collider);
            bones.SetActive_List(!isGiblets && ShowDamage);

            skeleton.animator.enabled = !_animationInvalidated; 

            foreach (var rb in rigidbodies)
            {
                rb.isKinematic = LimbsState == LimbsControllerState.Animation;
                rb.detectCollisions = collider;
                rb.useGravity = !isGiblets;
            }

            foreach (var det in detachments)
                det.Detached = LimbsState != LimbsControllerState.Animation;

            var alive = IsAlive;
            foreach (var ic in impactHandlers) 
            {
                ic.enabled = !alive;
            }
        }

        public Vector3 GetActivePosition ()
        {
            if (!rigidbodies.IsNullOrEmpty())
               return rigidbodies[0].transform.position;
            else
               return transform.position + Vector3.up * 0.75f;
        }

        public void RestartMonster(PulsePath.Unit unit = null, CharacterSheet.SmartId character = null)
        {
            _unit = unit;
            if (_unit != null)
            {
                transform.position = _unit.GetPosition();
                _state = new CharacterState(character);
                var ch = character.GetEntity();
                gameObject.name = ch.GetReadOnlyName();
            } else
            {
                _state = null;
            }
            _disruptMovementSeconds.Clear();
            _turnTimer.Reset();
            ShowDamage = false;
            postDeathDamage = 0;
            IsAlive = true;
            LimbsState = LimbsControllerState.Animation;
            skeleton.ResetAnimator(transform.position);

            foreach (var rb in rigidbodies)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            Update();
        }

        public void DropRigid() 
        {
            _postDeathTimer.Restart(2);
            LimbsState = LimbsControllerState.Ragdoll;
            IsAlive = false;
        }

        public void Giblets(Vector3 pushVector = default, float pushForce01 = 0)
        {
            if (!Camera.main.IsInCameraViewArea(GetActivePosition()) || !ImpactController)
            {
                Pool_MonstersController.ReturnToPool(this);
                return;
            }

            ShowDamage = true;
            IsAlive = false;
            LimbsState = LimbsControllerState.Giblets;
            bones.SetActive_List(false);
            skeleton.Freeze();

            Vector3 origin = GetActivePosition();

            Game.Enums.SoundEffects.BodyExplosion.PlayOneShotAt(origin);

            Singleton.Try<Pool_SdfGoreParticles>(s =>
            {
                var dir = pushVector * (1 + pushForce01);

                if (s.TrySpawn(origin - dir*0.3f, out var gore))
                    gore.PushDirection = dir;

            });

            ImpactController.Play(origin - (1 - pushForce01) * 0.5f * pushVector, 1, disintegrate: true);

            foreach (var rb in rigidbodies)
            {
                rb.angularVelocity = Quaternion.identity.eulerAngles;
                rb.velocity = rb.velocity.Y(0) * 0.15f;
            }

            Singleton.Try<Pool_BloodParticlesController>(s =>
            {

                int count = (int)(1 + 30 * s.VacancyPortion);

                for (int i = 0; i < count; i++)
                {
                    if (s.TrySpawn(origin, out var b))
                    {
                        Vector3 randomDirection = (0.5f + UnityEngine.Random.value * 0.5f) * UnityEngine.Random.insideUnitSphere;
                        Vector3 direction = Vector3.Lerp(randomDirection, pushVector.normalized * 0.5f, pushForce01*0.5f);
                        b.Restart(
                            position: origin + randomDirection * 0.5f, 
                            direction: direction * (1 + pushForce01) * 4, 
                            scale: 1.5f);
                    }
                    else
                        return;
                }
            });
        }

        public bool TryTakeHit(Attack attack, RollInfluence influence, LimbsControllerState onKillReaction = LimbsControllerState.Giblets) 
        {
            if (_state != null) 
            {
                var wasAlive = _state.HealthState != CreatureStateBase.CreatureHealthState.Dead;
                var isHit = _state.TryTakeHit(attack, influence);

                if (_state.HealthState != CreatureStateBase.CreatureHealthState.Alive)
                    IsAlive = false; 
                
                switch (_state.HealthState) 
                {
                    case CreatureStateBase.CreatureHealthState.UnconsciousDeathSavingThrows: break;
                    case CreatureStateBase.CreatureHealthState.UnconsciousStable: break;
                    case CreatureStateBase.CreatureHealthState.Dead:

                        if (wasAlive)
                        {
                            _postDeathTimer.Restart(2);
                            switch (onKillReaction) 
                            {
                                case LimbsControllerState.Ragdoll:
                                case LimbsControllerState.Animation: 
                                    LimbsState = onKillReaction; break;
                                default: 
                                    Giblets(); break;
                            }
                        }
                        break;
                    default:
                        if (isHit)
                        {
                            skeleton.GetHit();
                            _disruptMovementSeconds.SetMax(0.3f);
                        }
                        else
                        {
                            skeleton.PlayBlockAnimation();
                            _disruptMovementSeconds.SetMax(1f);
                        }
                        break;
                }

                return isHit;
            } else 
            {
                skeleton.GetHit();
                return true;
            }
        }

        protected void SetColliders(bool areEnabled)
        {
            foreach (var c in colliders)
                c.enabled = areEnabled;
        }

        public void Update()
        {
            if (_unit != null)
            {
                if (_turnTimer.GetSegmentsAndUpdate() > 0) 
                {
                    _state.OnStartTurn();
                    _state.OnEndTurn();
                }

                skeleton.Down = _state.HealthState != CreatureStateBase.CreatureHealthState.Alive && !ImpactController.IsPlaying;

                switch (_state.HealthState)
                {
                    case CreatureStateBase.CreatureHealthState.Alive:

                        if (!_disruptMovementSeconds.IsFinished)
                            break;

                        var ent = _state.CharacterId.GetEntity();
                        if (ent == null)
                            return;

                        _unit.Update(Time.deltaTime, _state.CharacterId.GetEntity()[SpeedType.Walking]);
                        transform.position = _unit.GetPosition();
                        skeleton.OnMove(transform.position);

                        if (skeleton.Direction.magnitude > 0.01f)
                            transform.LookAt(transform.position + skeleton.Direction, Vector3.up);
                        break;
                    case CreatureStateBase.CreatureHealthState.Dead:

                        switch (LimbsState) 
                        {
                            case LimbsControllerState.Giblets:

                                foreach (var rb in rigidbodies) 
                                {
                                    rb.velocity *= (1 - Time.deltaTime);
                                }

                                if (!ImpactController || !ImpactController.IsPlaying)
                                    Pool_MonstersController.ReturnToPool(this);
                                break;
                            case LimbsControllerState.Ragdoll:
                            case LimbsControllerState.Animation:
                                if (_postDeathTimer.IsFinished)
                                {
                                    var mgmt = Singleton.Get<Pool_MonstersController>();

                                    if (mgmt.VacancyPortion > 0.5f && Camera.main.IsInCameraViewArea(GetActivePosition()))
                                        _postDeathTimer.Restart(3f);
                                    else
                                        Giblets();
                                }
                                break;
                            default:
                                if (!ImpactController || !ImpactController.IsPlaying)
                                    Pool_MonstersController.ReturnToPool(this);
                                break;
                        }

                        break;
                }
            }
        }

        #region Inspector

        private readonly pegi.EnterExitContext context = new();

        public void InspectInList(ref int edited, int index)
        {
            if (_state != null)
                _state.InspectInList(ref edited, index);
            else if (gameObject.name.PegiLabel().ClickLabel() | Icon.Enter.Click())
                edited = index;

            gameObject.ClickHighlight();
        }

        public string GetReadOnlyName() => _state == null ? gameObject.name : _state.GetReadOnlyName(); 
        
        public void Inspect()
        {
            pegi.Nl();

            using (context.StartContext())
            {
                if (!context.IsAnyEntered) 
                {
                    "Weapon Root".PegiLabel(80).Edit_IfNull(ref weaponRoot, gameObject).Nl();
                    "Painting".PegiLabel(70).Edit_IfNull(ref _paintingReciever, gameObject).Nl();
                    "Impact".PegiLabel(60).Edit_IfNull(ref ImpactController, gameObject).Nl();

                    if (Application.isPlaying)
                    {
                        var damaged = ShowDamage;
                        "Damaged".PegiLabel().ToggleIcon(ref damaged).Nl().OnChanged(()=> ShowDamage = damaged);

                        var state = LimbsState;
                        "Limbs".PegiLabel(50).Edit_Enum(ref state).Nl().OnChanged(() => LimbsState = state);

                        var alive = IsAlive;
                        "IsAlive".PegiLabel().ToggleIcon(ref alive).Nl().OnChanged(() => IsAlive = alive);

                        "Down".PegiLabel().Click(() => skeleton.Down = true);
                        "Giblets".PegiLabel().Click(() => Giblets()).Nl();
                        "Restart".PegiLabel().Click(() => RestartMonster(null, null)).Nl();
                    }
                }

                _state.Enter_Inspect().Nl();
                "Skeleton".PegiLabel().Enter_Inspect(skeleton).Nl();
                "Character And Position".PegiLabel().Enter_Inspect(_unit).Nl();
                "Colliders".PegiLabel().Enter_List_UObj(colliders).Nl();
                if (context.IsCurrentEntered)
                {
                    "Refresh Colliders".PegiLabel().Click().Nl()
                        .OnChanged(() => colliders = new List<Collider>(GetComponentsInChildren<Collider>()));
                
                    if ("Add Impact Controller".PegiLabel().Click()) 
                    {
                        foreach(var c in colliders) 
                        {
                            if (c & !c.GetComponent<C_MonsterImpactHandler>())
                                c.gameObject.AddComponent<C_MonsterImpactHandler>();
                        }
                    }
                }

                "Imact Controllers".PegiLabel().Enter_List_UObj(impactHandlers).Nl();
                if (context.IsCurrentEntered) 
                {
                    "Refresh Impact Controllers".PegiLabel().Click().Nl()
                       .OnChanged(() => impactHandlers = new List<C_MonsterImpactHandler>(GetComponentsInChildren<C_MonsterImpactHandler>()));
                }


                "Bones".PegiLabel().Enter_List_UObj(bones).Nl();

                "Rigidbodies".PegiLabel().Enter_List_UObj(rigidbodies).Nl();
                if (context.IsCurrentEntered)
                {
                    if (rigidbodies.Count > 0)
                        "{0} Will be the center point".F(rigidbodies[0].name).PegiLabel().WriteHint().Nl();

                    "Refresh Rigidbodies".PegiLabel().Click().Nl()
                       .OnChanged(() => rigidbodies = new List<Rigidbody>(GetComponentsInChildren<Rigidbody>()));
                }

                "Detachables".PegiLabel().Enter_List_UObj(detachments).Nl();

                if (context.IsCurrentEntered)
                    "Refresh Detachments".PegiLabel().Click().Nl()
                       .OnChanged(() => detachments = new List<C_MonsterDetachableChunk>(GetComponentsInChildren<C_MonsterDetachableChunk>()));

            }
        }

        public string NeedAttention()
        {
            return skeleton.NeedAttention();
        }

        public void OnSceneDraw()
        {
            if (IsAlive && _state != null)
            {
                pegi.Handle.Label(GetReadOnlyName(), transform.position);
            }
        }

        #endregion

        void Reset()
        {
            skeleton.animator = GetComponent<Animator>();
            colliders = new List<Collider>(GetComponentsInChildren<Collider>());
        }

        void Awake() 
        {
            LimbsState = LimbsControllerState.Animation;
        }

        public enum LimbsControllerState { Undefined, Animation, Giblets, Ragdoll }
    }

    [PEGI_Inspector_Override(typeof(C_MonsterEnemy))] internal class C_MonsterEnemyDrawer : PEGI_Inspector_Override { }
}
