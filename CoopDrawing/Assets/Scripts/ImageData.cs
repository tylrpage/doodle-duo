using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ImageData : MonoBehaviour
{
    [HideInInspector] public int width;
    [HideInInspector] public int height;
    public bool[,] pixelData;
    [HideInInspector] public Texture2D outlineImage;
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
}