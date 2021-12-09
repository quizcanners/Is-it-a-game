using QuizCanners.Inspect;
using QuizCanners.SpecialEffects;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static QuizCanners.Utils.QcDebug;

namespace QuizCanners.IsItGame.UI
{
    public class UiViewService : IsItGameServiceBase, Singleton.ILoadingProgressForInspector
    {
        [SerializeField] private GameObject _loadingScreen;
        [SerializeField] private BlurTransitionSimple _blurOverlayTransition;
        [SerializeField] private BlurTransitionSimple _blurHexagonalTransition;
        [SerializeField] private ScreenBlurController _screenBlur;

        [SerializeField] private UiViewEnumeratedScriptableObject _views;
        [SerializeField] private RectTransform _root;
        [SerializeField] private RectTransform _raycastBlockObject;
        [SerializeField] private List<CachedView> _cachedViews = new();
        [Serializable] public struct CachedView : IPEGI_ListInspect, IGotReadOnlyName
        {
            public IigEnum_UiView ViewEnum;
            public GameObject Instance;

            public string GetReadOnlyName() => ViewEnum.ToString();

            public void InspectInList(ref int edited, int index)
            {
                pegi.editEnum(ref ViewEnum, width: 90);
                pegi.edit(ref Instance, pegi.UnityObjectSource.Scene); 
            }
        }

        private GameObject _currentViewInstance;
        private InstanceSource _currentViewIsAddressable;
        private IigEnum_UiView _targetView = IigEnum_UiView.None;
        private IigEnum_UiView _currentView = IigEnum_UiView.None;
        private ScreenChangeState _screenChangeState = ScreenChangeState.Standby;
        private UiTransitionType _currentTransitionType;
        private UiTransitionType _targetTransitionType;
        private UiRaycastBlock _raycastBlock = UiRaycastBlock.Undecided;
        private UiRaycastBlock RaycastBlock 
        { 
            get => _raycastBlock; 
            set 
            {
                _raycastBlock = value;
                _raycastBlockObject.gameObject.SetActive(_raycastBlock == UiRaycastBlock.On);
            } 
        }

        private enum InstanceSource {Unknown, Addressable, Resources, Cached }

        private readonly List<IigEnum_UiView> _viewsStack = new();

        private BlurTransitionSimple TransitionGraphic => _currentTransitionType switch
                {
                    UiTransitionType.Hexagonal => _blurHexagonalTransition,
                    UiTransitionType.CrossFade => _blurOverlayTransition,
                    UiTransitionType.ZoomOut => _blurOverlayTransition,
                    _ => _blurOverlayTransition,
                };
    
        public void Show(IigEnum_UiView view, bool clearStack, UiTransitionType transition = UiTransitionType.CrossFade) 
        {
            if (clearStack)
                _viewsStack.Clear();
            else
            {
                var ind = _viewsStack.IndexOf(view);
                if (ind != -1)
                    _viewsStack.RemoveRange(ind, _viewsStack.Count - ind);
                else if (_targetView != IigEnum_UiView.None)
                    _viewsStack.Add(_targetView);
            }

            _targetView = view;
            _targetTransitionType = transition;
        }

        public void Hide(IigEnum_UiView view, UiTransitionType transition = UiTransitionType.CrossFade)
        {
            if (_targetView == view) 
            {
                _targetTransitionType = transition;
                _targetView = _viewsStack.TryTake(_viewsStack.Count-1, defaultValue: IigEnum_UiView.None); 
            } else 
            {
                if (_viewsStack.Contains(view))
                    _viewsStack.Remove(view);
            }
        }

        public void HideCurrent(UiTransitionType transition = UiTransitionType.CrossFade) 
        {
            Hide(_currentView, transition);
        }

        public void ShowError(string text) 
        {
            _targetView = IigEnum_UiView.ErrorSorry;
            _screenChangeState = ScreenChangeState.ReadyToChangeView;
            Debug.LogError(text);
        }

        public bool IsLoading(ref string state, ref float progress01)
        {
            if (_screenChangeState == ScreenChangeState.LoadingNextView) 
            {
                state = "LoadingNextView";
                progress01 = handle.PercentComplete;
                return true;
            }

            return false;
        }

