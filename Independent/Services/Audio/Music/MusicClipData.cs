using System.Collections;
using UnityEngine;
using QuizCanners.Inspect;
using UnityEngine.AddressableAssets;
using System;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace QuizCanners.IsItGame
{
    [CreateAssetMenu(fileName = FILE_NAME, menuName = "Quiz Canners/" + GameController.PROJECT_NAME + "/Managers/Audio/" + FILE_NAME)]
    public class MusicClipData : ScriptableObject, IPEGI_ListInspect
    {
        public const string FILE_NAME = "Song";

        public AssetReference Reference;
        public bool AlwaysStartFromBeginning;
        public float Volume = 1;

        [NonSerialized] private AsyncOperationHandle<AudioClip> _handle;
        [NonSerialized] private AudioClip clip;

        public AudioClip TryGetClip() => clip;

        public void Release() => Addressables.Release(_handle);

        public IEnumerator GetClipAsync(Action<AudioClip> onComplete = null)
        {
            if (Reference == null)
            {
                onComplete?.Invoke(clip);
                yield break;
            }

            if (_handle.IsValid())
            {
                yield return _handle;
                onComplete?.Invoke(_handle.Result);
                yield break;
            }

            try
            {
                _handle = Reference.LoadAssetAsync<AudioClip>();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            _handle.Completed += action =>
            {
                if (action.IsDone)
                {
                    clip = action.Result;
                    onComplete?.Invoke(action.Result);
                }
            };

            yield return _handle;
        }

        #region Inspector
        public void InspectInList(ref int edited, int ind)
        {

            var enm = (IigEnum_Music)ind;
            enm.ToString().PegiLabel().write();

            if (icon.Play.Click())
                enm.Play();

        }
        #endregion
    }

}