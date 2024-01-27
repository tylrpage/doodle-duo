using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageViewer : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    public void RenderImage(bool[,] pixelData) {
        int width = pixelData.GetLength(0);
        int height = pixelData.GetLength(1);
        Texture2D texture = new Texture2D(width, height);
        Color[] testColors = new Color[width * height];
        for (int i = 0; i < testColors.Length; i++) {
            if (pixelData[i % width, i / width]) {
                testColors[i] = Color.black;
            } else {
                testColors[i] = Color.white;
            }
        }
        texture.SetPixels(testColors);
        texture.Apply();
        spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }
}