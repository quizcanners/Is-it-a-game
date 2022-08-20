using QuizCanners.Utils;
using UnityEngine;
namespace QuizCanners.IsItGame
{
    public class BFX_BloodDecalController : MonoBehaviour
    {
        [SerializeField] private BFX_ShaderProperies _properties;

        private void LateUpdate()
        {
            if (!_properties.IsVisible)
                gameObject.DestroyWhatever();
        }


        private void OnDisable()
        {
            gameObject.DestroyWhatever();
        }

        private void Reset()
        {
            _properties = GetComponentInChildren<BFX_ShaderProperies>();

        }
    }
}