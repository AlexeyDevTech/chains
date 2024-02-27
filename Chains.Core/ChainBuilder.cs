using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chains.Core
{

    //public class States
    //{
    //    public Dictionary<string, State> StatesList { get; set; } = new Dictionary<string, State>();
    //    public State CurrentState { get; set; }

    //    //public State CreateInitialState(string name)
    //    //{
    //    //    var r = new State(name);
    //    //    Root = r;
    //    //    return r;
    //    //}


    //    public void Active(State state) => state.Active = true;
    //}
    //public class State
    //{
    //    private bool _active;


    //    internal Action OnEnter { get; set; }
    //    internal Action OnLeave { get; set; }
    //    internal Action OnUpdate { get; set; }
    //    internal bool Active 
    //    { 
    //        get
    //        {
    //            return _active;

    //        }
    //        set
    //        {
    //            _active = value;
    //            if (value)
    //            {

    //                Enter();
    //            }
    //            else
    //                Leave();
    //        }
    //    }

    //    public string Name { get; set; } = "New State";
    //    internal List<Condition> Conditions { get; set; }

    //    public State(string Name)
    //    {
    //        this.Name = Name;
    //        Conditions = new List<Condition>();
    //    }

    //    internal void AddCondition(Condition condition) => Conditions.Add(condition);
    //    public void UpdateState()
    //    {
    //        for(int i = 0; i < Conditions.Count; i++)
    //        {
    //            Conditions[i].Update();       
    //        }
    //        OnUpdate?.Invoke();
    //    }
    //    internal void SetUpdateAction(Action action) => OnUpdate = action;
    //    internal void SetEnterAction(Action action) => OnEnter = action;
    //    internal void SetLeaveAction(Action action) => OnLeave = action;
    //    internal void Enter() => OnEnter?.Invoke();
    //    internal void Leave() => OnLeave?.Invoke();

    //    public override string ToString()
    //    {
    //        return $"State {Name}";
    //    }

    //}
    //public class Condition
    //{
    //    public Func<bool> Predicate { get; set; }
    //    public Action Action { get; set; }
    //    public void Update()
    //    {

    //        if (Predicate?.Invoke() ?? true)
    //        {
    //            Action?.Invoke();
    //        }

    //    }

    //}

    //public static class StateExtensions
    //{
    //    public static State OnState(this State parent, string Name)
    //    {
    //        var s = new State(Name);
    //        return s;
    //    }

    //    public static State ToState(this State state, string Name)
    //    {
    //        return state;
    //    }

    //    public static State Enter(this State state, Action action)
    //    {
    //        state.SetEnterAction(action);
    //        return state;
    //    }
    //    public static State Leave(this State state, Action action)
    //    {
    //        state.SetLeaveAction(action);
    //        return state;
    //    }
    //    public static State Update(this State state, Action action)
    //    {
    //        state.SetUpdateAction(action);
    //        return state;
    //    }
    //    public static State If(this State state, Func<bool> predict, Action action)
    //    {
    //        state.AddCondition(new Condition { Predicate = predict, Action = action });
    //        return state;
    //    }
    //    public static State Active(this State state)
    //    {
    //        state.Active = true;
    //        return state;
    //    }


    //}

    public abstract class StateManager<TState> where TState : Enum
    {
        Dictionary<TState, BaseState<TState>> States { get; set; } = new Dictionary<TState, BaseState<TState>>();
        public BaseState<TState> Current { get; set; }
        public bool IsTransitionState { get; set; }
        public StateManager() { }
        public void Start() 
        {
            Current.EnterState();
        }
        public void Update() 
        {
            TState nsKey = Current.GetNextState();
            if (nsKey.Equals(Current.StateKey))
                Current.UpdateState();
            else if(!IsTransitionState)
                TransitionToState(nsKey);
        }
        public void TransitionToState(TState state)
        {
            IsTransitionState = true;
            Current.ExitState();
            Current = States[state];
            Current.EnterState();
            IsTransitionState = false;
        }
        internal void Add(TState key, BaseState<TState> State) => States.Add(key, State);
    }

    public abstract class BaseState<TState> where TState : Enum
    {
        protected BaseState(TState key) 
        {
            StateKey = key;
        }
        public TState StateKey { get; private set; }
        public abstract void EnterState();
        public abstract void UpdateState();
        public abstract void ExitState();
        public abstract TState GetNextState();
    }

    public class State<TState> : BaseState<TState> where TState : Enum
    {
        public State(TState key) : base(key) { }
        public TState Next { get; set; }
        public Action OnEnterState { get; set; }
        public Action OnUpdateState { get; set; }
        public Action OnExitState { get; set; }

        public List<Condition<TState>> Conditions = new List<Condition<TState>>();
        public override void EnterState() => OnEnterState?.Invoke();
        public override void ExitState() => OnExitState?.Invoke();
        public override void UpdateState()
        {
            OnUpdateState?.Invoke();

        }
        public override TState GetNextState() => Next;
        
    }

    public class Condition<TState> where TState : Enum
    {
        TState KeyNextState;
        public Func<bool> Predicate { get; set; }
        public Condition(Func<bool> predicate ,TState keyNextState)
        {
            Predicate = predicate;
            KeyNextState = keyNextState;
        }



    }


}
