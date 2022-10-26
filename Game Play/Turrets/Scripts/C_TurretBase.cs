using Cinemachine.Utility;
using QuizCanners.Inspect;
using QuizCanners.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public class C_TurretBase : MonoBehaviour, IPEGI, IPEGI_Handles
    {
        const float CELL_SIZE = 3;

        internal static List<C_TurretBase> allInstances = new();
        internal static C_TurretBase pointedBase;
        internal static Gate.Frame _pointedSearch = new();

        [SerializeField] private MeshRenderer _boxRenderer;
        [SerializeField] private Material _unselected, _pointed, _selected;


        private Vector3 GetBoxCenter() => transform.position + Vector3.up;
        private Vector3 GetBoxSize() => Vector3.one * CELL_SIZE;


        protected void OnEnable()
        {
            allInstances.Add(this);
        }

        protected void OnDisable()
        {
            allInstances.Remove(this);
        }

        void CenterToCell() 
        {
            var cellPosition = transform.localPosition / CELL_SIZE;

            var positioveOffset = Vector3.one * 1000;

            cellPosition += positioveOffset;

            cellPosition = cellPosition.Round();

            transform.localPosition = (cellPosition - positioveOffset) * CELL_SIZE;
        }

        void OnDrawGizmos()
        {
            this.OnSceneDraw();
        }

        public void Update()
        {
            if (_pointedSearch.TryEnter()) 
            {
                float _minDistance = float.MaxValue;

                pointedBase = null;

                foreach (var b in allInstances) 
                {
                    if (TryPoint(out var dist) && dist < _minDistance) 
                    {
                        _minDistance = dist;
                        pointedBase = b;
                    }
                }
            }

            _boxRenderer.material = pointedBase ? (pointedBase == this ? _pointed : _unselected) : _unselected;
        }

        private bool TryPoint(out float dist) 
        {
            if (Camera.main.GetMousePosRay().TryHit(new QcMath.Primitive.Box(GetBoxCenter(), boxSize: GetBoxSize()), out var hit))
            {
                dist = hit.distance;
                return true;
            }
            else
            {
                dist = float.MaxValue;
                return false;
            }
        }

        #region Inspector

        public void OnSceneDraw()
        {
            if (pointedBase && pointedBase == this) 
            {
                pegi.Gizmo.DrawWireCube(GetBoxCenter(), GetBoxSize());
            }
        }

        public void Inspect()
        {
            if ("Center To Cell".PegiLabel().Click().Nl())
                CenterToCell();
        }

        #endregion
    }

    [PEGI_Inspector_Override(typeof(C_TurretBase))] internal class C_TurretBaseDrawer : PEGI_Inspector_Override { }
}
