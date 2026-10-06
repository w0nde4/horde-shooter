using _Project.Scripts.Services.Network;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace _Project.Scripts.Presentation.UI
{
    public class GameModeSelector : MonoBehaviour
    {
        [Header("Room Panel Window")]
        [SerializeField] private GameObject roomSettingsSubPanel;
        [SerializeField] private Button proceedButton;

        [Header("UI Inputs")]
        [SerializeField] private TMP_InputField roomNameInputField;
        [SerializeField] private TMP_Dropdown maxPlayersDropdown;

        [Inject] private FusionNetworkService _networkService;

        private enum SelectedMode { None, Host, Join }
        private SelectedMode _currentMode = SelectedMode.None;

        private void Awake()
        {
            if (roomSettingsSubPanel != null) roomSettingsSubPanel.SetActive(false);

            if (proceedButton != null) proceedButton.onClick.AddListener(OnProceedClicked);
        }

        public void OnSinglePlayer() 
        {
            _networkService.StartSingleplayerGameAsync().Forget();
        }

        public void OnHostSelected() 
        {
            _currentMode = SelectedMode.Host;
            OpenRoomSettings(showMaxPlayers: true);
        }

        public void OnJoinSelected() 
        {
            _currentMode = SelectedMode.Join;
            OpenRoomSettings(showMaxPlayers: false);
        }

        private void OpenRoomSettings(bool showMaxPlayers)
        {
            if (roomSettingsSubPanel == null) return;

            roomSettingsSubPanel.SetActive(true);
            roomNameInputField.text = string.Empty;

            if (maxPlayersDropdown != null)
            {
                maxPlayersDropdown.gameObject.SetActive(showMaxPlayers);
            }
        }

        private void OnProceedClicked()
        {
            if (string.IsNullOrWhiteSpace(roomNameInputField.text))
            {
                Debug.LogWarning("Room name cannot be empty!");
                return;
            }

            var roomName = roomNameInputField.text.Trim();

            if (_currentMode == SelectedMode.Host)
            {
                var maxPlayers = 4;
                var selectedText = maxPlayersDropdown.options[maxPlayersDropdown.value].text;
                
                if (int.TryParse(selectedText, out var parsedPlayers))
                {
                    maxPlayers = parsedPlayers;
                }

                _networkService.StartMultiplayerHostAsync(roomName, maxPlayers).Forget();
            }
            else if (_currentMode == SelectedMode.Join)
            {
                _networkService.JoinMultiplayerAsync(roomName).Forget();
            }

            roomSettingsSubPanel.SetActive(false);
        }
    }
}