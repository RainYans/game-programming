using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

/// Scroll-wheel zoom for the Cinemachine follow camera.
///
/// The CinemachineBrain drives the actual Camera, so zoom changes the *virtual* camera's lens
/// orthographic size — setting Camera.orthographicSize directly would be overwritten by the
/// brain every frame. Following and bounds are handled by Cinemachine (Framing Transposer +
/// Confiner2D); this component only does zoom. Lives on the Main Camera.
public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera vcam;

    [Header("Zoom (orthographic size)")]
    [SerializeField] private float zoomStep = 0.5f;
    [SerializeField] private float minOrthoSize = 2f;
    [SerializeField] private float maxOrthoSize = 12f;

    private void Awake()
    {
        if (vcam == null) vcam = FindFirstObjectByType<CinemachineVirtualCamera>();
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null || vcam == null) return;

        float scroll = mouse.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) < 0.01f) return;

        float size = vcam.m_Lens.OrthographicSize + (scroll > 0f ? -zoomStep : zoomStep);
        vcam.m_Lens.OrthographicSize = Mathf.Clamp(size, minOrthoSize, maxOrthoSize);
    }
}
