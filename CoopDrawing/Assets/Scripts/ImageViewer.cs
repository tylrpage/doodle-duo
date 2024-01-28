using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageViewer : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    public Sprite GetRenderedSprite(bool[,] pixelData)
    {
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
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    public void RenderImage(bool[,] pixelData) {
        spriteRenderer.sprite = GetRenderedSprite(pixelData);

        ScaleImageToFit();
    }

    public void ScaleImageToFit() {
        Camera camera = Camera.main;
        float cameraHeight = camera.orthographicSize * 2;
        float cameraWidth = cameraHeight * camera.aspect;

        float originalImageWidth = spriteRenderer.bounds.size.x;
        float originalImageHeight = spriteRenderer.bounds.size.y;

        float scaleUpRatio = Mathf.Min(cameraWidth / originalImageWidth, cameraHeight / originalImageHeight);

        spriteRenderer.gameObject.transform.localScale = new Vector3(scaleUpRatio, scaleUpRatio, 1);
    }

    public Bounds GetBounds() {
        return spriteRenderer.bounds;
    }
}