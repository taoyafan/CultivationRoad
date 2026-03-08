using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace CultivationRoad.JourneyScene
{
    public class EnterBattleOnClick : MonoBehaviour
    {
        private string battleSceneName = "BattleScene";

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClick);
            }
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnButtonClick);
            }
        }

        private void OnButtonClick()
        {
            EnterBattle();
        }

        public void EnterBattle()
        {
            SceneManager.LoadScene(battleSceneName);
        }
    }
}
