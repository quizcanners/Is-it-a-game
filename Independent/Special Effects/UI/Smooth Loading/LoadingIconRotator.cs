using System.Collections.Generic;
using QuizCanners.Lerp;
using QuizCanners.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace QuizCanners.SpecialEffects
{
    public class LoadingIconRotator : MonoBehaviour
    {
        public float rotationSpeed = 10f;

        private float rotation;

        public Color frontColor = Color.white;
        public Color backColor = Color.black;

        public List<Graphic> loadingCircles = new List<Graphic>();

        public Graphic shadow;

        private float transparency = 0.1f;

        private float soundTimer;

        private bool isFadingOut;

        private void Update()
        {
            float cnt = loadingCircles.Count;

            if (cnt > 1)
            {

                var changing = LerpUtils.IsLerpingBySpeed(ref transparency, isFadingOut ? 0f : 1f, isFadingOut ? 8f : 2f);

                if (changing || transparency > 0)
                {
                    soundTimer -= Time.deltaTime;

                    if (soundTimer < 0)
                    {
                        //SoundEffects..Play();
                        soundTimer += 0.3f;
                    }

                    rotation += Time.deltaTime * rotationSpeed;
                    rotation = rotation % loadingCircles.Count;

                    float minPortion = 1 / cnt;

                    for (int i = 0; i < cnt; i++)
                    {
                        float distance = 1f - (rotation - i + cnt) % cnt / cnt;

                        float front = (distance + Mathf.Max(0, minPortion - distance) * cnt);

                        front *= front;

                        loadingCircles[i].color = (frontColor * front + backColor * (1 - front)).Alpha(transparency);
                    }

                    shadow.TrySetAlpha(transparency);
                }
            }

            /*  if (rTransform) {
                  if (rotation >= 1) {
                      rotation -= 1f;
                      rotationDegrees = Mathf.LerpAngle(rotationDegrees, rotationDegrees + angle, 1);

                      var rot = rTransform.localRotation.eulerAngles;

                      rot.z = rotationDegrees;

                      rTransform.localRotation = Quaternion.Euler(rot);

                  }
              }*/

        }

        private void MakeTransparent()
        {
            foreach (var circle in loadingCircles)
            {
                if (circle)
                {
                    circle.color = Color.clear;
                }
            }

            if (shadow)
            {
                shadow.TrySetAlpha(0);
            }

            transparency = 0;
        }

        private void OnEnable()
        {
            isFadingOut = false;
            MakeTransparent();
        }

        private void OnDisable() => MakeTransparent();



        public void FadeAway() => isFadingOut = true;
    }
}