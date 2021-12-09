using QuizCanners.Utils;
using UnityEngine;
using UnityEngine.UI;
using QuizCanners.Migration;


namespace QuizCanners.SpecialEffects
{
    [ExecuteAlways]
    public partial class RoundedGraphic : Image, ICfg
    {

        #region Rounded Corners

        [SerializeField] private int[] _roundedCournersPixels = new int[1];
        private float GetCornerFraction(int index) => (1f - ((float)_roundedCournersPixels[index % _roundedCournersPixels.Length]) / MaxCourner);

        public enum Corner
        {
            Down_Left = 0,
            Up_Left = 1,
            Up_Right = 2,
            Down_Right = 3
        }

        private int MaxCourner 
        { 
            get 
            {
                var size = rectTransform.rect.size;

                return Mathf.FloorToInt(Mathf.Min(size.x, size.y) * 0.5f);
            } 
        }

        public bool LinkedCorners
        {
            get { return _roundedCournersPixels.Length == 1; }

            set
            {
                var targetValue = value ? 1 : 4;

                if (targetValue == _roundedCournersPixels.Length) return;

                if (material)
                    material.SetShaderKeyword(UNLINKED_VERTICES, targetValue > 1);

                var tmp = _roundedCournersPixels[0];

                _roundedCournersPixels = new int[targetValue];

                for (var i = 0; i < _roundedCournersPixels.Length; i++)
                    _roundedCournersPixels[i] = tmp;
            }
        }

        #endregion

        #region Screen Position

        public bool feedPositionData = true;

        protected enum PositionDataType
        {
            ScreenPosition,
            AtlasPosition,
            FadeOutPosition
        }

        [SerializeField] protected PositionDataType _positionDataType = PositionDataType.ScreenPosition;

        public float FadeFromX
        {
            set
            {
                var min = fadeOutUvPosition.min;
                if (Mathf.Approximately(min.x, value) == false)
                {
                    min.x = value;
                    fadeOutUvPosition.min = min;
                    SetVerticesDirty();
                }
            }
        }

        public float FadeToX
        {
            set
            {
                var max = fadeOutUvPosition.max;
                if (Mathf.Approximately(max.x, value) == false)
                {
                    max.x = value;
                    fadeOutUvPosition.max = max;
                    SetVerticesDirty();
                }
            }
        }

        public float FadeFromY
        {
            set
            {
                Vector2 min = fadeOutUvPosition.min;
                if (Mathf.Approximately(min.y, value) == false)
                {
                    min.y = value;
                    fadeOutUvPosition.min = min;
                    SetVerticesDirty();
                }
            }
        }

        public float FadeToY
        {
            set
            {
                Vector2 max = fadeOutUvPosition.max;
                if (Mathf.Approximately(max.y, value) == false)
                {
                    max.y = value;
                    fadeOutUvPosition.max = max;
                    SetVerticesDirty();
                }
            }
        }

        [SerializeField] private Rect fadeOutUvPosition = new Rect(0, 0, 1, 1);

        private Rect SpriteRect
        {
            get
            {

                var sp = sprite;

                if (!sp)
                    return Rect.MinMaxRect(0, 0, 100, 100);

                if (!Application.isPlaying)
                    return sp.rect;

                return (sp.packed && sp.packingMode != SpritePackingMode.Tight) ? sp.textureRect : sp.rect;
            }
        }

        #endregion

        #region Populate Mesh

        public const string UNLINKED_VERTICES = "_UNLINKED";
        public const string EDGE_SOFTNESS_FLOAT = "_Edges";

