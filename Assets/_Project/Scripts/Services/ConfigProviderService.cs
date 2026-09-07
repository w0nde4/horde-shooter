using _Project.Scripts.Core.Interfaces;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace _Project.Scripts.Services
{
    public class ConfigProviderService : IConfigProvider
    {
        public T GetConfig<T>(string key) where T : ScriptableObject
        {
            var handle = Addressables.LoadAssetAsync<T>(key);
            handle.WaitForCompletion();
            return handle.Result;
        }

        public async UniTask<T> GetConfigAsync<T>(string key) where T : ScriptableObject
        {
            var handle = Addressables.LoadAssetAsync<T>(key);
            await handle.ToUniTask();
            return handle.Result;
        }
    }
}