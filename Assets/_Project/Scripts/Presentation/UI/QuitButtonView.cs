using UnityEngine;

namespace _Project.Scripts.Presentation.UI
{
    public class QuitButtonView : MonoBehaviour
    {
        public void OnExitButtonClicked()
        {
            Application.Quit();
        }
    }
}