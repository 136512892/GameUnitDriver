/// <summary>
/// 事件响应器接口
/// </summary>
public interface IEventResponser
{
    /// <summary>
    /// 进入事件
    /// </summary>
    void OnEnter();
    /// <summary>
    /// 点击事件
    /// </summary>
    void OnClick();
    /// <summary>
    /// 停留事件
    /// </summary>
    void OnStay();
    /// <summary>
    /// 退出事件
    /// </summary>
    void OnExit();
}