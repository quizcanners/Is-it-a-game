using QuizCanners.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop.Turrets
{
    public class Abstract_TurretHead : MonoBehaviour
    {
        internal static List<Abstract_TurretHead> allInstances = new();

        protected virtual void OnEnable()
        {
            allInstances.Add(this);
        }

        protected virtual void OnDisable()
        {
            allInstances.Remove(this);
        }
    }
}
