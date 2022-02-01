using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.UI
{
    public class TextChatItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI messageTextMeshProUGUI;
        [SerializeField] private Image messageBG;
        public void Setup(string playerName, string message, Sprite messageSprite)
        {
            messageTextMeshProUGUI.SetText($"{playerName} : {message}");
            messageBG.sprite = messageSprite;
            messageBG.type = Image.Type.Sliced;
            messageBG.pixelsPerUnitMultiplier = 5;
        }
    }
}
