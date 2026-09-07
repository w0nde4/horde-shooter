using System;
using _Project.Scripts.Services;
using _Project.Scripts.Services.Network;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace _Project.Scripts.Installers.Bootstrap
{
    public class BootstrapLoader : IInitializable
    {
        private readonly FusionNetworkService _networkService;
        private readonly bool _useNetwork;

        [Inject]
        public BootstrapLoader(FusionNetworkService networkService)
        {
            _networkService = networkService;
            // hardcoded set for now, later set in configs
            _useNetwork = true;
        }
        public async void Initialize()
        {
            try
            {
                if (_useNetwork)
                {
                    await _networkService.StartGameAsync(GetGameModeFromArgs(), "TestSession", true);
                }
                //await SceneManager.LoadSceneAsync("Main").ToUniTask();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load scene: {e.Message}");
            }
        }
        
        private GameMode GetGameModeFromArgs()
        {
            var args = System.Environment.GetCommandLineArgs();
            foreach (var arg in args)
            {
                if (arg.Equals("-host", StringComparison.OrdinalIgnoreCase))
                {
                    return GameMode.Host;
                }
                else if (arg.Equals("-client", StringComparison.OrdinalIgnoreCase))
                {
                    return GameMode.Client;
                }
            }

            return GameMode.AutoHostOrClient;
        }
    }
}