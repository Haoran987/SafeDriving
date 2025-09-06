using UnityEngine;
using UnityEngine.InputSystem;

public class DrivingWheelTest : MonoBehaviour
{
    [Header("Assign InputActionProperty (Value/Axes)")]
    public InputActionProperty steer;     // <Joystick>/stick/x
    public InputActionProperty throttle;  // <Joystick>/throttle OR split axis
    public InputActionProperty brake;     // <Joystick>/brake OR split axis
    public InputActionProperty accelerator; // <Joystick>/accelerator OR split axis (optional)
    public InputActionProperty hat;       // <Joystick>/hat (optional)

    void OnEnable() {
        steer.action.Enable();
        throttle.action.Enable();
        brake.action.Enable();
        if (accelerator.reference != null) accelerator.action.Enable();
        if (hat.reference != null) hat.action.Enable();
    }

    void Update() {
        float s = steer.action.ReadValue<float>();
        float t = throttle.action.ReadValue<float>();
        float b = brake.action.ReadValue<float>();
        if (accelerator.reference != null) {
            float a = accelerator.action.ReadValue<float>();
            t = Mathf.Max(t, a);
            Debug.Log($"Accelerator={a:F2}");
        }

        if (Mathf.Abs(s) > 0.01f || t > 0.01f || b > 0.01f)
            Debug.Log($"steer={s:F2}  throttle={t:F2}  brake={b:F2}");
        if (hat.reference != null) {
            Vector2 h = hat.action.ReadValue<Vector2>();
            if (h != Vector2.zero)
                Debug.Log($"hat={h}");
        }
    }
}
