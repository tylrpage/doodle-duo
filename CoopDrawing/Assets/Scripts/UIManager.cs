using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour, IService
{
    [SerializeField] private TMP_Text statusText;

    private void Awake()
    {
        GameManager.Instance.RegisterService(this);
    }

    public void SetStatusText(string text)
    {
        statusText.text = text;
    }
}
