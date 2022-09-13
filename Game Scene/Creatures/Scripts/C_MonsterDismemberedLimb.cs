using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public class C_MonsterDismemberedLimb : MonoBehaviour
    {
        [SerializeField] private GameObject _partToDisconnect;


        void OnEnable()
        {
            _partToDisconnect.transform.parent = null;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}