        private bool IsOverlay
        {
            get
            {
                var c = canvas;
                return c && (c.renderMode == RenderMode.ScreenSpaceOverlay || !c.worldCamera);
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {

            if (!gameObject.activeInHierarchy)
            {
                if (!QcDebug.IsRelease && Application.isEditor)
                    Debug.LogError("On populate mesh is called for disabled UI element");

                return;
            }

            var rt = rectTransform;
            var pivot = rt.pivot;
            var rectSize = rt.rect.size;


            vh.Clear();

            var vertex = UIVertex.simpleVert;


            var rctS = rectSize;

            float rectDiff = rctS.x - rctS.y;

            rctS = new Vector2(Mathf.Max(0, rectDiff / rctS.x), Mathf.Max(0, (-rectDiff) / rctS.y));

            var scaleToSided = rctS.x - rctS.y; // If x>0 - positive, else - negative


            if (feedPositionData)
            {

                var pos = Vector2.zero;

                switch (_positionDataType)
                {
                    case PositionDataType.AtlasPosition:

                        var sp = sprite;

                        if (sp)
                        {

                            var tex = sp.texture;

                            if (tex)
                            {

                                var texturePixelSize = new Vector2(tex.width, tex.height);

                                var atlased = SpriteRect;

                                pos = atlased.center / texturePixelSize;

                                vertex.uv3 = atlased.size / texturePixelSize;
                            }
                        }

                        break;
                    case PositionDataType.ScreenPosition:


                        var canvas1 = canvas;
                        pos = RectTransformUtility.WorldToScreenPoint(
                            IsOverlay ? null : (canvas1 ? canvas1.worldCamera : null), rt.position);

                        pos.Scale(new Vector2(1f / Screen.width, 1f / Screen.height));

                        break;

                    case PositionDataType.FadeOutPosition:

                        pos = fadeOutUvPosition.min;

                        vertex.uv3 = fadeOutUvPosition.max;

                        break;
                }

                vertex.uv2 = pos;

            }

            var corner1 = (Vector2.zero - pivot) * rectSize;
            var corner2 = (Vector2.one - pivot) * rectSize;

            vertex.color = color;

            vertex.uv0 = new Vector2(0, 0);
            vertex.uv1 = new Vector2(scaleToSided, GetCornerFraction(0));
            vertex.position = new Vector2(corner1.x, corner1.y);
            vh.AddFull(vertex);

            vertex.uv0 = new Vector2(0, 1);
            vertex.uv1.y = GetCornerFraction(1);
            vertex.position = new Vector2(corner1.x, corner2.y);
            vh.AddFull(vertex);

            vertex.uv0 = new Vector2(1, 1);
            vertex.uv1.y = GetCornerFraction(2);
            vertex.position = new Vector2(corner2.x, corner2.y);
            vh.AddFull(vertex);

            vertex.uv0 = new Vector2(1, 0);
            vertex.uv1.y = GetCornerFraction(3);
            vertex.position = new Vector2(corner2.x, corner1.y);
            vh.AddFull(vertex);

            if (LinkedCorners)
            {

                //1  2
                //0  3
                vh.AddTriangle(0, 1, 2);
                vh.AddTriangle(2, 3, 0);
            }
            else
            {
                //1    6,9    2
                //7  13  14   8
                //4  12  15   11
                //0    5,10   3

                // TODO: Implement atlasing for Unlinked

                var cornMid = (corner1 + corner2) * 0.5f;

                vertex.uv1.y = GetCornerFraction(0);
                vh.AddFull(vertex.Set(0, 0.5f, corner1, cornMid)); //4
                vh.AddFull(vertex.Set(0.5f, 0, cornMid, corner1)); //5

                vertex.uv1.y = GetCornerFraction(1);
                vh.AddFull(vertex.Set(0.5f, 1, cornMid, corner2)); //6
                vh.AddFull(vertex.Set(0, 0.5f, corner1, cornMid)); //7

                vertex.uv1.y = GetCornerFraction(2);
                vh.AddFull(vertex.Set(1, 0.5f, corner2, cornMid)); //8
                vh.AddFull(vertex.Set(0.5f, 1, cornMid, corner2)); //9

                vertex.uv1.y = GetCornerFraction(3);
                vh.AddFull(vertex.Set(0.5f, 0, cornMid, corner1)); //10
                vh.AddFull(vertex.Set(1, 0.5f, corner2, cornMid)); //11

                vertex.uv1.y = GetCornerFraction(0);
                vh.AddFull(vertex.Set(0.5f, 0.5f, cornMid, cornMid)); //12

                vertex.uv1.y = GetCornerFraction(1);
                vh.AddFull(vertex.Set(0.5f, 0.5f, cornMid, cornMid)); //13

                vertex.uv1.y = GetCornerFraction(2);
                vh.AddFull(vertex.Set(0.5f, 0.5f, cornMid, cornMid)); //14

                vertex.uv1.y = GetCornerFraction(3);
                vh.AddFull(vertex.Set(0.5f, 0.5f, cornMid, cornMid)); //15

                vh.AddTriangle(0, 4, 5);
                vh.AddTriangle(1, 6, 7);
                vh.AddTriangle(2, 8, 9);
                vh.AddTriangle(3, 10, 11);

                vh.AddTriangle(12, 5, 4);
                vh.AddTriangle(13, 7, 6);
                vh.AddTriangle(14, 9, 8);
                vh.AddTriangle(15, 11, 10);

            }
        }

        #endregion

        #region Updates
        private Vector3 _previousPos = Vector3.zero;
        private Transform _previousParent;

        private void Update()
        {
            var needsUpdate = false;

            if (transform.parent != _previousParent || (feedPositionData && rectTransform.position != _previousPos))
                needsUpdate = true;
            
            if (needsUpdate)
            {
                SetAllDirty();
                _previousPos = rectTransform.position;
                _previousParent = transform.parent;
            }
        }
        #endregion

    }
}