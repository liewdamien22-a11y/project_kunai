using UnityEngine;

public class PlayerViewBobbing : MonoBehaviour
{
    [Header("Bobbing Settings")]
    public float bobFrequency = 10f;    // Speed of the footstep cycle
    public float bobAmountX = 0.02f;    // Horizontal sway distance
    public float bobAmountY = 0.04f;    // Vertical bounce height

    [Header("Sway Settings (Mouse Look)")]
    public float swayAmount = 0.05f;    // Lag amount when looking around
    public float maxSwayAmount = 0.1f;
    public float smoothSway = 6f;

    private Vector3 defaultLocalPosition;
    private float timer = 0f;

    void Start()
    {
        // Store the resting transform offset of the weapon
        defaultLocalPosition = transform.localPosition;
    }

    void Update()
    {
        HandleWeaponSway();
        HandleWeaponBob();
    }

    void HandleWeaponSway()
    {
        // 1. Get mouse rotation input
        float mouseX = -Input.GetAxis("Mouse X") * swayAmount;
        float mouseY = -Input.GetAxis("Mouse Y") * swayAmount;

        // Clamp sway so it doesn't drift too far off-screen on fast mouse flicks
        mouseX = Mathf.Clamp(mouseX, -maxSwayAmount, maxSwayAmount);
        mouseY = Mathf.Clamp(mouseY, -maxSwayAmount, maxSwayAmount);

        Vector3 targetSway = new Vector3(mouseX, mouseY, 0);

        // Smoothly interpolate current sway toward target offset
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            defaultLocalPosition + targetSway,
            Time.deltaTime * smoothSway
        );
    }

    void HandleWeaponBob()
    {
        // 2. Check player movement velocity input
        float inputHorizontal = Input.GetAxis("Horizontal");
        float inputVertical = Input.GetAxis("Vertical");

        // Calculate if the player is actively moving on the ground
        bool isMoving = (Mathf.Abs(inputHorizontal) > 0.1f || Mathf.Abs(inputVertical) > 0.1f);

        if (isMoving)
        {
            // Advance wave timer
            timer += Time.deltaTime * bobFrequency;

            // Generate Figure-8 wave coordinates
            float waveX = Mathf.Cos(timer) * bobAmountX;
            float waveY = Mathf.Sin(timer * 2f) * bobAmountY; // 2x frequency for vertical footfalls

            // Apply bobbing offset on top of default position
            Vector3 targetPosition = defaultLocalPosition + new Vector3(waveX, waveY, 0);
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * 10f);
        }
        else
        {
            // Reset timer and return gently to resting state when standing still
            timer = 0f;
            transform.localPosition = Vector3.Lerp(transform.localPosition, defaultLocalPosition, Time.deltaTime * 5f);
        }
    }
}
