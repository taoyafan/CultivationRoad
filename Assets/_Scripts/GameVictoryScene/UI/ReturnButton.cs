using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnButton : MonoBehaviour
{
    public void OnClick()
    {
        SceneManager.LoadScene("BattleScene");
    }
}
