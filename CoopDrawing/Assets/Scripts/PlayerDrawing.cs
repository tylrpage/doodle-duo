using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDrawing : MonoBehaviour
{
    public bool[,] pixelData;
    private Texture2D _drawingTexture;
    [SerializeField] private Image image;
    [SerializeField] private int kernalSize;
    private int width;
    private int height;
    private bool[,] expandKernel;

    public void Init(int width, int height)
    {
        this.width = width;
        this.height = height;
        
        pixelData = new bool[width, height];
        _drawingTexture = new Texture2D(width, height);
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                _drawingTexture.SetPixel(i, j, Color.clear);
            }
        }
        _drawingTexture.Apply();
        image.sprite = Sprite.Create(_drawingTexture, new Rect(0, 0, _drawingTexture.width, _drawingTexture.height), new Vector2(0.5f, 0.5f));
        expandKernel = GenerateKernel(kernalSize);
    }

    void OnDisable() {
        Destroy(_drawingTexture);
    }

    public void AdvancePixelData(Vector2 dotPosition) {
        // TODO: Do we need to do any converting between the dotPosition coordinate system and the pixelData coordinate system?
        int x = (int)dotPosition.x;
        int y = (int)dotPosition.y;

        ExpandAroundPixel(pixelData, x, y);
        _drawingTexture.Apply();
    }

    public void Clear()
    {
        // Clear our drawing
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                _drawingTexture.SetPixel(i, j, Color.clear);
            }
        }
        _drawingTexture.Apply();
    }

    // Sets the drawn image to match with whatever is in the list of given dotPositions. Use this to rewind to previous states.
    public void SetPixelData(List<Vector2> dotPositions) {
        pixelData = new bool[width, height];
        foreach (Vector2 dotPosition in dotPositions) {
            AdvancePixelData(dotPosition);
        }
    }

    // Creates a new kernel: A filled circle within a |size| x |size| grid.
    private bool[,] GenerateKernel(int size) {
        bool[,] kernel = new bool[size, size];
        float radius = size / 2.0f;
        Vector2 center = new Vector2(size / 2f, size / 2f);
        for (int i = 0; i < kernel.GetLength(0); i++) {
            for (int j = 0; j < kernel.GetLength(1); j++) {
                Vector2 point = new Vector2(i, j);
                float distance = Vector2.Distance(point, center);
                kernel[i, j] = distance <= radius;
            }
        }
        return kernel;
    }

    private void ExpandAroundPixel(bool[,] processedPixels, int x, int y) {
        for (int i = -expandKernel.GetLength(0) / 2; i <= expandKernel.GetLength(0) / 2; i++) {
            for (int j = -expandKernel.GetLength(1) / 2; j <= expandKernel.GetLength(1) / 2; j++) {
                int expandedX = x + i;
                int expandedY = y + j;
                if (expandedX >= 0 && expandedX < width && expandedY >= 0 && expandedY < height) {
                    processedPixels[expandedX, expandedY] = true;
                    _drawingTexture.SetPixel(expandedX, expandedY, Color.black);
                }
            }
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