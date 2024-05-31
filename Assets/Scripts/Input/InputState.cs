/// <summary>
/// 按键状态
/// </summary>
public enum KeyState
{
    NONE,
    /// <summary>
    /// 按键开始被按下
    /// </summary>
    PRESSED,
    /// <summary>
    /// 按键持续被按下
    /// </summary>
    HOLD,
    /// <summary>
    /// 按键被抬起
    /// </summary>
    RELEASED,
}