using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour, IService
{
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Transform dot;
    [SerializeField] private Transform page;
    [SerializeField] private Image finalImage;
    [SerializeField] private Image outlineImage;

    private DrawingManager _drawingManager;
    private StateManager _stateManager;
    private ImageManager _imageManager;

    private void Awake()
    {
        GameManager.Instance.RegisterService(this);
    }

    private void Start()
    {
        _drawingManager = GameManager.Instance.GetService<DrawingManager>();
        
        _stateManager = GameManager.Instance.GetService<StateManager>();
        _stateManager.StateChanged += OnStateChanged;
        OnStateChanged(_stateManager.CurrentState);
        
        _imageManager = GameManager.Instance.GetService<ImageManager>();
        _imageManager.ImageChanged += OnImageChanged;
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

    private void OnStateChanged(StateManager.State newState)
    {
        switch (newState)
        {
            case StateManager.State.Waiting:
                // Hide everything
                dot.gameObject.SetActive(false);
                outlineImage.gameObject.SetActive(false);
                finalImage.gameObject.SetActive(false);
                break;
            case StateManager.State.Playing:
                // Show stuff to play
                dot.gameObject.SetActive(true);
                outlineImage.gameObject.SetActive(true);
                break;
        }
    }
    
    private void OnImageChanged()
    {
        if (_imageManager.CurrentImageIndex < 0)
        {
            // No current image
            outlineImage.sprite = null;
            finalImage.sprite = null;

            return;
        }
        
        outlineImage.sprite = _imageManager.levels[_imageManager.CurrentImageIndex].processedOutline;
        finalImage.sprite = _imageManager.levels[_imageManager.CurrentImageIndex].final;
    }
}
