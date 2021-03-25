using QuizCanners.Inspect;
using QuizCanners.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeons_and_Dragons
{
    public abstract class CreatureStateGeneric<T> : CreatureStateBase, IPEGI, IGotReadOnlyName where T: Creature, new()
    {
        public T Creature => CreatureId?.GetEntity();
        protected override Creature CreatureBase => Creature;
        public abstract DnD_SmartId<T> CreatureId { get; }

        #region Inspector

        protected override void Inspect_Context()
        {
            typeof(T).ToPegiStringType().PegiLabel().enter_Inspect(CreatureId).nl();
        }

        public override void InspectInList(ref int edited, int ind)
        {
            base.InspectInList(ref edited, ind);
            CreatureId.enter_Inspect_AsList(ref edited, ind);
        }
        #endregion
    }

    public abstract class CreatureStateBase : IPEGI_ListInspect
    {
        [SerializeField] private bool _hpInitialized;
        [SerializeField] protected int _currentHp;
        [SerializeField] private CreatureHealthState _creatureHealthState;
        [SerializeField] public int Initiative;
        [SerializeField] private Exhaustion _exhaustion = new();

        public int ExhaustionLevel 
        {
            get => _exhaustion.Level;
            set 
            {
                _exhaustion.Level = value;
                CurrentHitPoints = CurrentHitPoints;
                if (_exhaustion.Death)
                    _creatureHealthState = CreatureHealthState.Dead;
            }
        }

        public virtual void RollInitiative() 
        {
            Initiative = CreatureBase.Roll(AbilityScore.Dexterity).Value;
        }

        public enum CreatureHealthState { Alive, UnconsciousDeathSavingThrows, UnconsciousStable, Dead }

        protected virtual int ArmorClass => CreatureBase == null ? 10 : CreatureBase.ArmorClass;

        public int MaxHp 
        { 
            get 
            {
                int value;
                var creatureBase = CreatureBase;
                if (creatureBase == null)
                    value = 8;
                else
                    value = creatureBase.MaxHitPoints;

                return _exhaustion.HitPointMaximum.ApplyTo(value);
            } 
        }

        public void Resurect() 
        {
            ExhaustionLevel -= 1;
            _creatureHealthState = CreatureHealthState.Alive;
            CurrentHitPoints = Mathf.Max(1, CurrentHitPoints);
        }

        public void AddHitPoints(int toAdd) 
        {
            if (toAdd <= 0)
            {
                Debug.LogError("Adding {0} Hp??".F(toAdd));
                return;
            }

            if (_creatureHealthState == CreatureHealthState.Dead) 
            {
                return;
            }

            CurrentHitPoints = Mathf.Min(MaxHp, CurrentHitPoints + toAdd);

            switch (_creatureHealthState)
            {
                case CreatureHealthState.Alive:  break;
                case CreatureHealthState.UnconsciousDeathSavingThrows: _creatureHealthState = CreatureHealthState.Alive; break;
                case CreatureHealthState.UnconsciousStable: _creatureHealthState = CreatureHealthState.Alive; break;
            }
        }

        public void SubtractHitPoints(int toSubtract)
        {
            if (toSubtract <= 0)
            {
                Debug.LogError("Subtracting {0} Hp??".F(toSubtract));
                return;
            }

            switch (_creatureHealthState) 
            {
                case CreatureHealthState.Alive:
                    int damage = Mathf.Min(CurrentHitPoints, toSubtract);
                    CurrentHitPoints -= toSubtract;

                    var remaining = toSubtract - damage;
                    if (remaining > MaxHp)
                    {
                        _creatureHealthState = CreatureHealthState.Dead;
                    } else 
                    {
                        if (CurrentHitPoints < 1)
                            _creatureHealthState = CreatureHealthState.UnconsciousDeathSavingThrows;
                    }
                    break;
                case CreatureHealthState.UnconsciousDeathSavingThrows:
                    //TODO: Process damage
                    break;
                case CreatureHealthState.UnconsciousStable:
                    _creatureHealthState = CreatureHealthState.UnconsciousDeathSavingThrows;
                    break;
            }

         
        }

        private int CurrentHitPoints
        {
            set
            {
                _hpInitialized = true;
                _currentHp = Mathf.Max(0, value);
            }
            get => _hpInitialized ? System.Math.Min(_currentHp, MaxHp) : MaxHp;
        }

        protected abstract Creature CreatureBase { get; }

        [SerializeField] private pegi.EnterExitContext inspectedStuff = new();

        public virtual List<Attack> GetAttacksList()
        {
            var lst = new List<Attack>();

            if (CreatureBase != null)
            {
                lst.AddRange(CreatureBase.GetAttacks());
            }

            return lst;
        }


        public bool TryTakeHit(Attack attack, RollInfluence influence)
        {
            if (!attack.RollAttack(influence, ArmorClass, out bool isCritical))
            {
                return false;
            }

            var drawDamage = attack.RollDamage(isCritical);

            if (CreatureBase != null)
            {
                SubtractHitPoints(CreatureBase.CalculateDamage(drawDamage, damageType: attack.DamageType));
            }
            else
            {
                SubtractHitPoints(drawDamage);
            }

            return true;
        }

        #region Inspector
        public virtual string GetReadOnlyName() => CreatureBase == null ? "NULL" : CreatureBase.GetNameForInspector();

        public void Inspect()
        {
            if (!inspectedStuff.IsAnyEntered)
            {
                if (CreatureBase != null)
                    CreatureBase.Inspect_StatBlock(GetAttacksList());

                pegi.line(Color.gray);
            }

            using (inspectedStuff.StartContext()) 
            {
                Inspect_Context();
            }

        }

        protected virtual void Inspect_Context() { }

        public void LongRest() 
        {
            _hpInitialized = false;
            _creatureHealthState = CreatureHealthState.Alive;
            _exhaustion.Level -= 1;
        }

        public virtual void InspectInList(ref int edited, int ind)
        {
            var hp = CurrentHitPoints;

            if (_hpInitialized)
                icon.Refresh.Click(LongRest);
            
            switch (_creatureHealthState) 
            {
                case CreatureHealthState.Alive:

                    var ex = ExhaustionLevel;
                    if ("Ex".PegiLabel(20).edit(ref ex, 25))
                        ExhaustionLevel = ex;

                    if ("Hp".PegiLabel(30).editDelayed(ref hp, 40))
                        CurrentHitPoints = hp;

                    if (CurrentHitPoints != MaxHp)
                    {
                        "/{0}".F(MaxHp).PegiLabel(40).write();
                    }
                    break;
                case CreatureHealthState.Dead:
                    if ("Resurect".PegiLabel().Click())
                        Resurect();
                    break;
                default: _creatureHealthState.ToString().PegiLabel().write(); break;
            }

          
        }
        #endregion
    }
}