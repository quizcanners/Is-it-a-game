using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{

    public class UVTextureAnimator : MonoBehaviour, IPEGI, IPEGI_ListInspect, INeedAttention
    {
        [SerializeField] private MeshRenderer currentRenderer;
        [SerializeField] private int Rows = 4; //tilesY
        [SerializeField] private int Columns = 4; //tilesX
        [SerializeField] private float Speed = 1;

        [NonSerialized] public float Progress01;

        private bool isInitialized;
       
        private bool _isAnimating;
        private Vector2 size;
      
        private MaterialInstancer.ForMeshRenderer _matInstancer;
        private readonly Gate.Frame _frameGate = new();

        private static readonly ShaderProperty.VectorValue TEX_NEXT_FRAME = new("_SpriteSegment");
        private static readonly ShaderProperty.VectorValue INTERPOLATION = new("InterpolationValue");

        public bool IsDone => _isAnimating && Progress01 > 0.999f;
        private int TotalFrames => Columns * Rows;

        public void ResetAnimation()
        {
            _isAnimating = false;
            Progress01 = 0;
            UpdateInternal();
        }

        public void Play()
        {
            _isAnimating = true;
            Progress01 = 0;
            UpdateInternal();
        }

        private Material GetMaterial() 
        {
            if (_matInstancer == null)
                _matInstancer = new MaterialInstancer.ForMeshRenderer(currentRenderer);

            return _matInstancer.MaterialInstance;
        }

        void LateUpdate()
        {
            if (_frameGate.TryEnter())
            {
                UpdateInternal();
            }
        }

        private void UpdateInternal()
        {
            _frameGate.TryEnter();

            if (_isAnimating)
            {
                Progress01 = Mathf.Min(Progress01 + Time.deltaTime * Speed, 1);
            }

            float frame = Progress01 * 0.9999f * Columns * Rows;

            var currentIndex = Mathf.FloorToInt(frame);

            var nextIndex = currentIndex + 1;

            if (nextIndex == TotalFrames)
                nextIndex = currentIndex;

   
            Vector2 IndexToUv(int index) 
            {
                var uIndex = index % Columns;
                var vIndex = index / Columns;

                return new(uIndex * size.x, (1.0f - size.y) - vIndex * size.y);

            }

            var currentOffset = IndexToUv(currentIndex);
            var nextOffset = IndexToUv(nextIndex);

            Material m = GetMaterial();

            m.Set(TEX_NEXT_FRAME,
                new Vector4(size.x, size.y,
                z: currentOffset.x,
                w: currentOffset.y));

            m.Set(INTERPOLATION, new Vector4(Mathf.Clamp01(frame - currentIndex), 0, nextOffset.x, nextOffset.y));
        }

        #region Inspector
        public void Inspect()
        {
            var changes = pegi.ChangeTrackStart();

            pegi.Nl();

            "Renderer".PegiLabel(90).Edit_IfNull(ref currentRenderer, gameObject).Nl();

            "Progress".PegiLabel(90).Edit_01(ref Progress01).Nl();

            "Speed".PegiLabel(90).Edit(ref Speed).Nl();

            if (_matInstancer != null && "Delete Material Instance".PegiLabel().Click().Nl())
                _matInstancer = null;

            _matInstancer.Nested_Inspect().Nl();

            if (changes)
                UpdateInternal();
        }

        public void AutoAssignMaterial() 
        {
            if (!currentRenderer)
                currentRenderer = GetComponent<MeshRenderer>();

            Singleton.Try<Pool_BloodSquirts>(s =>
            {
                if (s.TryGetReplacementMaterial(GetMaterial(), out var newMat))
                    currentRenderer.sharedMaterial = newMat;
            });

            this.SetToDirty();
        }

        public void InspectInList(ref int edited, int index)
        {
            "Renderer".PegiLabel(90).Edit_IfNull(ref currentRenderer, gameObject).Nl();

            if (currentRenderer)
                Singleton.Try<Pool_BloodSquirts>(s => 
                {
                    if (s.TryGetReplacementMaterial(GetMaterial(), out var newMat) && "Update Material".PegiLabel().Click())
                        currentRenderer.sharedMaterial = newMat;
                });

            name.PegiLabel(0.33f).Write();
            pegi.Edit_01(ref Progress01);


            if (!_isAnimating)
                Icon.Play.Click(Play);
            else if (Icon.Pause.Click())
            {
                _isAnimating = false;
                Progress01 = 0;
            }
                

            if (Icon.Enter.Click())
                edited = index;


            pegi.ClickHighlight(this);
        }

        #endregion

        private void Reset()
        {
            currentRenderer = GetComponent<MeshRenderer>();
        }

        private void OnEnable()
        {
            if (!isInitialized)
            {
                isInitialized = true;

                if (!currentRenderer)
                    currentRenderer = GetComponent<MeshRenderer>();

                size = new Vector2(1f / Columns, 1f / Rows);
            }
        }

        public string NeedAttention()
        {
            

            if (!GetMaterial())
                return "No Material";

            return null;
        }
    }

    [PEGI_Inspector_Override(typeof(UVTextureAnimator))] internal class UVTextureAnimatorDrawer : PEGI_Inspector_Override { }
}