using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    [DisallowMultipleComponent]
    public class C_AttachmentPositionProxy : MonoBehaviour, IPEGI
    {
        public static C_AttachmentPositionProxy AttachToRayhitPoint(RaycastHit hit, string withName = "ON RAYCAST ATTACHMENT") 
        {
            return AttachTo(hit.transform, hit.point, Quaternion.LookRotation(-hit.normal), withName);
        }

        public static C_AttachmentPositionProxy AttachTo(Transform target, Vector3 position, Quaternion rotation, string withName = "ATTACHMENT") 
        {
            var tf = new GameObject(withName).transform;
            tf.parent = target;
            tf.position = position;
            tf.rotation = rotation;

            return tf.gameObject.AddComponent<C_AttachmentPositionProxy>();
        }

        void OnDisable() 
        {
            gameObject.DestroyWhatever();
        }

        public void Inspect()
        {
            "Use this component to create attachments. Will destroy itself on disable.".PegiLabel().Write_Hint();
        }
    }

    [PEGI_Inspector_Override(typeof(C_AttachmentPositionProxy))] internal class C_BoltImpactPositionDrawer : PEGI_Inspector_Override { }
}