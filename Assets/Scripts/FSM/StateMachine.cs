using System;
using System.Collections.Generic;

public class StateMachine
{
    protected readonly List<IState> states = new List<IState>();
    protected List<StateSwitchCondition> conditions 
        = new List<StateSwitchCondition>();
    public IState CurrentState { get; protected set; }

    public bool Add(IState state)
    {
        //判断是否已经存在
        if (!states.Contains(state))
        {
            //判断是否存在同名状态
            if (states.Find(m => m.Name == state.Name) == null)
            {
                //存储到列表
                states.Add(state);
                //执行状态初始化事件
                state.OnInitialization();
                return true;
            }
        }
        return false;
    }
    public bool Add<T>(string stateName = null) where T : IState, new()
    {
        Type type = typeof(T);
        T t = (T)Activator.CreateInstance(type);
        t.Name = string.IsNullOrEmpty(stateName) ? type.Name : stateName;
        return Add(t);
    }
    public bool Remove(IState state)
    {
        //判断是否存在
        if (states.Contains(state))
        {
            //如果要移除的状态为当前状态 首先执行当前状态退出事件
            if (CurrentState == state)
            {
                CurrentState.OnExit();
                CurrentState = null;
            }
            //执行状态终止事件
            state.OnTermination();
            return states.Remove(state);
        }
        return false;
    }
    public bool Remove(string stateName)
    {
        var targetIndex = states.FindIndex(m => m.Name == stateName);
        if (targetIndex != -1)
        {
            var targetState = states[targetIndex];
            if (CurrentState == targetState)
            {
                CurrentState.OnExit();
                CurrentState = null;
            }
            targetState.OnTermination();
            return states.Remove(targetState);
        }
        return false;
    }
    public bool Remove<T>() where T : IState
    {
        return Remove(typeof(T).Name);
    }
    public bool Switch(IState state)
    {
        //如果当前状态已经是切换的目标状态 无需切换 返回false
        if (CurrentState == state) return false;
        //当前状态不为空则执行状态退出事件
        CurrentState?.OnExit();
        //判断切换的目标状态是否存在于列表中
        if (!states.Contains(state)) return false;
        //更新当前状态
        CurrentState = state;
        //更新后 当前状态不为空则执行状态进入事件
        CurrentState?.OnEnter();
        return true;
    }
    public bool Switch(string stateName)
    {
        //根据状态名称在列表中查询
        var targetState = states.Find(m => m.Name == stateName);
        return Switch(targetState);
    }
    public bool Switch<T>() where T : IState
    {
        return Switch(typeof(T).Name);
    }
    public void Switch2Next()
    {
        if (states.Count != 0)
        {
            //如果当前状态不为空 则根据当前状态找到下一个状态
            if (CurrentState != null)
            {
                int index = states.IndexOf(CurrentState);
                index = index + 1 < states.Count ? index + 1 : 0;
                IState targetState = states[index];
                //首先执行当前状态的退出事件 再更新到下一状态
                CurrentState.OnExit();
                CurrentState = targetState;
            }
            //当前状态为空 则直接进入列表中的第一个状态
            else CurrentState = states[0];
            //执行状态进入事件
            CurrentState.OnEnter();
        }
    }
    public void Switch2Last()
    {
        if (states.Count != 0)
        {
            //如果当前状态不为空 则根据当前状态找到上一个状态
            if (CurrentState != null)
            {
                int index = states.IndexOf(CurrentState);
                index = index - 1 >= 0 ? index - 1 : states.Count - 1;
                IState targetState = states[index];
                //首先执行当前状态的退出事件 再更新到上一状态
                CurrentState.OnExit();
                CurrentState = targetState;
            }
            //当前状态为空 则直接进入列表中的最后一个状态
            else CurrentState = states[states.Count - 1];
            //执行状态进入事件
            CurrentState.OnEnter();
        }
    }
    public void Switch2Null()
    {
        if (CurrentState != null)
        {
            CurrentState.OnExit();
            CurrentState = null;
        }
    }

    public T GetState<T>(string stateName) where T : IState
    {
        return (T)states.Find(m => m.Name == stateName);
    }
    public T GetState<T>() where T : IState
    {
        return (T)states.Find(m => m.Name == typeof(T).Name);
    }

    public void OnUpdate()
    {
        //若当前状态不为空 执行状态停留事件
        CurrentState?.OnStay();
        //检测所有状态切换条件
        for (int i = 0; i < conditions.Count; i++)
        {
            var condition = conditions[i];
            //条件满足
            if (condition.predicate.Invoke())
            {
                //源状态名称为空 表示从任意状态切换至目标状态
                if (string.IsNullOrEmpty(condition.sourceStateName))
                {
                    Switch(condition.targetStateName);
                }
                //源状态名称不为空 表示从指定状态切换至目标状态
                else
                {
                    //首先判断当前的状态是否为指定的状态
                    if (CurrentState.Name == condition.sourceStateName)
                    {
                        Switch(condition.targetStateName);
                    }
                }
            }
        }
    }
    public void OnDestroy()
    {
        //执行状态机内所有状态的状态终止事件
        for (int i = 0; i < states.Count; i++)
        {
            states[i].OnTermination();
        }
    }
    public StateMachine SwitchWhen(Func<bool> predicate,
        string targetStateName)
    {
        conditions.Add(new StateSwitchCondition(
            predicate, null, targetStateName));
        return this;
    }
    public StateMachine SwitchWhen(Func<bool> predicate, 
        string sourceStateName, string targetStateName)
    {
        conditions.Add(new StateSwitchCondition(
            predicate, sourceStateName, targetStateName));
        return this;
    }
    public StateBuilder<T> Build<T>(
        string stateName = null) where T : State, new()
    {
        Type type = typeof(T);
        T t = (T)Activator.CreateInstance(type);
        t.Name = string.IsNullOrEmpty(stateName) ? type.Name : stateName;
        t.machine = this;
        if (states.Find(m => m.Name == t.Name) == null)
            states.Add(t);
        return new StateBuilder<T>(t, this);
    }
}