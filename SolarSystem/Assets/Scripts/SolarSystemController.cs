using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Central manager for the interactive solar system.
/// Handles camera zoom, UI info panel, and return-to-overview.
/// Attach this to an empty GameObject called "SolarSystemController".
/// </summary>
public class SolarSystemController : MonoBehaviour
{
    public static SolarSystemController Instance { get; private set; }

    [Header("Camera")]
    public Camera mainCamera;
    [Tooltip("How far the camera stops from the selected object")]
    public float zoomDistance = 5f;
    [Tooltip("Camera transition speed (higher = faster)")]
    public float transitionSpeed = 2f;

    [Header("UI Elements")]
    public GameObject infoPanel;
    public TMP_Text nameText;
    public TMP_Text factText;
    public Button backButton;

    [Header("Audio")]
    [Tooltip("Assign a click sound AudioClip here (optional)")]
    public AudioSource clickAudio;

    private Vector3 defaultCameraPosition;
    private Quaternion defaultCameraRotation;
    private Transform focusTarget;
    private bool isReturning = false;
    private Coroutine returnCoroutine;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        // Save the starting camera transform as the overview position
        defaultCameraPosition = mainCamera.transform.position;
        defaultCameraRotation = mainCamera.transform.rotation;

        if (infoPanel != null) infoPanel.SetActive(false);
        if (backButton != null) backButton.onClick.AddListener(ReturnToMainView);
    }

    void Update()
    {
        // While a planet/moon is focused, keep the camera following it smoothly
        if (focusTarget != null && !isReturning)
        {
            // Zoom in from the direction the camera is currently at
            Vector3 dirFromTarget = (mainCamera.transform.position - focusTarget.position).normalized;
            Vector3 desiredPos = focusTarget.position + dirFromTarget * zoomDistance + Vector3.up * zoomDistance * 0.3f;

            mainCamera.transform.position = Vector3.Lerp(
                mainCamera.transform.position,
                desiredPos,
                Time.deltaTime * transitionSpeed * 3f
            );

            // Smoothly look at the focused object
            Quaternion desiredRot = Quaternion.LookRotation(focusTarget.position - mainCamera.transform.position);
            mainCamera.transform.rotation = Quaternion.Slerp(
                mainCamera.transform.rotation,
                desiredRot,
                Time.deltaTime * transitionSpeed * 3f
            );
        }
    }

    /// <summary>
    /// Called by PlanetClickHandler when a planet or moon is clicked.
    /// </summary>
    public void FocusOn(Transform target, string planetName, string fact)
    {
        focusTarget = target;
        isReturning = false;

        if (returnCoroutine != null) StopCoroutine(returnCoroutine);

        if (clickAudio != null) clickAudio.Play();

        if (nameText != null) nameText.text = planetName;
        if (factText != null) factText.text = fact;
        if (infoPanel != null) infoPanel.SetActive(true);
    }

    /// <summary>
    /// Smoothly returns camera to the overview position.
    /// Connected to the Back button in the UI.
    /// </summary>
    public void ReturnToMainView()
    {
        focusTarget = null;
        isReturning = true;

        if (infoPanel != null) infoPanel.SetActive(false);
        if (returnCoroutine != null) StopCoroutine(returnCoroutine);
        returnCoroutine = StartCoroutine(AnimateReturn());
    }

    IEnumerator AnimateReturn()
    {
        float elapsed = 0f;
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * transitionSpeed;
            mainCamera.transform.position = Vector3.Lerp(startPos, defaultCameraPosition, elapsed);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, defaultCameraRotation, elapsed);
            yield return null;
        }

        mainCamera.transform.position = defaultCameraPosition;
        mainCamera.transform.rotation = defaultCameraRotation;
        isReturning = false;
    }
}
