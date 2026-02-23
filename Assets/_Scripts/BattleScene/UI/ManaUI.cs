using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ManaUI : MonoBehaviour
{
    [SerializeField] private TMP_Text manaText;
    public void UpdateManaText(int currentMana)
    {
        manaText.text = currentMana.ToString();
    }
}
