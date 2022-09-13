using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public class C_MonsterDismemberedLimb : MonoBehaviour, IPEGI
    {
        [SerializeField] private GameObject _partToDisconnect;
        [SerializeField] private Transform _dismembermentRoot;

        public Transform Root => _dismembermentRoot ? _dismembermentRoot : transform;

        void OnEnable()
        {
            _partToDisconnect.transform.parent = null;
        }

        private void OnDestroy()
        {
            _partToDisconnect.gameObject.DestroyWhatever();
        }

        void Update()
        {

        }

        public void Inspect()
        {
            pegi.Nl();

            "Disconnect".PegiLabel().Edit(ref _partToDisconnect).Nl();
            "Break Point".PegiLabel().Edit(ref _dismembermentRoot).Nl();

            if ((transform.localPosition != Vector3.zero || transform.localRotation != Quaternion.identity) &&
                "Reset Transform".PegiLabel().Click().Nl())
            {
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }

        }
    }

    [PEGI_Inspector_Override(typeof(C_MonsterDismemberedLimb))] internal class C_MonsterDismemberedLimbDrawer : PEGI_Inspector_Override { }
}