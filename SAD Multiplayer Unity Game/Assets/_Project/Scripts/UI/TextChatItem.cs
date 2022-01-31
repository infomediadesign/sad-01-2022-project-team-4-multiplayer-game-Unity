using TMPro;
using UnityEngine;

namespace _Project.Scripts.UI
{
    public class TextChatItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI messageTextMeshProUGUI;
        public void Setup(string playerName, string message, Color messageColor)
        {
            messageTextMeshProUGUI.SetText($"{playerName} : {message}");
            messageTextMeshProUGUI.color = messageColor;
        }
    }
}
