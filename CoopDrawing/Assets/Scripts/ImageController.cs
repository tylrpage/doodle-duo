using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageController : MonoBehaviour
{
    [SerializeField] private ImageData imageData;
    [SerializeField] private ImageViewer imageViewer;
    public GameObject pencil;
    public GameObject finishDetector;

    void Start() {
        SetNewImage("Henry");
    }

    void SetNewImage(string imageName) {
        string outlinePath = "TargetDrawings/" + imageName + "Outline";
        string finalPath = "TargetDrawings/" + imageName;
        imageData.SetNewImage(outlinePath, finalPath);
        imageViewer.RenderImage(imageData.pixelData);
        SetStartAndEndPositions();
    }

    public void SetStartAndEndPositions() {
        Bounds bounds = imageViewer.GetBounds();
        Vector3 bottomLeftWorldSpace = bounds.min;
        float width = bounds.size.x;
        float height = bounds.size.y;
        pencil.transform.position = new Vector2(bottomLeftWorldSpace.x + width * imageData.StartPositionPercentage.x, bottomLeftWorldSpace.y + height * imageData.StartPositionPercentage.y);
        finishDetector.transform.position = new Vector2(bottomLeftWorldSpace.x + width * imageData.EndPositionPercentage.x, bottomLeftWorldSpace.y + height * imageData.EndPositionPercentage.y);
    }
}