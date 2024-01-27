using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text statusText;

    public void SetStatusText(string text)
    {
        statusText.text = text;
    }
}