        AsyncOperationHandle<GameObject> handle;
        private IDisposable _timer; 

        private void LateUpdate()
        {
            switch (_screenChangeState)
            {
                case ScreenChangeState.Standby:

                    CheckStateMachine();

                    if (_targetView != _currentView) 
                    {
                        _screenChangeState = ScreenChangeState.RequestedScreenShot;
                        _currentTransitionType = _targetTransitionType;

                        ScreenBlurController.ProcessCommand processCommand = ScreenBlurController.ProcessCommand.Nothing;

                        processCommand = _currentTransitionType switch
                        {
                            UiTransitionType.WipeAway =>    ScreenBlurController.ProcessCommand.WashAway,
                            UiTransitionType.ZoomOut =>     ScreenBlurController.ProcessCommand.ZoomOut,
                            _ =>                            ScreenBlurController.ProcessCommand.Blur,
                        };

                        TransitionGraphic.SetObscure(onObscured: () => _screenChangeState = ScreenChangeState.ReadyToChangeView, processCommand);

                    }
                    break;
                case ScreenChangeState.RequestedScreenShot: break;
                case ScreenChangeState.ReadyToChangeView:

                    _screenChangeState = ScreenChangeState.LoadingNextView;
                    _currentView = _targetView;

                    DestroyInstance(); 

                    if (_currentView == IigEnum_UiView.None) 
                    {
                        _screenChangeState = ScreenChangeState.ViewIsSetUp;
                        break;
                    }

                    GameObject cached = null;
                    foreach (var c in _cachedViews)
                        if (c.ViewEnum == _currentView && c.Instance)
                            cached = c.Instance;

                    if (cached) 
                    {
                        FinalizeSetup(cached, InstanceSource.Cached);
                    } 
                    else
                    {
                        var reff = _views.GetReference(_currentView);

                        if (reff == null)
                        {
                            ShowError("Reference {0} not found".F(_currentView));
                            _screenChangeState = ScreenChangeState.ViewIsSetUp;
                            return;
                        }

                        _timer = timerGlobal.StartSaveMaxTimer(key: nameof(UiViewService), details: _currentView.ToString()); //GetCollection("{0}".F(nameof(UiViewService))).StartTimer(_currentView.ToString());

                        if (reff.IsReferenceVaid)
                        {
                            handle = reff.Reference.InstantiateAsync(_root);
                            handle.Completed += result =>
                            {
                                if (result.Status == AsyncOperationStatus.Succeeded)
                                {
                                    FinalizeSetup(result.Result, InstanceSource.Addressable);
                                }
                                else
                                {
                                    ShowError("Couldn't load the {0} view".F(_currentView));
                                }
                            };
                        }
                        else
                        {
                            if (!reff.DirectReference)
                            {
                                ShowError("No References for {0} found".F(_currentView));
                                return;
                            }

                            FinalizeSetup(Instantiate(reff.DirectReference, _root) as GameObject, InstanceSource.Resources);
                        }
                    }

                    break;

                case ScreenChangeState.LoadingNextView: break;
                case ScreenChangeState.ViewIsSetUp:

                    _timer.Dispose();

                    if (_loadingScreen)
                        _loadingScreen.SetActive(false);

                    TransitionGraphic.Reveal();
                    _screenChangeState = ScreenChangeState.Standby;

                    break;
            }

            void FinalizeSetup(GameObject instance, InstanceSource source)
            {
                _currentViewIsAddressable = source;
                _screenChangeState = ScreenChangeState.ViewIsSetUp;
                _currentViewInstance = instance;
                _currentViewInstance.SetActive(true);
            }
        }

        #region Inspector
        public override string InspectedCategory => Utils.Singleton.Categories.ROOT;


