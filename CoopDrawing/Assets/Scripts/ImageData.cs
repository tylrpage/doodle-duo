using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Handles the processing of the pixel data that represents the target image. Also handles the loading
// of the images.
public class ImageData : MonoBehaviour
{
    // Width of original image in pixels
    [HideInInspector] public int width;
    // Height of original image in pixels.
    [HideInInspector] public int height;
    // pixelData represents the target outline. True values represent pixels that are within the outline.
    public bool[,] pixelData;
    // Reference image from which pixelData is generated.
    [HideInInspector] public Texture2D outlineImage;
    // The final image to show once the players complete their drawing!
    [HideInInspector] public Texture2D finalImage;
    private bool[,] expandKernel;
    // The position of dot in the target image, as a percentage of the image's width and height.
    public Vector2 StartPositionPercentage;
    // The position of the red dot in the target image, as a percentage of the image's width and height.
    public Vector2 EndPositionPercentage;

    void Awake() {
        expandKernel = GenerateKernel(5);
    }

    // Loads the images from their paths
    public void SetNewImage(string pathToOutline, string pathToFinal)
    {
        SetNewImage(Resources.Load<Texture2D>(pathToOutline), Resources.Load<Texture2D>(pathToFinal));
    }

    public void SetNewImage(Texture2D outline, Texture2D final)
    {
        outlineImage = outline;
        finalImage = final;
        pixelData = ProcessImage(outlineImage);
    }

    // Processes the outline image into an array of bools representing the pixels within which the drawing must be made
    private bool[,] ProcessImage(Texture2D image) {
        width = image.width;
        height = image.height;
        bool[,] processedPixels = new bool[width, height];
        Color[] pixels = image.GetPixels();
        for (int i = 0; i < pixels.Length; i++) {
            Color pixel = pixels[i];
            int x = i % width;
            int y = i / width;
            if (pixel.a > 0.5f) {
                ExpandAroundPixel(processedPixels, x, y);
            }
            if (pixel.g == 1f) {
                StartPositionPercentage = new Vector2((float)x / width, (float)y / height);
            }
            if (pixel.b == 1f) {
                EndPositionPercentage = new Vector2((float)x / width, (float)y / height);

            }
        }
        return processedPixels;
    }

    // Modifies processedPixels to include the given pixel and its neighbors as defined by expandKernel.
    // The center of the expandKernel is the pixel at (x, y), and the true values in the kernel are the
    // pixels that will be included in the target outline.
    private void ExpandAroundPixel(bool[,] processedPixels, int x, int y) {
        for (int i = -expandKernel.GetLength(0) / 2; i <= expandKernel.GetLength(0) / 2; i++) {
            for (int j = -expandKernel.GetLength(1) / 2; j <= expandKernel.GetLength(1) / 2; j++) {
                int expandedX = x + i;
                int expandedY = y + j;
                if (expandedX >= 0 && expandedX < width && expandedY >= 0 && expandedY < height) {
                    processedPixels[expandedX, expandedY] = true;
                }
            }
        }
    }

    // Returns true if the pixel at the given coordinates is within the target outline
    public bool IsPixelOnTarget(int x, int y) {
        if (!(x >= 0 && x < width && y >= 0 && y < height)) {
            return false;
        }
        return pixelData[x, y];
    }

    // Returns the x and y coordinates of the pixel nearest to the given percentages,
    // where (0%, 0%) is the bottom left and (100%, 100%) is the top right.
    public (int x, int y) GetNearestPixel(float xPercent, float yPercent) {
        int x = Mathf.RoundToInt(xPercent * width);
        int y = Mathf.RoundToInt(yPercent * height);
        return (x, y);
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

    // Debugging method, prints kernel to console.
    private void PrintKernel(bool[,] kernel) {
        string output = "";
        for (int i = 0; i < kernel.GetLength(0); i++) {
            for (int j = 0; j < kernel.GetLength(1); j++) {
                output += kernel[i, j] ? "1" : "0";
            }
            output += "\n";
        }
        Debug.Log(output);
    }
}