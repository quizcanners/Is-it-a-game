using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.SpecialEffects
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public class EffectsRenderingInEditor : MonoBehaviour
    {
        #if UNITY_EDITOR
        private ShaderProperty.Feature _isInEditor = new ShaderProperty.Feature("_qc_SCENE_RENDERING");

        void OnPreRender() => _isInEditor.Enabled = false;
        void OnPostRender() => _isInEditor.Enabled = true;
        #endif
    }
}
