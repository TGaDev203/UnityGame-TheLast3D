using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [Header("Parallax Settings")]
    [SerializeField] private float movementStrength;
    [SerializeField] private bool moveHorizontal = true;
    [SerializeField] private bool moveVertical = false;

    [Header("Animation Settings")]
    [SerializeField] private float animationSpeed;
    [SerializeField] private float animationOffset;

    [Header("Internal State")]
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.localPosition;
    }

    void Update()
    {
        float time = Time.unscaledTime * animationSpeed + animationOffset;
        float x = moveHorizontal ? Mathf.Sin(time) * movementStrength : 0f;
        float y = moveVertical ? Mathf.Cos(time) * movementStrength : 0f;

        transform.localPosition = startPosition + new Vector3(x, y, 0f);
    }
}