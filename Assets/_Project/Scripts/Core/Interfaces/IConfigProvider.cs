using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Project.Scripts.Core.Interfaces
{
    public interface IConfigProvider
    {
        T GetConfig<T>(string key) where T : ScriptableObject; //sync
        UniTask<T> GetConfigAsync<T>(string key) where T : ScriptableObject; //async
    }
}