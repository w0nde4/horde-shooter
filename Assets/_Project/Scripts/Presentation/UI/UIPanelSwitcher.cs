using UnityEngine;

namespace _Project.Scripts.Presentation.UI
{
    public class UIPanelSwitcher : MonoBehaviour
    {
        public void Show(GameObject panel) => panel?.SetActive(true);
        public void Hide(GameObject panel) => panel?.SetActive(false);
    }
}