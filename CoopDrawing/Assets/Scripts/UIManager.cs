using System;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour, IService
{
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Transform dot;
    [SerializeField] private Transform page;

    private DrawingManager _drawingManager;

    private void Awake()
    {
        GameManager.Instance.RegisterService(this);
    }

    private void Start()
    {
        _drawingManager = GameManager.Instance.GetService<DrawingManager>();
    }

    private void LateUpdate()
    {
        // Calculate and update dot position
        
        dot.position = _drawingManager.DotPosition + (Vector2)page.position - (_drawingManager.PageSize / 2f);
    }

    public void SetStatusText(string text)
    {
        statusText.text = text;
    }
}
