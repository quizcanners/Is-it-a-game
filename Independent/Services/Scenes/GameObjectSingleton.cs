using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame
{
    public class GameObjectSingleton : MonoBehaviour
    {
        [SerializeField] private SingletonRole _role;

        private enum SingletonRole 
        { 
            MainCamera = 0, 
            Canvas = 1,
            Player = 2,
            DirectionalLight = 3,
            EventSystem = 4,
            EffectManagers = 5,
        }

        private static readonly Dictionary<SingletonRole, GameObject> _inTheScene = new Dictionary<SingletonRole, GameObject>();

        private void Awake()
        {
            CheckSingleton();
        }

#if UNITY_EDITOR
        private void OnEnable()
        {
            CheckSingleton();
        }
#endif

        private void CheckSingleton() 
        {
            if (_inTheScene.TryGetValue(_role, out GameObject go) && go && go != gameObject)
            {
                Destroy(gameObject);
            }
            else
            {
                _inTheScene[_role] = gameObject;
            }
        }

    }
}
