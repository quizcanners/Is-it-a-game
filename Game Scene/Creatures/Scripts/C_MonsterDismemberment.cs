using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public class C_MonsterDismemberment : MonoBehaviour, IPEGI
    {
        [SerializeField] private Transform _limbToHide;
        [SerializeField] private C_MonsterDismemberedLimb _demolishablePrefab;

        private C_MonsterDismemberedLimb _instance;
        private Gate.UnityTimeUnScaled _dismambermentDelay = new Gate.UnityTimeUnScaled();

        private bool _demolished;

        public void MarkForDemolicion() 
        {
            _dismambermentDelay.Update();
        }

        public void DemolishIfMarked() 
        {
            if (_dismambermentDelay.TryUpdateIfTimePassed(secondsPassed: 0.3f, out var wasInitialized) && wasInitialized) 
            {
                Demolished = true;
            }
        }

        bool Demolished 
        {
            get => _demolished;
            set 
            {
                if (value) 
                {
                    _limbToHide.localScale = Vector3.one * 0.01f;
                    _instance = Instantiate(_demolishablePrefab, transform);
                } else 
                {
                    _limbToHide.localScale = Vector3.one;
                    _instance.gameObject.DestroyWhatever();
                }

                _demolished = value;
            }
        }

        public void Inspect()
        {
            pegi.Nl();

            "Rayfire Prefab".PegiLabel().Edit(ref _demolishablePrefab).Nl();

            "Linb to Hide".PegiLabel().Edit(ref _limbToHide).Nl();

            if (_limbToHide && (!_limbToHide.IsChildOf(transform) || (transform == _limbToHide)))
                "You should hide a child object".PegiLabel().WriteWarning().Nl();


            if (Application.isPlaying) 
            {
                var dem = Demolished;
                if ("Demolished".PegiLabel().ToggleIcon(ref dem).Nl())
                    Demolished = dem;

            }
        }
    }

    [PEGI_Inspector_Override(typeof(C_MonsterDismemberment))] internal class C_MonsterDismembermentDrawer : PEGI_Inspector_Override { }
}