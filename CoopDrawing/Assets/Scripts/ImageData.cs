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

    private bool[,] expandKernel = new bool[,] {
        {true, true, true, true, true, true, true, true, true, true, true, true, true, true},
        {true, true, true, true, true, true, true, true, true, true, true, true, true, true},
        {true, true, true, true, true, true, true, true, true, true, true, true, true, true},
        {true, true, true, true, true, true, true, true, true, true, true, true, true, true},
        {true, true, true, true, true, true, true, true, true, true, true, true, true, true},
        {true, true, true, true, true, true, true, true, true, true, true, true, true, true},
        {true, true, true, true, true, true, true, true, true, true, true, true, true, true},
        {true, true, true, true, true, true, true, true, true, true, true, true, true, true},
        {true, true, true, true, true, true, true, true, true, true, true, true, true, true},
        {true, true, true, true, true, true, true, true, true, true, true, true, true, true},
        {true, true, true, true, true, true, true, true, true, true, true, true, true, true},
        {true, true, true, true, true, true, true, true, true, true, true, true, true, true},
        {true, true, true, true, true, true, true, true, true, true, true, true, true, true},
        {true, true, true, true, true, true, true, true, true, true, true, true, true, true},
        {true, true, true, true, true, true, true, true, true, true, true, true, true, true},
    };

    // Loads the images from their paths
    public void SetNewImage(string pathToOutline, string pathToFinal) {
        outlineImage = Resources.Load<Texture2D>(pathToOutline);
        finalImage = Resources.Load<Texture2D>(pathToFinal);
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
            if (pixel.a > 0.5f) {
                int x = i % width;
                int y = i / width;
                ExpandAroundPixel(processedPixels, x, y);
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
}