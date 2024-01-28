using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knob : MonoBehaviour
{
    [SerializeField] private Sprite[] knobSprites;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private int currentSpriteIndex;
    private Coroutine spinCoroutine;
    bool spinning;

    void Start() {
        spinning = false;
    }

    // TODO: Replace this with the actual logic when buttons are pressed.
    void Update() {
        if (Input.GetKey(KeyCode.UpArrow)) {
            Increase();
        } else if (Input.GetKey(KeyCode.DownArrow)) {
            Decrease();
        } else {
            Stop();
        }
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
            spriteRenderer.sprite = knobSprites[currentSpriteIndex];
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator CounterClockwiseSpin() {
        while (true) {
            currentSpriteIndex = (currentSpriteIndex - 1) % knobSprites.Length;
            if (currentSpriteIndex < 0) currentSpriteIndex += knobSprites.Length;
            spriteRenderer.sprite = knobSprites[currentSpriteIndex];
            yield return new WaitForSeconds(0.1f);
        }
    }
}
