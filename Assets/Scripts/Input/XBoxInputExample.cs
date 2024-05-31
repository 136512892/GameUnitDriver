using UnityEngine;

public class XBoxInputExample : MonoBehaviour
{
    private void OnGUI()
    {
        GUILayout.Label(string.Format(" A：{0}", Input.GetKey(XBox.A)));
        GUILayout.Label(string.Format(" B：{0}", Input.GetKey(XBox.B)));
        GUILayout.Label(string.Format(" X：{0}", Input.GetKey(XBox.X)));
        GUILayout.Label(string.Format(" Y：{0}", Input.GetKey(XBox.Y)));
        GUILayout.Label(string.Format(" LB：{0}", Input.GetKey(XBox.LB)));
        GUILayout.Label(string.Format(" RB：{0}", Input.GetKey(XBox.RB)));
        GUILayout.Label(string.Format(" Left Stick Button：{0}", 
            Input.GetKey(XBox.LeftStick)));
        GUILayout.Label(string.Format(" Right Stick Button：{0}",
            Input.GetKey(XBox.RightStick)));
        GUILayout.Label(string.Format(" View：{0}", 
            Input.GetKey(XBox.View)));
        GUILayout.Label(string.Format(" Menu：{0}", 
            Input.GetKey(XBox.Menu)));

        GUILayout.Label(string.Format(" Left Stick：{0}", new Vector2(
            Input.GetAxis(XBox.LeftStickHorizontal),
            Input.GetAxis(XBox.LeftStickVertical))));
        GUILayout.Label(string.Format(" Right Stick：{0}", new Vector2(
            Input.GetAxis(XBox.RightStickHorizontal),
            Input.GetAxis(XBox.RightStickVertical))));
        GUILayout.Label(string.Format(" D Pad：{0}", new Vector2(
            Input.GetAxis(XBox.DPadHorizontal),
            Input.GetAxis(XBox.DPadVertical))));
        GUILayout.Label(string.Format(" LT：{0}", Input.GetAxis(XBox.LT)));
        GUILayout.Label(string.Format(" RT：{0}", Input.GetAxis(XBox.RT)));
    }
}