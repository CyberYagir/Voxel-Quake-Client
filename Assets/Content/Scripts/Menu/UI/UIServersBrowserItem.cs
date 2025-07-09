using Content.Scripts.Menu.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Content.Scripts.Menu.UI
{
    public class UIServersBrowserItem: MonoBehaviour
    {
        [SerializeField] private ServersListService.ServerData serverData;

        [SerializeField] private TMP_Text nameText, playersText;
        private UIServersBrowserWindow window;


        public void Init(ServersListService.ServerData data, UIServersBrowserWindow window)
        {
            this.window = window;
            serverData = data;

            nameText.text = serverData.title;
            playersText.text = serverData.players_count + "/" + serverData.max_players_count;

                var button = GetComponent<Button>();
                button.onClick = new Button.ButtonClickedEvent();
                button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            window.ConnectToServer(serverData);
        }
    }
}