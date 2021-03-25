using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace QuizCanners.IsItGame
{
    public abstract class AddressableBase 
    {
        public string Name;
        [SerializeField] public Object DirectReference;

        protected abstract AssetReference GetReference();

        protected abstract AsyncOperationHandle GetHandle();

        protected abstract void StartLoad();

        protected Object Result => DirectReference ? DirectReference : (Object)(GetHandle().IsValid() ? GetHandle().Result : null);
        public void Release()
        {
            var h = GetHandle();
            if (h.IsValid())
            {
                Addressables.Release(h);
            }
        }

        protected async Task GetAsync_Internal<T>(Action<T> onComplete = null) where T : Object
        {
            if (DirectReference)
            {
                onComplete?.Invoke((T)DirectReference);
                return;
            }

            if (GetReference() == null)
            {
                onComplete?.Invoke((T)Result);
                return;
            }
            var h = GetHandle();
            if (h.IsValid())
            {
                await h.Task;
                onComplete?.Invoke((T)Result);
                return;
            }
            
            try
            {
                StartLoad();

                GetHandle().Completed += action =>
                {
                    if (action.IsDone)
                    {
                        var tmp = onComplete;
                        onComplete = null;

                        tmp?.Invoke((T)Result);
                        
                    }
                };
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                onComplete?.Invoke((T)Result);
            }

            await GetHandle().Task;
        }
    }

    [Serializable]
    public class AddressableReff : AddressableBase
    {
        public AssetReference Reference;

        [NonSerialized] protected AsyncOperationHandle _handle;
        protected override AsyncOperationHandle GetHandle() => _handle;
        public Task GetAsync<T>(Action<T> onComplete = null) where T : Object => GetAsync_Internal(onComplete);

        protected override void StartLoad() 
        {
            _handle = GetReference().LoadAssetAsync<Object>();
        }

        protected override AssetReference GetReference() => Reference;
    }

    [Serializable]
    public abstract class InspectableAddressableGeneric<T> : AddressableBase where T: Object
    {
        public AssetReferenceT<T> Reference;

        [NonSerialized] protected AsyncOperationHandle<T> _handle;
        protected override AsyncOperationHandle GetHandle() => _handle;
        public Task GetAsync(Action<T> onComplete = null) => GetAsync_Internal(onComplete);

        protected override void StartLoad() => _handle = GetReference().LoadAssetAsync<T>();

        protected override AssetReference GetReference() => Reference;
    }

    [Serializable]
    public class Addressable_Sprite : AddressableBase
    {
        public AssetReferenceSprite Reference;

        [NonSerialized] protected AsyncOperationHandle<Sprite> _handle;
        protected override AsyncOperationHandle GetHandle() => _handle;
        public Task GetAsync(Action<Sprite> onComplete = null) => GetAsync_Internal(onComplete);

        protected override void StartLoad() => _handle = GetReference().LoadAssetAsync<Sprite>();

        protected override AssetReference GetReference() => Reference;
    }

    [Serializable]
    public class Addressable_GameObject : AddressableBase
    {
        public AssetReferenceGameObject Reference;

        [NonSerialized] protected AsyncOperationHandle<GameObject> _handle;
        protected override AsyncOperationHandle GetHandle() => _handle;
        public Task GetAsync(Action<GameObject> onComplete = null) => GetAsync_Internal(onComplete);

        protected override void StartLoad() => _handle = GetReference().LoadAssetAsync<GameObject>();

        protected override AssetReference GetReference() => Reference;
    }

    [Serializable] public class Addressable_AudioClip : InspectableAddressableGeneric<AudioClip> { }
    [Serializable] public class Addressable_Texture2D : InspectableAddressableGeneric<Texture2D> { }
    [Serializable] public class Addressable_ScriptableObject : InspectableAddressableGeneric<ScriptableObject> { }
}
