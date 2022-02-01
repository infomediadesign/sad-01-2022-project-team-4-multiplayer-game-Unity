using System;
using _Project.Scripts.Networking;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.UI
{
    public class ChatManager : MonoBehaviour
    {
        public static Action<string,string,bool> onNewChatMessageReceived;

        [SerializeField] private Transform chatMessageContent;
        [SerializeField] private TextChatItem _textChatItemPrefab;
        [SerializeField] private TMP_InputField _chatMessageInputField;
        [SerializeField] private GameObject _chatUIHolder;

        [SerializeField] private Sprite myMessageSprite, otherMessageSprite;

        private void OnEnable()
        {
            onNewChatMessageReceived += ONNewChatMessageReceived;
        }

        private void OnDisable()
        {
            onNewChatMessageReceived -= ONNewChatMessageReceived;
        }
        
        private void ONNewChatMessageReceived(string playerName, string message, bool isLocalPlayer)
        {
            TextChatItem _textChatItem = Instantiate(_textChatItemPrefab, chatMessageContent);
            _textChatItem.Setup(playerName, message, isLocalPlayer ? myMessageSprite : otherMessageSprite);
        }

        public void OnSendButtonClick()
        {
            string myChatMessage = _chatMessageInputField.text;
            if (string.IsNullOrEmpty(myChatMessage))
            {
                return;
            }

            myChatMessage = myChatMessage.Trim();
            
            if (string.IsNullOrEmpty(myChatMessage))
            {
                return;
            }

            SocketManager.GetInstance().SendChatMessage(myChatMessage);

            _chatMessageInputField.text = "";
        }

        public void ToggleChatUI()
        {
            _chatUIHolder.SetActive(!_chatUIHolder.activeSelf);
            AlwaysOnUIManager.onUpdatePlayerInput?.Invoke(_chatUIHolder.activeSelf, GameUI.ChatPanel);
        }
        
        public void HideChatUI()
        {
            _chatUIHolder.SetActive(false);
        }

        public void ClearMessages()
        {
            int messagesCount = chatMessageContent.childCount;
            for (int i = 0; i < messagesCount; i++)
            {
                Destroy(chatMessageContent.GetChild(0).gameObject);
            }
        }
    }
}
