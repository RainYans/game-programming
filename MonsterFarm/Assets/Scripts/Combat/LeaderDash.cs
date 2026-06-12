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

    private AvatarController controller;
    private float dashTimer;
    private float cooldownTimer;

    /// Number of dashes performed. Read by the combat tutorial to advance the "Shift to dash" step.
    public int DashCount { get; private set; }

    private void Awake() => controller = GetComponent<AvatarController>();

    private void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;

        if (dashTimer > 0f)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f && controller != null) controller.SpeedMultiplier = 1f;
        }

        if (controller == null) return;
        if (dashTimer > 0f || cooldownTimer > 0f) return;
        if (KeyBindings.Pressed(BindAction.Dash))
        {
            dashTimer = dashDuration;
            cooldownTimer = dashCooldown;
            controller.SpeedMultiplier = dashMultiplier;
            DashCount++;
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
