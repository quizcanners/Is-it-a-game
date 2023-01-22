using QuizCanners.Inspect;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public abstract class CreatureProxy_Base : MonoBehaviour, IPEGI
    {
        [NonSerialized] public C_Monster_Data Parent;

        private bool _isAliveInternal;
        private bool _hadParent;
        public bool IsAlive
        {
            get => Parent ? Parent.IsAlive : _isAliveInternal;
            set
            {
                if (Parent)
                    Parent.IsAlive = value;
                _isAliveInternal = value;
            }
        }

        public virtual void Connect(C_Monster_Data data) 
        {
            Parent = data;
            _hadParent = true;
        }

        public virtual void Disconnect() 
        {
            Parent = null;
            _isAliveInternal = false;
        }

        public bool IsTestDummy => !_hadParent;

        public abstract void Giblets(Vector3 pushVector = default, float pushForce01 = 0);

        public virtual void Inspect()
        {
        }

        public abstract void ApplyImpact(RaycastHit hit, ref bool pierced, Action<Vector3> onDismemberment);
    }
}