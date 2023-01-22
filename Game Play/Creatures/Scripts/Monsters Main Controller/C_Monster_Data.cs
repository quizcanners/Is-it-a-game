using Dungeons_and_Dragons;
using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.ComponentModel;
using System.Net;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using static QuizCanners.IsItGame.Develop.C_MonsterDetailed;

namespace QuizCanners.IsItGame.Develop
{
    public class C_Monster_Data : MonoBehaviour, IPEGI, IPEGI_Handles, IPEGI_ListInspect
    {
        [SerializeField] private NavMeshAgent navMeshAgent;
        [NonSerialized] private bool _fallbackAlive;
        [NonSerialized] private CharacterState _state;
        [NonSerialized] private int _version = -1;


        public InstanceReference GetReference() => new InstanceReference(this);

        public struct InstanceReference : IPEGI
        {
            public int version;
            public C_Monster_Data enemy { get; private set; }
            private int hash;

            public bool IsValid => enemy && enemy.gameObject.activeSelf && version == enemy._version;

            public void Clear() 
            {
                enemy = null;
            }

            public override string ToString() => enemy ? enemy.name : "NULL";
            public override int GetHashCode() => hash;

            public void Inspect()
            {
                "version: {0}".F(version).PegiLabel();
                pegi.ClickHighlight(enemy);
                pegi.Nl();
                enemy.Nested_Inspect();
              
            }

            public InstanceReference(C_Monster_Data data) 
            {
                version = data._version;
                enemy = data;
                hash = data.gameObject.GetHashCode() + version;
            }
        }

        //Proxies
        private Proxy _proxyState;
        public enum Proxy { None, GPU_Instanced, Detailed }

        [NonSerialized] public C_MonsterDetailed proxy_detailed;
        [NonSerialized] public C_MonsterInstanced proxy_instanced;

        private readonly LogicWrappers.Timer _disruptMovementSeconds = new();

        private readonly LogicWrappers.TimeFixedSegmenter _turnTimer = new(unscaledTime: false, segmentLength: DnDTime.SECONDS_PER_TURN);
        private readonly LoopLock loopLock = new();


        public bool TryGetProxy(out CreatureProxy_Base proxy) 
        {
            proxy = proxy_detailed ? proxy_detailed : proxy_instanced;
            return proxy;
        }

        public bool IsAlive
        {
            get => _state != null ? _state.HealthState == CreatureStateBase.CreatureHealthState.Alive : _fallbackAlive;
            set
            {
                _fallbackAlive = value;

                if (loopLock.Unlocked)
                    using (loopLock.Lock())
                    {
                        if (!value)
                        {
                            if (_state != null)
                                _state.Kill();

                            Pool.Return(this);
                        }

                        if (TryGetProxy(out var proxy))
                            proxy.IsAlive = value;
                    }
            }
        }

        public Vector3 GetActivePosition() => proxy_detailed ? proxy_detailed.GetActivePosition() : (transform.position + Vector3.up * 0.5f); // TODO: GIve position of proxies

        public void RestartMonster(CharacterSheet.SmartId character = null)
        {
            _proxyState = Proxy.None;

            if (character != null)
            {
                _state = new CharacterState(character);
                var ch = character.GetEntity();
                gameObject.name = ch.ToString();
            } else 
            {
                _state = null;
                gameObject.name = "DUMMY";
            }

            _disruptMovementSeconds.Clear();
            _turnTimer.Reset();
            _version++;
            IsAlive = true;
        }

        public void Disintegrate() 
        {
            if (proxy_detailed)
                proxy_detailed.OnDisintegrate();

            IsAlive = false;
            //TODO: Disintegrate child
        }

        public bool TryTakeHit(Attack attack, RollInfluence influence, LimbsControllerState onKillReaction = LimbsControllerState.Giblets)
        {
            if (_state != null)
            {
                var wasAlive = _state.HealthState != CreatureStateBase.CreatureHealthState.Dead;
                var isHit = _state.TryTakeHit(attack, influence);

                if (_state.HealthState != CreatureStateBase.CreatureHealthState.Alive)
                    IsAlive = false;

                // TODO: Call On Hit

                _disruptMovementSeconds.SetMax(isHit ? 1f : 0.3f);

                if (proxy_detailed)
                {
                    proxy_detailed.OnHit(isHit, wasAlive, onKillReaction: onKillReaction);

                    if (wasAlive)
                    {
                        DisconnectProxies();
                        Pool.Return(this);
                    }    
                } else if (proxy_instanced) 
                {
                    proxy_instanced.OnHit(damage: isHit);
                }

                

                return isHit;
            }
            else
            {
                return true;
            }
        }


