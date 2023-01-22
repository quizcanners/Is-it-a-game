using PainterTool.Examples;
using QuizCanners.Inspect;
using QuizCanners.RayTracing;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    [SelectionBase]
    public partial class C_MonsterDetailed : CreatureProxy_Base, INeedAttention, IPEGI_ListInspect
    {
        public C_RayRendering_ExplosionImactEffectController ImpactController;
        [SerializeField] private C_PaintingReceiver _paintingReciever;
        [SerializeField] protected Monster_AnimatedSkeleton skeleton;
        [SerializeField] protected List<Collider> colliders;
        [SerializeField] protected List<C_MonsterImpactHandler> impactHandlers;
        [SerializeField] protected List<GameObject> bones;
        [SerializeField] protected List<Rigidbody> rigidbodies;
        [SerializeField] protected List<C_RayRendering_DynamicPrimitive> dynamicPrimitives;
        [SerializeField] protected List<C_MonsterDetachableChunk> detachments;
        [SerializeField] protected List<C_MonsterDismemberment> dismemberments;
        [SerializeField] protected Transform weaponRoot;

        private LimbsControllerState _limbsState;
        private bool _animationInvalidated;

        [NonSerialized] private int _postDeathDamage;

        private readonly LogicWrappers.Timer _postDeathTimer = new();
        private readonly Gate.Vector3Value _deltaPosition = new();

        public void OnAnyDamage() 
        {
            if (!IsAlive) 
            {
                _postDeathDamage += 1;
                if (_postDeathDamage > 6)
                    Giblets();
            }
        }

        public override void ApplyImpact(RaycastHit hit, ref bool pierced, Action<Vector3> onDismemberment) 
        {
            if (!Camera.main.IsInCameraViewArea(hit.point, maxDistance: 50))
                return;

            if (ShowDamage &&  (LimbsState == LimbsControllerState.Animation || LimbsState == LimbsControllerState.Ragdoll))
            {
                ImpactController.Play(hit.point, 1, disintegrate: false, origin: hit.transform);
            }

            if (!IsAlive && LimbsState == LimbsControllerState.Animation)
            {
                DropRigid();
                pierced = true;
            }

            var dismember = hit.collider.gameObject.GetComponentInParent<C_MonsterDismemberment>();
            if (dismember)
            {
                dismember.MarkForDemolicion();

                if (!IsAlive)
                {
                    onDismemberment.Invoke(dismember.GetDetachPoint());
                }
            }
            
        }

        public void OnIsAliveStateChange(bool value) 
        {
            Singleton.Try<Singleton_ChornobaivkaController>(s =>
            {
                LayerMask targetMask = value ? s.Enemies : s.SoftLayerToPierce;

                foreach (var c in colliders)
                    c.gameObject.layer = targetMask;
            });

            UpdateState_Internal();

            skeleton.Down = !value;

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
                    case LimbsControllerState.Disintegrating:
                    case LimbsControllerState.Giblets:
                    case LimbsControllerState.Ragdoll:
                        _animationInvalidated = true;
                        if (Parent)
                            Parent.IsAlive = false;
                        break;
                    case LimbsControllerState.Animation: _animationInvalidated = false; break;
                }

                UpdateState_Internal();
            }
        }

        public bool Kinematic 
        {
            get => LimbsState == LimbsControllerState.Animation; //LimbsState != LimbsControllerState.Ragdoll && LimbsState != LimbsControllerState.Disintegrating;
        }

        public bool IsCollider 
        {
            get 
            {
                if (IsAlive)
                    return true;

                return LimbsState switch
                {
                    LimbsControllerState.Giblets => false,
                    LimbsControllerState.Disintegrating => true,
                    LimbsControllerState.Animation => true,
                    LimbsControllerState.Ragdoll => true,
                    _ => true,
                };
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

        public Vector3 GetActivePosition ()
        {
            if (!rigidbodies.IsNullOrEmpty())
               return rigidbodies[0].transform.position;
            else
               return transform.position + Vector3.up * 0.75f;
        }

        public void RestartMonster(C_Monster_Data data)
        {
            Parent = data;
            
            ShowDamage = false;
          
            LimbsState = LimbsControllerState.Animation;
            skeleton.ResetAnimator(transform.position);

            _postDeathDamage = 0;
            foreach (var rb in rigidbodies)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            foreach (C_MonsterDismemberment dis in dismemberments)
                dis.Restart();
            
            Update();
        }

        public void DropRigid() 
        {
            _postDeathTimer.Restart(2);
            LimbsState = LimbsControllerState.Ragdoll;
           
        }

        public void OnDisintegrate() 
        {
            if (!Camera.main.IsInCameraViewArea(GetActivePosition()))
            {
                Pool.Return(this);
                return;
            }

            _postDeathTimer.Restart(2);

            ShowDamage = true;
            LimbsState = LimbsControllerState.Disintegrating;
            bones.SetActive_List(false);

            Vector3 origin = GetActivePosition();

            ImpactController.Play(origin , 1, disintegrate: true);

            foreach (var rb in rigidbodies)
            {
               // rb.angularVelocity = Quaternion.identity.eulerAngles;
                rb.velocity = rb.velocity.Y(0) * 0.15f;
            }

            Singleton.Try<Pool_BloodParticlesController>(s =>
            {
                float vacancy = s.VacancyPortion;

                int count = (int)(1 + 40 * vacancy);

                for (int i = 0; i < count; i++)
                {
                    if (s.TrySpawn(origin, out var b))
                    {
                        Vector3 direction = (0.5f + UnityEngine.Random.value * 0.5f) * UnityEngine.Random.insideUnitSphere;

                        direction = direction.Y(Mathf.Abs(direction.y));

                        b.Restart(
                            position: origin + direction * 0.5f,
                            direction: direction * 3,
                            scale: 0.5f + (1 - vacancy) * 2);
                    }
                    else
                        return;
                }
            });

        }

        public override void Giblets(Vector3 pushVector = default, float pushForce01 = 0)
        {
            IsAlive = false;

            if (LimbsState == LimbsControllerState.Giblets || LimbsState == LimbsControllerState.Disintegrating)
                return;

            if (!IsTestDummy && (!Camera.main.IsInCameraViewArea(GetActivePosition())))
            {
                Pool.Return(this);
                return;
            }

            ShowDamage = true;
            LimbsState = LimbsControllerState.Giblets;
            bones.SetActive_List(false);
            skeleton.Freeze();

            Vector3 origin = GetActivePosition();

            Game.Enums.UiSoundEffects.BodyExplosion.PlayOneShotAt(origin);

            foreach (var rb in rigidbodies)
            {
                // rb.angularVelocity = Quaternion.identity.eulerAngles;
                rb.velocity = rb.velocity.Y(0) * 0.15f;
            }

            ImpactController.Play(origin - pushVector.normalized * 0.5f, 1, disintegrate: true);

            if (IsVisibleByCamera(maxDistance: 50))
            {
                Monster_Gore_Utils.SprayBloodParticles(origin: origin, pushVector: pushVector, pushForce01: pushForce01);
            }
            else
            {
                Monster_Gore_Utils.PaintBigBloodSplatter(origin: origin, pushVector: pushVector, pushForce01: pushForce01);
            }

            Singleton.Try<Pool_VolumetricBlood>(s =>
            {
                for (int i = 0; i < 2; i++)
                    if (!s.TrySpawnRandom(origin, pushVector, out var inst, size: 2.5f))
                        break;
            });
        }

        public void OnHit(bool gotHit, bool wasAlive, LimbsControllerState onKillReaction = LimbsControllerState.Giblets) 
        {
            if (gotHit && wasAlive)
            {
                _postDeathTimer.Restart(2);
                switch (onKillReaction)
                {
                    case LimbsControllerState.Ragdoll:
                    case LimbsControllerState.Animation:
                    case LimbsControllerState.Disintegrating:
                        LimbsState = onKillReaction; break;
                    default:
                        Giblets(); break;
                }
            }
            else
            {
                if (IsVisibleByCamera())
                {
                    if (gotHit)
                    {
                        skeleton.GetHit();
                    }
                    else
                    {
                        skeleton.PlayBlockAnimation();
                    }
                }
            }
        }

        protected void SetColliders(bool areEnabled)
        {
            foreach (var c in colliders)
                c.enabled = areEnabled;
        }

        protected bool IsVisibleByCamera(float size = 1, float maxDistance = -1) 
        {
            return Camera.main.IsInCameraViewArea(GetActivePosition(), objectSize: size, maxDistance: maxDistance);
        }

        public void Update()
        {
          //  if (IsTestDummy)
            //    return;

            skeleton.Down = !IsAlive; //_state.HealthState != CreatureStateBase.CreatureHealthState.Alive;// && !ImpactController.IsPlaying;

            if (IsAlive) 
            {
                if (Parent)
                {
                    transform.SetPositionAndRotation(Parent.transform.position, Parent.transform.rotation);
                    skeleton.OnMove(transform.position);
                } else 
                {
                    skeleton.OnMove(transform.position);
                    if (skeleton.Direction.magnitude > 0.01f)
                        transform.LookAt(transform.position + skeleton.Direction, Vector3.up);
                }
               

             
            } else 
            {
                switch (LimbsState)
                {
                    case LimbsControllerState.Giblets:

                        /*
                        foreach (var rb in rigidbodies) 
                        {
                            rb.velocity *= (1 - Time.deltaTime);
                        }*/

                        if (!ImpactController.IsPlaying)
                            Pool.Return(this);
                        break;

                    case LimbsControllerState.Disintegrating:
                        if (!ImpactController.IsPlaying)
                            Pool.Return(this);
                        break;

                    case LimbsControllerState.Ragdoll:
                    case LimbsControllerState.Animation:
                        if (_postDeathTimer.IsFinished)
                        {
                            var mgmt = Singleton.Get<Pool_MonstersDetailed>();

                            if (mgmt.VacancyPortion < 0.5f)
                            {
                                OnDisintegrate();
                            }
                            else
                            if (_deltaPosition.TryChange(transform.position, changeTreshold: 1))
                            {
                                _postDeathTimer.Restart(3f);
                            }
                            else
                            {
                                if (IsVisibleByCamera(maxDistance: 200f))//Camera.main.IsInCameraViewArea(GetActivePosition()))
                                    _postDeathTimer.Restart(3f);
                                else
                                    OnDisintegrate();//Giblets();
                            }
                        }
                        break;
                    default:
                        if (!ImpactController || !ImpactController.IsPlaying)
                            Pool.Return(this);
                        break;
                }

            }


        }

        private void UpdateState_Internal()
        {
            var isGiblets = LimbsState == LimbsControllerState.Giblets;

            weaponRoot.gameObject.SetActive(!isGiblets);
            bool collider = IsCollider;
            SetColliders(collider);
            bones.SetActive_List(!isGiblets && ShowDamage && LimbsState != LimbsControllerState.Disintegrating);

            skeleton.animator.enabled = !_animationInvalidated;

            foreach (var d in dynamicPrimitives)
                d.enabled = false;// isGiblets;

            var kinematic = Kinematic;

            foreach (var rb in rigidbodies)
            {
                rb.isKinematic = kinematic;
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

            if (LimbsState == LimbsControllerState.Giblets || LimbsState == LimbsControllerState.Disintegrating || LimbsState == LimbsControllerState.Animation) 
            {
                foreach (C_MonsterDismemberment dis in dismemberments)
                    dis.ClearCollars();
            }

            if (IsVisibleByCamera(maxDistance: 30f) && LimbsState == LimbsControllerState.Ragdoll) 
            {
                foreach (C_MonsterDismemberment dis in dismemberments)
                    dis.AllowDemolition = true;
            } 
        }

        #region Inspector

        private readonly pegi.EnterExitContext context = new();

        public void InspectInList(ref int edited, int index)
        {
      
            if (gameObject.name.PegiLabel().ClickLabel() | Icon.Enter.Click())
                edited = index;

            pegi.ClickHighlight(gameObject);
        }

        public override string ToString() => gameObject.name;

        public override void Inspect()
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
                        "Restart".PegiLabel().Click(() => RestartMonster(null)).Nl();
                    }
                }

                "Skeleton".PegiLabel().Enter_Inspect(skeleton).Nl();
         
                InspectChildList("Colliders", colliders);
                if (context.IsCurrentEntered)
                {
                    if ("All On".PegiLabel().Click().Nl())
                        SetColliders(true);
                    if ("All Off".PegiLabel().Click().Nl())
                        SetColliders(false);

                    if ("Add Impact Controller".PegiLabel().Click())
                        foreach (var c in colliders)
                            if (c & !c.GetComponent<C_MonsterImpactHandler>())
                                c.gameObject.AddComponent<C_MonsterImpactHandler>();
                }
                        

                InspectChildList("Imact Controllers", impactHandlers);

                "Bones".PegiLabel().Enter_List_UObj(bones).Nl();

                InspectChildList("Rigidbodies", rigidbodies);
                if (context.IsCurrentEntered)
                {
                    if (rigidbodies.Count > 0)
                        "{0} Will be the center point".F(rigidbodies[0].name).PegiLabel().Write_Hint().Nl();
                }


                InspectChildList("Detachables", detachments);

                InspectChildList("Dismemberments", dismemberments);

                InspectChildList("Dynamic Primitives", dynamicPrimitives);

                void InspectChildList<T>(string label, List<T> list) where T: Component
                {
                    label.PegiLabel().Enter_List_UObj(list).Nl();

                    if (context.IsCurrentEntered)
                        "Refresh {0}".F(label).PegiLabel().Click().Nl()
                           .OnChanged(() =>
                           {
                               list.Clear();
                               list.AddRange(GetComponentsInChildren<T>());
                           });
                }

            }
        }

        public string NeedAttention()
        {
            return skeleton.NeedAttention();
        }

       

        #endregion

        void OnDisable()
        {
            Pool.Return(this);
        }

        void Reset()
        {
            skeleton.animator = GetComponent<Animator>();
            colliders = new List<Collider>(GetComponentsInChildren<Collider>());
        }

        void Awake() 
        {
            LimbsState = LimbsControllerState.Animation;
        }

        public enum LimbsControllerState { Undefined, Animation, Giblets, Ragdoll, Disintegrating }
    }

    [PEGI_Inspector_Override(typeof(C_MonsterDetailed))] internal class C_MonsterEnemyDrawer : PEGI_Inspector_Override { }
}
