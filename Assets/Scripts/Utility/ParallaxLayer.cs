using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [Header("Parallax Settings")]
    public float movementStrength = 0.05f;
    public bool moveHorizontal = true;
    public bool moveVertical = false;

    [Header("Animation Settings")]
    public float animationSpeed = 1.0f;
    public float animationOffset = 0f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.localPosition;
    }

    void Update()
    {
        float time = Time.time * animationSpeed + animationOffset;
        float x = moveHorizontal ? Mathf.Sin(time) * movementStrength : 0f;
        float y = moveVertical ? Mathf.Cos(time) * movementStrength : 0f;

        transform.localPosition = startPosition + new Vector3(x, y, 0f);
    }
}