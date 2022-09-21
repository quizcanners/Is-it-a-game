using QuizCanners.Inspect;
using QuizCanners.Lerp;
using QuizCanners.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public class C_BloodSquirt : MonoBehaviour, IPEGI
    {
        [SerializeField] private List<UVTextureAnimator> _animators = new List<UVTextureAnimator>();

        private bool _isAnimating;
        private float _targetScale = 1;

        float Scale 
        {
            get => transform.localScale.x;
            set 
            {
                transform.localScale = Vector3.one * value;
            }
        }

        void Update() 
        {
            Scale = LerpUtils.LerpBySpeed(Scale, _targetScale, 10, unscaledTime: false);

            if (_isAnimating)
            {
                foreach (var a in _animators)
                {
                    if (a.IsDone == false)
                        return;
                }

                Pool.Return(this);
            }
        }

        void OnDisable() 
        {
            _isAnimating = false;
        }

        public void ResetAnimation() 
        {
            foreach (var a in _animators)
                a.ResetAnimation();
        }

        public void Play(float scale = 1)
        {
            gameObject.SetActive(true);
            Scale = 0.1f;

            _targetScale = scale;

            foreach (var a in _animators)
            {
                if (!a)
                    Debug.LogError("Unassigned element on", this);
                else 
                    a.Play();
            }

            _isAnimating = true;
        }


        #region Inspector

        private readonly pegi.CollectionInspectorMeta collectionInspectorMeta = new("Elements");

        private float _masterProgress;

        public void Inspect()
        {
            if (collectionInspectorMeta.IsAnyEntered == false)
            {

                if (Application.isPlaying)
                {
                    if ("Animating".PegiLabel().ToggleIcon(ref _isAnimating).Nl())
                    {
                        if (_isAnimating)
                            Play();
                        else
                            ResetAnimation();
                    }

                    if ("Animation".PegiLabel().Edit_01(ref _masterProgress).Nl())
                    {
                        foreach (UVTextureAnimator a in _animators)
                            a.Progress01 = _masterProgress;
                    }

                }
                else
                {
                    if ("Find All".PegiLabel().Click())
                    {
                        _animators.Clear();
                        _animators = GetComponentsInChildren<UVTextureAnimator>().ToList();
                    }

                    if ("Auto Prepare All Materials".PegiLabel().Click())
                    {
                        foreach (var a in _animators)
                            a.AutoAssignMaterial();
                    }

                    pegi.Nl();

                    var ch = transform.GetChild(0);
                    if (ch)
                    {
                       // if (ch.localRotation.eulerAngles.z != 0 && "Fix Rotation".PegiLabel().Click().Nl())
                         //   ch.localRotation = Quaternion.Euler(ch.localRotation.eulerAngles.Z(0));

                        if ("Fix Rotation".PegiLabel().Click().Nl())
                            ch.localRotation = Quaternion.Euler(new Vector3(-90, -90, 0));
                    }
                }

            

            }

            collectionInspectorMeta.Edit_List(_animators).Nl();

           

        }
        #endregion
    }

   
    [PEGI_Inspector_Override(typeof(C_BloodSquirt))] internal class C_BloodSquirtDrawer : PEGI_Inspector_Override { }
}