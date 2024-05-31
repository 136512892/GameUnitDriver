using System;

public class State : IState
{
    public string Name { get; set; }

    public StateMachine machine;
    public Action onInitialization;
    public Action onEnter;
    public Action onStay;
    public Action onExit;
    public Action onTermination;

    public virtual void OnInitialization()
    {
        onInitialization?.Invoke();
    }
    public virtual void OnEnter()
    {
        onEnter?.Invoke();
    }
    public virtual void OnStay()
    {
        onStay?.Invoke();
    }
    public virtual void OnExit()
    {
        onExit?.Invoke();
    }
    public virtual void OnTermination()
    {
        onTermination?.Invoke();
    }
    /// <summary>
    /// 设置状态切换条件
    /// </summary>
    /// <param name="predicate">切换条件</param>
    /// <param name="targetStateName">目标状态名称</param>
    public void SwitchWhen(Func<bool> predicate, string targetStateName)
    {
        machine.SwitchWhen(predicate, Name, targetStateName);
    }
}