        public void Update()
        {
            if (_turnTimer.GetSegmentsAndUpdate() > 0)
            {
                _state.OnStartTurn();
                _state.OnEndTurn();
            }

            switch (_state.HealthState)
            {
                case CreatureStateBase.CreatureHealthState.Alive:

                    var ent = _state.CharacterId.GetEntity();
                    if (ent == null)
                        return;

                    if (_disruptMovementSeconds.IsFinished)
                    {
                        navMeshAgent.speed = 1.25f;
                    }
                    else
                        navMeshAgent.speed = 0;

                    break;
                case CreatureStateBase.CreatureHealthState.Dead:

             
                    break;
            }

        }

        #region Pathfinding
        public void GoTo(Vector3 target)
        {
            navMeshAgent.destination = target;
        }

        public void GoTo(Transform target) 
        {
            navMeshAgent.destination = target.position;
        }

        #endregion

        #region Inspector

        public void InspectInList(ref int edited, int index)
        {
            if (_state != null)
                _state.InspectInList(ref edited, index);
            else
                name.PegiLabel().Write();


            if (Icon.Enter.Click())
                edited = index;

            pegi.ClickHighlight(this);
        }

        public override string ToString() => _state == null ? gameObject.name : _state.ToString();

        private readonly pegi.EnterExitContext context = new();

        public bool TrySetProxy(Proxy newProxy)
        {
            if (_proxyState == newProxy)
                return true;

            DisconnectProxies();

            switch (newProxy)
            {
                case Proxy.GPU_Instanced:
                    if (Pool.TrySpawn(transform.position, out proxy_instanced))
                    {
                        proxy_instanced.Connect(this);
                        _proxyState = Proxy.GPU_Instanced;
                    };
                    break;
                case Proxy.Detailed:

                    if (Pool.TrySpawn(transform.position, out proxy_detailed))
                    {
                        _proxyState = Proxy.Detailed;
                        proxy_detailed.RestartMonster(this);
                    }
                    break;
            }

            return _proxyState == newProxy;
        }

        public void Inspect()
        {
            pegi.TryDefaultInspect(this);
            using (context.StartContext())
            {
                if (context.IsAnyEntered == false) 
                {
                    var pr = _proxyState;
                    "Proxy".PegiLabel().Edit_Enum(ref pr).Nl(() => TrySetProxy(pr));                     
                }

                _state.Enter_Inspect().Nl();

                if (proxy_detailed)
                {
                    if ("Detailed Proxy".PegiLabel().IsEntered())
                    {
                        pegi.Nl();
                        "Detailed Proxy".PegiLabel().Write();
                        pegi.ClickHighlight(proxy_detailed).Nl();

                        proxy_detailed.Enter_Inspect().Nl();
                    } else 
                    {
                        pegi.ClickHighlight(proxy_detailed);
                        pegi.Nl();
                    }
                }
               

                if (proxy_instanced)
                {
                    if ("Detailed Instanced".PegiLabel().IsEntered())
                    {
                        pegi.Nl();
                        "Detailed Instanced".PegiLabel().Write();
                        pegi.ClickHighlight(proxy_instanced).Nl();

                        proxy_instanced.Enter_Inspect().Nl();
                    }
                    else
                    {
                        pegi.ClickHighlight(proxy_instanced);
                        pegi.Nl();
                    }
                }
            }
            // if ("Go To Player".PegiLabel().Click().Nl())
            //   navMeshAgent.SetDestination( );

        }

        public void OnSceneDraw()
        {
            if (IsAlive && _state != null)
            {
                pegi.Handle.Label(ToString(), transform.position);
            }
        }

        #endregion

        void DisconnectProxies() 
        {
            if (TryGetProxy(out var prosy))
                prosy.Disconnect();

            proxy_detailed = null;
            proxy_instanced = null;

            _proxyState = Proxy.None;
        }

        void OnDisable()
        {
            DisconnectProxies();
        }

    }

    [PEGI_Inspector_Override(typeof(C_Monster_Data))] internal class C_Monster_DataDrawer : PEGI_Inspector_Override { }
}
