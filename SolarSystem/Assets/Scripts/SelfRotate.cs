using UnityEngine;

/// <summary>
/// Makes a GameObject spin on its own Y-axis (planet self-rotation).
/// </summary>
public class SelfRotate : MonoBehaviour
{
    [Tooltip("Spin speed in degrees per second")]
    public float speed = 30f;

    void Update()
    {
        transform.Rotate(Vector3.up, speed * Time.deltaTime);
    }
}
