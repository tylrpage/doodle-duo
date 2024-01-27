using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawingEvaluator : MonoBehaviour
{
    [SerializeField] private ImageData imageData;
    [SerializeField] private ImageViewer imageViewer;

    void Update() {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = 10;
        Vector3 pencilPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        if (EvaluatePosition(pencilPosition)) {
            Camera.main.backgroundColor = Color.green;
        } else {
            Camera.main.backgroundColor = Color.red;
        }
    }

    public bool EvaluatePosition(Vector3 pencilPosition) {
        Bounds imageBounds = imageViewer.GetBounds();
        Vector3 bottomLeftWorldSpace = imageBounds.min;
        float width = imageBounds.size.x;
        float height = imageBounds.size.y;

        float xPercent = (pencilPosition.x - bottomLeftWorldSpace.x) / width;
        float yPercent = (pencilPosition.y - bottomLeftWorldSpace.y) / height;

        (var x, var y) = imageData.GetNearestPixel(xPercent, yPercent);
        return imageData.IsPixelOnTarget(x, y);
    }
}