        private IigEnum_UiView _debugType = IigEnum_UiView.None;
        public override void Inspect()
        {
            base.Inspect();

            "Transition Type".PegiLabel().editEnum(ref _targetTransitionType).nl();

            "Target view".PegiLabel().editEnum(ref _debugType);

            icon.Add.Click(()=> Show(_debugType, clearStack: false, _targetTransitionType));

            icon.Play.Click(() => Show(_debugType, clearStack: true, _targetTransitionType));

            pegi.nl();

            if (_targetView!= IigEnum_UiView.None) 
            {
                icon.Clear.Click(()=> Hide(_targetView));
                _targetView.ToString().PegiLabel().write();
                pegi.nl();
            }

            for (int i=_viewsStack.Count-1; i>=0; i--) 
            {
                var v = _viewsStack[i];
;               if (icon.Close.Click())
                   Hide(v);

                v.ToString().PegiLabel().write();

                pegi.nl();
            }

            if (_views)
            {
                "Enumerated Views ".PegiLabel().write(); 
                _views.ClickHighlight();
                pegi.nl();
            }

            "Screen Change State: {0}".F(_screenChangeState).PegiLabel().nl();

            "Current View".PegiLabel().editEnum(ref _currentView).nl();

            if (_currentViewInstance)
                "Destroy {0}".F(_currentViewInstance.name).PegiLabel().Click(() => Addressables.Release(_currentViewInstance)).nl();

            if (!Application.isPlaying)
            {
                "Cached".PegiLabel().edit_List(_cachedViews);
            }
        }

        public void InspectCurrentView() 
        {
            if (!_currentViewInstance)
                "No views".PegiLabel().nl();
            else
            {
                _currentViewInstance.name.PegiLabel(style: pegi.Styles.ListLabel).nl();
                pegi.Try_Nested_Inspect(_currentViewInstance.GetComponent<IPEGI>());
            }
        }

        #endregion

        protected void Awake()
        {
            if (_loadingScreen)
                _loadingScreen.SetActive(true);

            foreach (var c in _cachedViews)
                if (c.Instance)
                    c.Instance.SetActive(false);
        }

        private void DestroyInstance() 
        {
            if (_currentViewInstance)
            {
                switch (_currentViewIsAddressable) 
                {
                    case InstanceSource.Addressable: Addressables.Release(_currentViewInstance); break;
                    case InstanceSource.Resources: _currentViewInstance.DestroyWhatever(); break;
                    case InstanceSource.Cached: _currentViewInstance.SetActive(false); break;
                }

                _currentViewInstance = null;
            }
        }

        private void CheckStateMachine()
        {
            if (TryEnterIfStateChanged())
            {
                IigEnum_UiView newView = _targetView;
                if (GameStateMachine.TryChangeFallback(ref newView, fallbackValue: IigEnum_UiView.None))
                {
                    if (newView != IigEnum_UiView.None && !_viewsStack.Contains(newView))
                    {
                        Show(newView, clearStack: true, transition: UiTransitionType.Blur);
                    }
                }

                if (GameStateMachine.TryChangeFallback(ref _raycastBlock, fallbackValue: UiRaycastBlock.Off))
                    RaycastBlock = _raycastBlock;
            }
        }

        protected override void OnBeforeOnDisableOrEnterPlayMode()
        {
            DestroyInstance();

            _targetView = IigEnum_UiView.None;
            _currentView = IigEnum_UiView.None;
            _screenChangeState = ScreenChangeState.Standby;
        }

        protected enum ScreenChangeState { Standby, RequestedScreenShot, ReadyToChangeView, LoadingNextView, ViewIsSetUp }
    }

    [PEGI_Inspector_Override(typeof(UiViewService))] internal class UiViewServiceDrawer : PEGI_Inspector_Override { }

    public enum UiTransitionType { Blur, CrossFade, Hexagonal, WipeAway, ZoomOut }

    public enum UiRaycastBlock { Undecided, Off, On }

    public static class UiViewsExtensions 
    {
        public static void Show(this IigEnum_UiView view, bool clearStack, UiTransitionType transition = UiTransitionType.CrossFade)
            => Singleton.Try<UiViewService>(s=> s.Show(view, clearStack, transition));

        public static void Hide(this IigEnum_UiView view, UiTransitionType transition = UiTransitionType.CrossFade)
            => Singleton.Try<UiViewService>(s => s.Hide(view, transition));
    }
}
