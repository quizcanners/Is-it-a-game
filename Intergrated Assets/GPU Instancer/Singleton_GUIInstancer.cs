using GPUInstancer;
using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;


//https://wiki.gurbu.com/index.php?title=GPU_Instancer:BestPractices
// Use this for smoke

namespace QuizCanners.IsItGame
{
    [ExecuteAlways]
    public class Singleton_GUIInstancer : Singleton.BehaniourBase
    {
        public GPUInstancerPrefabManager prefabManager;
        public GPUInstancerPrefabPrototype prefab;
        public int bufferSize = 10000;
        public int count = 0;

        private bool _initialized;
        private Matrix4x4[] _matrix4x4Array;

        private void Initialize() 
        {
            if (_initialized)
                return;

            _initialized = true;

            _matrix4x4Array = new Matrix4x4[bufferSize];

            for (int i = 0; i < _matrix4x4Array.Length; i++)
                _matrix4x4Array[i] = Matrix4x4.zero; // Matrix4x4.TRS(Random.insideUnitSphere * 40, Quaternion.identity, Vector3.one);

            GPUInstancerAPI.InitializeWithMatrix4x4Array(prefabManager, prefab, _matrix4x4Array);
        }


        public override void Inspect()
        {
          //  base.Inspect();

            pegi.Nl();

            if (Application.isPlaying == false) 
            {
                pegi.TryDefaultInspect(this);
                return;
            }

            if (!_initialized && "Initialize".PegiLabel().Click())
                Initialize();

            if (_initialized)
            {
                if ("Add".PegiLabel().Click().Nl())
                {
                    count++;
                    _matrix4x4Array[count] = Matrix4x4.TRS((Random.insideUnitSphere * 15f).AbsY(), Random.rotation, Vector3.one * (5f + Random.value * 5f));
                    GPUInstancerAPI.UpdateVisibilityBufferWithMatrix4x4Array(prefabManager, prefab, _matrix4x4Array, arrayStartIndex: 0, bufferStartIndex: 0, count);
                }

                if ("Shuffle".PegiLabel().Click().Nl())
                {
                    for (int i = 0; i < _matrix4x4Array.Length; i++)
                        _matrix4x4Array[i] = Matrix4x4.TRS(Random.insideUnitSphere * 550, Quaternion.identity, Vector3.one);
                    GPUInstancerAPI.UpdateVisibilityBufferWithMatrix4x4Array(prefabManager, prefab, _matrix4x4Array);
                    // GPUInstancerAPI.Up
                }
            }

        }
    }

    [PEGI_Inspector_Override(typeof(Singleton_GUIInstancer))] internal class Singleton_GUIInstancerDrawer : PEGI_Inspector_Override { }
}
