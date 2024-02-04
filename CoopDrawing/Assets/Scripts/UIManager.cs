using System;
using System.Collections;
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
    [SerializeField] private Image drawingImage;
    [SerializeField] private Knob horizontalKnob;
    [SerializeField] private Knob verticalKnob;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private float timerDangerMax;
    [SerializeField] private Color timerDangerColor;
    [SerializeField] private Color timerNormalColor;
    [SerializeField] private TMP_Text waitingText;
    [SerializeField] private TMP_Text restartingText;
    [SerializeField] private TMP_Text connectingText;
    [SerializeField] private TMP_Text winnerText;

    private DrawingManager _drawingManager;
    private StateManager _stateManager;
    private ImageManager _imageManager;
    private NetworkManager _networkManager;
    private Vector2? _previousDotPosition;

    private void Awake()
    {
        GameManager.Instance.RegisterService(this);
    }

    private void Start()
    {
        _drawingManager = GameManager.Instance.GetService<DrawingManager>();
        _drawingManager.RoleChanged += OnRoleChanged;
        OnRoleChanged(ServerRoleAssignmentMessage.Role.None);
        
        _stateManager = GameManager.Instance.GetService<StateManager>();
        _stateManager.StateChanged += OnStateChanged;
        OnStateChanged(_stateManager.CurrentState);
        
        _imageManager = GameManager.Instance.GetService<ImageManager>();
        _imageManager.ImageChanged += OnImageChanged;

        _networkManager = GameManager.Instance.GetService<NetworkManager>();
    }

    private void OnRoleChanged(ServerRoleAssignmentMessage.Role role)
    {
        horizontalKnob.SetAssigned(role == ServerRoleAssignmentMessage.Role.Horizontal);
        verticalKnob.SetAssigned(role == ServerRoleAssignmentMessage.Role.Vertical);
    }

    private void LateUpdate()
    {
        // Calculate and update dot position
        dot.position = _drawingManager.DotPosition + (Vector2)page.position - (_drawingManager.PageSize / 2f);
        
        // Update knob animations
        if (_previousDotPosition != null)
        {
            Vector2 delta = _drawingManager.DotPosition - _previousDotPosition.Value;
            if (delta.x > 0)
            {
                horizontalKnob.Increase();
            }
            else if (delta.x < 0)
            {
                horizontalKnob.Decrease();
            }
            else
            {
                horizontalKnob.Stop();
            }
            
            if (delta.y > 0)
            {
                verticalKnob.Increase();
            }
            else if (delta.y < 0)
            {
                verticalKnob.Decrease();
            }
            else
            {
                verticalKnob.Stop();
            }
        }
        _previousDotPosition = _drawingManager.DotPosition;
        
        // Update timer
        if (_stateManager.CurrentState == StateManager.State.Playing)
        {
            // Actual time left can go a little negative, to be nice, but don't show that
            float timeLeftNonNeg = Mathf.Max(0, _drawingManager.TimeLeft);
            if (timeLeftNonNeg > timerDangerMax)
            {
                timerText.color = timerNormalColor;
                timerText.text = timeLeftNonNeg.ToString("N0");
            }
            else
            {
                // Danger timer
                timerText.color = timerDangerColor;
                // Show some decimal places for fun
                timerText.text = timeLeftNonNeg.ToString("F2");
            }   
        }
    }

    public void SetStatusText(string text)
    {
        statusText.text = text;
    }

    private void OnStateChanged(StateManager.State newState)
    {
        // Only show connecting text if in connecting state
        connectingText.gameObject.SetActive(newState == StateManager.State.Connecting);
        
        switch (newState)
        {
            case StateManager.State.Connecting:
                // Hide everything
                dot.gameObject.SetActive(false);
                outlineImage.gameObject.SetActive(false);
                finalImage.gameObject.SetActive(false);
                drawingImage.gameObject.SetActive(false);
                timerText.gameObject.SetActive(false);
                waitingText.gameObject.SetActive(false);
                restartingText.gameObject.SetActive(false);
                winnerText.gameObject.SetActive(false);
                
                // Hide knob colors until playing
                horizontalKnob.SetAssigned(false);
                verticalKnob.SetAssigned(false);
                break;
            case StateManager.State.Waiting:
                // Hide everything
                dot.gameObject.SetActive(false);
                outlineImage.gameObject.SetActive(false);
                finalImage.gameObject.SetActive(false);
                drawingImage.gameObject.SetActive(false);
                timerText.gameObject.SetActive(false);
                waitingText.gameObject.SetActive(true);
                restartingText.gameObject.SetActive(false);
                winnerText.gameObject.SetActive(false);
                
                // Hide knob colors until playing
                horizontalKnob.SetAssigned(false);
                verticalKnob.SetAssigned(false);
                break;
            case StateManager.State.CountIn:
                dot.gameObject.SetActive(false);
                outlineImage.gameObject.SetActive(false);
                finalImage.gameObject.SetActive(false);
                drawingImage.gameObject.SetActive(false);
                timerText.gameObject.SetActive(true);
                timerText.text = "Get Ready...";
                waitingText.gameObject.SetActive(false);
                restartingText.gameObject.SetActive(false);
                winnerText.gameObject.SetActive(false);
                
                // Hide knob colors until playing
                horizontalKnob.SetAssigned(false);
                verticalKnob.SetAssigned(false);
                break;
            case StateManager.State.Playing:
                // Show stuff to play
                dot.gameObject.SetActive(true);
                outlineImage.gameObject.SetActive(true);
                finalImage.gameObject.SetActive(false);
                drawingImage.gameObject.SetActive(true);
                timerText.gameObject.SetActive(true);
                waitingText.gameObject.SetActive(false);
                break;
            case StateManager.State.Ending:
                outlineImage.gameObject.SetActive(false);
                finalImage.gameObject.SetActive(true);
                waitingText.gameObject.SetActive(false);
                break;
            case StateManager.State.Restarting:
                restartingText.gameObject.SetActive(true);
                waitingText.gameObject.SetActive(false);
                timerText.color = timerNormalColor;
                timerText.text = "Time's Up!";
                break;
            case StateManager.State.Won:
                winnerText.gameObject.SetActive(true);
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
