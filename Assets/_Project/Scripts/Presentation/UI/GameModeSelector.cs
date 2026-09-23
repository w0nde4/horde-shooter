using _Project.Scripts.Services.Network;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Presentation.UI
{
    public class GameModeSelector : MonoBehaviour
    {
        [SerializeField] private string sessionName = "MyRoom";
        [SerializeField] private int maxPlayers = 4;
        
        [Inject] private FusionNetworkService _networkService;

        public void OnSinglePlayer() => _networkService.StartSingleplayerGameAsync().Forget();
        public void OnHost() => _networkService.StartMultiplayerHostAsync(sessionName, maxPlayers).Forget();
        public void OnJoin() => _networkService.JoinMultiplayerAsync(sessionName).Forget();
    }
}