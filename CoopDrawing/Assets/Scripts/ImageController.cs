using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageController : MonoBehaviour
{
    [SerializeField] private ImageData imageData;
    [SerializeField] private ImageViewer imageViewer;

    void Start() {
        SetNewImage("Henry2");
    }

    void SetNewImage(string imageName) {
        string outlinePath = "TargetDrawings/" + imageName + "Outline";
        string finalPath = "TargetDrawings/" + imageName;
        imageData.SetNewImage(outlinePath, finalPath);
        imageViewer.RenderImage(imageData.pixelData);
    }
}