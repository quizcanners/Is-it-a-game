using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

namespace QuizCanners.IsItGame.Develop
{
    public class Pool_VolumetricBlood : PoolSingletonBase<BFX_BloodController>
    {
        public bool SpawnDecalOnTarget;
        public GameObject TargetDecalPrefab;

        public bool TrySpawnRandom(Vector3 position, Vector3 direction, out BFX_BloodController instance, float size = 2) 
        {
            if (!TrySpawnIfVisible(position, out instance))
                return false;

            float angle = Mathf.Atan2(-direction.x, -direction.z) * Mathf.Rad2Deg + 180;

            var tf = instance.transform;

            tf.rotation = Quaternion.Euler(0, angle + 90 + 180 * (UnityEngine.Random.value * 2 - 1), 0);

            float distance = QcMath.SmoothStep(0, 30, GetDistanceToCamera(position));

            float distanceScale = (1f + distance) * 0.5f;

            size *= distanceScale;

            tf.localScale = distanceScale * size * Vector3.one;


            instance.AnimationSpeed = Mathf.Clamp(1f / size, min: 1f, max: 1.5f);

            return true;
        }

        public bool TrySpawnFromHit(RaycastHit hit, Vector3 direction,  out BFX_BloodController instance, float size = 2, bool impactDecal = true) 
        {
            if (!TrySpawnIfVisible(hit.point, out instance))
                return false;

             float angle = Mathf.Atan2(-direction.x, -direction.z) * Mathf.Rad2Deg + 180;

            var tf = instance.transform;

            tf.rotation = Quaternion.Euler(0, angle + 90 + 45 * (UnityEngine.Random.value * 2 - 1), 0);

            float distance = QcMath.SmoothStep(0, 30, GetDistanceToCamera(hit.point));

            float distanceScale = (1f + distance ) * 0.5f;

            size *= distanceScale;

            tf.localScale = distanceScale * size * Vector3.one;


            instance.AnimationSpeed = Mathf.Clamp(1f / size, min: 1f, max: 1.5f);

            //var settings = instance.GetComponent<BFX_BloodController>();
            //settings.LightIntensityMultiplier = DirLight.intensity;

            //settings.FreezeDecalDisappearance = InfiniteDecal;


            if (SpawnDecalOnTarget && impactDecal && distance<5)
            {
                //var nearestBone = GetNearestObject(hit.transform.root, hit.point);
                var nearestBone = hit.collider.transform;

                if (nearestBone)
                {
                    GameObject attachBloodInstance = Instantiate(TargetDecalPrefab);
                    var bloodT = attachBloodInstance.transform;
                    bloodT.position = hit.point;
                    bloodT.localRotation = Quaternion.identity;
                    bloodT.localScale = Vector3.one * UnityEngine.Random.Range(0.75f, 1.2f);
                    bloodT.LookAt(hit.point + hit.normal, Vector3.up);
                    bloodT.Rotate(90, 0, 0);
                    bloodT.parent = nearestBone;
                }
            }

            return true;
        }

        /*
        Transform GetNearestObject(Transform hit, Vector3 hitPos)
        {
            var closestPos = 100f;
            Transform closestBone = null;
            var childs = hit.GetComponentsInChildren<Transform>();

            foreach (var child in childs)
            {
                var dist = Vector3.Distance(child.position, hitPos);
                if (dist < closestPos)
                {
                    closestPos = dist;
                    closestBone = child;
                }
            }

            var distRoot = Vector3.Distance(hit.position, hitPos);
            if (distRoot < closestPos)
            {
                closestPos = distRoot;
                closestBone = hit;
            }
            return closestBone;
        }*/

    }

    [PEGI_Inspector_Override(typeof(Pool_VolumetricBlood))] internal class Pool_VolumetricBloodDrawer : PEGI_Inspector_Override { }
}