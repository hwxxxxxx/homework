using UnityEngine;

public class GameInput : MonoBehaviour
{
    public Vector2 GetMoveInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        return new Vector2(horizontal, vertical).normalized;
    }

    public Vector2 GetLookInput()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        return new Vector2(mouseX, mouseY);
    }

    public bool IsJumpPressed()
    {
        return Input.GetButtonDown("Jump");
    }

    public bool IsRunPressed()
    {
        return Input.GetKey(KeyCode.LeftShift);
    }

    public bool IsFirePressed()
    {
        return Input.GetMouseButton(0);
    }

    public bool IsAimPressed()
    {
        return Input.GetMouseButton(1);
    }

    public bool IsReloadPressed()
    {
        return Input.GetKeyDown(KeyCode.R);
    }

    public bool IsSkillPressed()
    {
        return Input.GetKeyDown(KeyCode.Q);
    }

    public bool IsInteractPressed()
    {
        return Input.GetKeyDown(KeyCode.E);
    }

    public bool IsPausePressed()
    {
        return Input.GetKeyDown(KeyCode.Escape);
    }
}