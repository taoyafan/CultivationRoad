using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseContinueButton : MonoBehaviour
{
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private Button buttonComponent;
    [SerializeField] private Image buttonIcon;

    // public void Update()
    // {
    //     // 更新按钮状态
    //     UpdateButtonState();
    // }

    public void ManualUpdate()
    {
        if (buttonText == null || buttonIcon == null)
        {
            return;
        }

        UpdateButtonText();
        UpdateButtonIconColor();
    }

    /// <summary>
    /// 更新按钮文字
    /// </summary>
    private void UpdateButtonText()
    {
        if (buttonText == null)
        {
            return;
        }

        if (TimeSystem.Instance.IsPlaying)
        {
            buttonText.text = "暂停";
        }
        else
        {
            buttonText.text = "继续";
        }
    }

    private void UpdateButtonIconColor()
    {
        if (buttonIcon == null)
        {
            return;
        }

        if (!TimeSystem.Instance.IsPlaying)
        {
            buttonIcon.color = Color.green;
        }
        else
        {
            buttonIcon.color = Color.red;
        }
    }

    public void OnClick()
    {
        if (!TimeSystem.Instance.IsPlaying)
        {
            // 当前是暂停状态，点击继续
            TimeSystem.Instance.AddAction(new ResumeTimeGA(), 0);
        }
        else
        {
            // 当前是运行状态，点击暂停
            TimeSystem.Instance.AddAction(new PauseTimeGA(), 0.01f);
        }
    }
}
