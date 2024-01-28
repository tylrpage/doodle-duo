using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDrawing : MonoBehaviour
{
    public bool[,] pixelData;
    private Texture2D _drawingTexture;
    [SerializeField] private Image image;
    private DrawingManager _drawingManager;
    private int width;
    private int height;

    void Start() {
        _drawingManager = GameManager.Instance.GetService<DrawingManager>();
        _drawingManager.DotMoved += AdvancePixelData;
        width = (int)_drawingManager.PageSize.x;
        height = (int)_drawingManager.PageSize.y;
        
        pixelData = new bool[width, height];
        _drawingTexture = new Texture2D(width, height);
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                _drawingTexture.SetPixel(i, j, Color.clear);
            }
        }
        _drawingTexture.Apply();
        image.sprite = Sprite.Create(_drawingTexture, new Rect(0, 0, _drawingTexture.width, _drawingTexture.height), new Vector2(0.5f, 0.5f));
    }

    void OnDisable() {
        _drawingManager.DotMoved -= AdvancePixelData;
        Destroy(_drawingTexture);
    }

    public void AdvancePixelData(Vector2 dotPosition) {
        // TODO: Do we need to do any converting between the dotPosition coordinate system and the pixelData coordinate system?
        int x = (int)dotPosition.x;
        int y = (int)dotPosition.y;

        pixelData[x, y] = true;
        _drawingTexture.SetPixel(x, y, Color.black);
        _drawingTexture.Apply();
    }

    // Sets the drawn image to match with whatever is in the list of given dotPositions. Use this to rewind to previous states.
    public void SetPixelData(List<Vector2> dotPositions) {
        pixelData = new bool[width, height];
        foreach (Vector2 dotPosition in dotPositions) {
            AdvancePixelData(dotPosition);
        }
    }

    // TODO: Render the drawing to the screen based on the pixelData. If it's too slow, optimize it later by only drawing the new parts.
    private void RenderDrawing() {
        Color[] colors = new Color[width * height];
        for (int i = 0; i < colors.Length; i++) {
            if (pixelData[i % width, i / width]) {
                colors[i] = Color.black;
            } else {
                colors[i] = Color.clear;
            }
        }
        _drawingTexture.SetPixels(colors);
        _drawingTexture.Apply();
    }
}