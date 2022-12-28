using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AOClient.UI.Main.PlayerInfo
{
    public sealed class MapPositionUI : MonoBehaviour
    {
        [SerializeField] private Button showHideMapNameButton;
        [SerializeField] private TextMeshProUGUI mapName, position;

        private void Start()
        {
            showHideMapNameButton.onClick.AddListener(ShowHideMapName);
            mapName.gameObject.SetActive(false);
        }

        public void UpdatePosition(string position, string mapName)
        {
            this.position.text = position;
            this.mapName.text = mapName;
        }

        private void ShowHideMapName()
        {
            position.gameObject.SetActive(!position.gameObject.activeSelf);
            mapName.gameObject.SetActive(!mapName.gameObject.activeSelf);
        }
    }
}
