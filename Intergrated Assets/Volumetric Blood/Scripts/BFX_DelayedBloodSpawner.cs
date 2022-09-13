using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public class BFX_DelayedBloodSpawner : MonoBehaviour
    {
        private Gate.UnityTimeScaled _bloodSpawnDelay = new Gate.UnityTimeScaled();

        int _bloodSpawns = 2;

        public static BFX_DelayedBloodSpawner CreateOnHit(RaycastHit hit, Vector3 direction) 
        {
            var root = new GameObject();

            var cmp = root.AddComponent<BFX_DelayedBloodSpawner>();

            var tf = root.transform;

            tf.parent = hit.collider.transform;

            tf.position = hit.point;

            tf.rotation = Quaternion.LookRotation(direction);

            return cmp;
        }

        private void OnDisable()
        {
            gameObject.DestroyWhatever();
        }

        void Update()
        {
            if (_bloodSpawnDelay.TryUpdateIfTimePassed(0.15f)) 
            {
                Singleton.Try<Pool_VolumetricBlood>(s => 
                {
                    s.TrySpawnRandom(transform.position, transform.forward, out var instance);
                });

                _bloodSpawns--;

                if (_bloodSpawns<=0)
                    gameObject.DestroyWhatever();
            }
        }
    }
}
