using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Knob : MonoBehaviour
{
    [SerializeField] private Sprite[] knobSprites;
    [SerializeField] private Image image;
    [SerializeField] private Color unassignedColor;
    [SerializeField] private Color assignedColor;
    [SerializeField] private float frameDuration;
    
    private int currentSpriteIndex;
    private Coroutine spinCoroutine;
    bool spinning;

    void Start() {
        spinning = false;
    }
    
    void Update() {
        // if (Input.GetKey(KeyCode.UpArrow)) {
        //     Increase();
        // } else if (Input.GetKey(KeyCode.DownArrow)) {
        //     Decrease();
        // } else {
        //     Stop();
        // }
    }

    public void SetAssigned(bool assigned)
    {
        image.color = assigned ? assignedColor : unassignedColor;
    }

    public void Increase() {
        if (!spinning) {
            spinCoroutine = StartCoroutine(ClockwiseSpin());
            spinning = true;
        }
    }

    public void Decrease() {
        if (!spinning) {
            spinCoroutine = StartCoroutine(CounterClockwiseSpin());
            spinning = true;
        }
    }

    public void Stop() {
        if (spinning) {
            StopCoroutine(spinCoroutine);
            spinning = false;
        }
    }

    private IEnumerator ClockwiseSpin() {
        while (true) {
            currentSpriteIndex = (currentSpriteIndex + 1) % knobSprites.Length;
            image.sprite = knobSprites[currentSpriteIndex];
            yield return new WaitForSeconds(frameDuration);
        }
    }

    private IEnumerator CounterClockwiseSpin() {
        while (true) {
            currentSpriteIndex = (currentSpriteIndex - 1) % knobSprites.Length;
            if (currentSpriteIndex < 0) currentSpriteIndex += knobSprites.Length;
            image.sprite = knobSprites[currentSpriteIndex];
            yield return new WaitForSeconds(frameDuration);
        }
    }
}
