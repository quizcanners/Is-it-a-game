using QuizCanners.Inspect;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public abstract class CreatureProxy_Base : MonoBehaviour, IPEGI
    {
        [NonSerialized] public C_Monster_Data parent;

        private bool _isAliveInternal;
        private bool _hadParent;
        public bool IsAlive
        {
            get => parent ? parent.IsAlive : _isAliveInternal;
            set
            {
                if (parent)
                    parent.IsAlive = value;
                _isAliveInternal = value;
            }
        }

        public virtual void Connect(C_Monster_Data data) 
        {
            parent = data;
            _hadParent = true;
        }

        public virtual void Disconnect() 
        {
            parent = null;
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