using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public class C_MonsterDismemberment : MonoBehaviour, IPEGI, IPEGI_ListInspect
    {
        [SerializeField] private Transform _limbToHide;
        [SerializeField] private C_MonsterDismemberedLimb _demolishablePrefab;

        private C_MonsterDismemberedLimb _instance;
        private Gate.UnityTimeUnScaled _dismambermentDelay = new Gate.UnityTimeUnScaled();

        private bool _demolished;
        private bool _allowDemolition;


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
            if (AllowDemolition)
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

        public void DemolishIfMarked() 
        {
            if (_dismambermentDelay.ValueIsDefined) 
            {
                if (_dismambermentDelay.TryUpdateIfTimePassed(0.5f))
                    return;

                Demolished = true;
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

                    Pool.TrySpawnIfVisible<BFX_BloodController>(_instance.Root.position);

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

           
            "Demolish Marked".PegiLabel().ToggleIcon(ref _allowDemolition).Nl().OnChanged(()=> AllowDemolition = _allowDemolition);

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