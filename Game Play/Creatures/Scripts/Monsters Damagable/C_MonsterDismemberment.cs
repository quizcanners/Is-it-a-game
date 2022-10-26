using Dungeons_and_Dragons;
using QuizCanners.Inspect;
using QuizCanners.Utils;
using System.Net;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public class C_MonsterDismemberment : MonoBehaviour, IPEGI, IPEGI_ListInspect
    {
        [SerializeField] private Transform _limbToHide;
        [SerializeField] private C_MonsterDismemberedLimb _demolishablePrefab;

        private C_MonsterDismemberedLimb _instance;
        private Gate.UnityTimeUnScaled _dismambermentDelay = new Gate.UnityTimeUnScaled();

        private static Gate.UnityTimeUnScaled _dismambermentLimiter = new Gate.UnityTimeUnScaled();

        public static bool TryGetDismemberPermission() => _dismambermentLimiter.TryUpdateIfTimePassed(0.5f);

        private bool _demolished;
        private bool _allowDemolition;

        public Vector3 GetDetachPoint()
        {
            if (_instance)
                return _instance.Root.position;

            return _limbToHide.position;
        }

        public void Restart() 
        {
            Demolished = false;
            AllowDemolition = false;
        }

        public bool AllowDemolition 
        {
            get => _allowDemolition;
            set 
            {
                _allowDemolition = value;
                if (_allowDemolition)
                    DemolishIfMarked();
            }
        }

        public void MarkForDemolicion() 
        {
            if (AllowDemolition && TryGetDismemberPermission())
                Demolished = true;
            else
            {
                _dismambermentDelay.Update();
            }

        }

        public void ClearCollars() 
        {
            if (_instance)
                _instance.gameObject.DestroyWhatever();
        }

        private void DemolishIfMarked() 
        {
            if (_dismambermentDelay.ValueIsDefined) 
            {
                if (_dismambermentDelay.TryUpdateIfTimePassed(0.1f))
                    return;

                if (TryGetDismemberPermission())
                {
                    Demolished = true;
                }
            }
        }

       

        public bool Demolished 
        {
            get => _demolished;
            set 
            {
                if (_demolished == value)
                    return;

                if (value) 
                {
                    _limbToHide.localScale = Vector3.one * 0.01f;

                    _instance = Instantiate(_demolishablePrefab, transform);

                    //Pool.TrySpawnIfVisible<BFX_BloodController>(_instance.Root.position);

                    //if (!monster.IsAlive)
                    //BFX_DelayedBloodSpawner.CreateFromTransform(transform, Vector3.zero);

                    Singleton.Try<Pool_VolumetricBlood>(s =>
                    {
                        s.TrySpawnRandom(transform.position, transform.forward, out var instance);
                    });

                    _limbToHide.gameObject.SetActive(false);
                } else 
                {
                    _limbToHide.localScale = Vector3.one;
                    _limbToHide.gameObject.SetActive(true);
                    if (_instance)
                        _instance.gameObject.DestroyWhatever();
                }

                _demolished = value;
            }
        }

        public void Inspect()
        {
            pegi.Nl();


            if (Application.isPlaying)
            {
                "Demolish Marked".PegiLabel().ToggleIcon(ref _allowDemolition).Nl().OnChanged(() => AllowDemolition = _allowDemolition);
            }

            "Rayfire Prefab".PegiLabel().Edit(ref _demolishablePrefab, allowSceneObjects: false).Nl();

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

        public void InspectInList(ref int edited, int index)
        {
            var dem = Demolished;
            if (pegi.ToggleIcon(ref dem))
                Demolished = dem;

            _limbToHide.ToString().PegiLabel().Write();
            pegi.ClickHighlight(this);

            if (Icon.Enter.Click())
                edited = index;
        }
    }

    [PEGI_Inspector_Override(typeof(C_MonsterDismemberment))] internal class C_MonsterDismembermentDrawer : PEGI_Inspector_Override { }
}