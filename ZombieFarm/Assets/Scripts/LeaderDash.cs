using UnityEngine;
using UnityEngine.InputSystem;

/// Adds a Shift-to-dash burst to whatever has an AvatarController. Boosts the controller's
/// SpeedMultiplier for a short window then drops back, with a cooldown so it isn't spammy.
/// Used in the battle scene to give the leader a real "I'm moving" beat between fights.
[RequireComponent(typeof(AvatarController))]
public class LeaderDash : MonoBehaviour
{
    [SerializeField] private float dashMultiplier = 2.4f;
    [SerializeField] private float dashDuration = 0.22f;
    [SerializeField] private float dashCooldown = 1.2f;
    [SerializeField] private Key dashKey = Key.LeftShift;

    private AvatarController controller;
    private float dashTimer;
    private float cooldownTimer;

    private void Awake() => controller = GetComponent<AvatarController>();

    private void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;

        if (dashTimer > 0f)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f && controller != null) controller.SpeedMultiplier = 1f;
        }

        Keyboard kb = Keyboard.current;
        if (kb == null || controller == null) return;
        if (dashTimer > 0f || cooldownTimer > 0f) return;
        if (kb[dashKey].wasPressedThisFrame)
        {
            dashTimer = dashDuration;
            cooldownTimer = dashCooldown;
            controller.SpeedMultiplier = dashMultiplier;
            SfxManager.Play(SfxKind.Dash);
        }
    }

    private void OnDisable()
    {
        // Make sure we don't leave the leader permanently sprinting if the dash is interrupted
        // (e.g. the scene unloads mid-dash).
        if (controller != null) controller.SpeedMultiplier = 1f;
        dashTimer = 0f;
    }
}
