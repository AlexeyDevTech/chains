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

        internal protected Action OnEnterState { get; set; }
        internal protected Action OnUpdateState { get; set; }
        internal protected Action OnExitState { get; set; }

        protected BaseState(TState key) 
        {
            StateKey = key;
        }
        /// <summary>
        /// адрес объекта текущего состояния
        /// </summary>
        public TState StateKey { get; private set; }
        public List<Condition<TState>> Conditions = new List<Condition<TState>>();
        //////////////////////////////////////////////////////////////
        ///
        /// 
        /// На данное поле установлена логика перехода в другое состояние через
        /// метод Update()
        /// 
        /// 
        ///////////////////////////////////////////////////////////////
        /// <summary>
        /// Адрес следующего состояния
        /// </summary>
        public TState Next { get; set; }
        public abstract void EnterState();
        public abstract void UpdateState();
        public abstract void ExitState();
        public abstract TState GetNextState();
    }

    public class State<TState> : BaseState<TState> where TState : Enum
    {
        public State(TState key) : base(key) { Next = key; }
       
      

   
        public override void EnterState() => OnEnterState?.Invoke();
        public override void ExitState() => OnExitState?.Invoke();
        public override void UpdateState()
        {
            OnUpdateState?.Invoke();
            if(Conditions != null && Conditions.Count > 0)
            {
                for(int i = 0; i < Conditions.Count; i++)
                {
                    Conditions[i].Update();
                }
            }
        }
        public override TState GetNextState() => Next;
        
    }

    public class Condition<TState> where TState : Enum
    {
        TState KeyNextState;
        public Func<bool> Predicate { get; set; }
        public BaseState<TState> State { get; set; }

        public Condition(BaseState<TState> state, Func<bool> predicate ,TState keyNextState)
        {
            State = state;
            Predicate = predicate;
            KeyNextState = keyNextState;
            State.Conditions.Add(this);
        }

        public void Update()
        {
            //////////////////////////////////////////////////////////
            ///
            /// В данном участке используется своеобразный "костыль",
            /// Который меняет адрес следующего состояния для 
            /// провокации смены состояния в менеджере. Использовать
            /// такой переход можно и напрямую, с помощью 
            /// методов расширения (см. ниже)
            ///
            ///////////////////////////////////////////////////////////
            if (Predicate?.Invoke() ?? false)
                State.To(KeyNextState);
        }
    }

    public static class StatesExtensions
    {
        public static BaseState<T> To<T>(this BaseState<T> state, T next) where T : Enum
        {
            state.Next = next;
            return state;
        }
        public static BaseState<T> If<T>(this BaseState<T> state, Func<bool> predicate, T next) where T : Enum
        {
            _ = new Condition<T>(state, predicate, next);
            return state;
        }
        public static BaseState<T> From<T>(this StateManager<T> manager, T key) where T : Enum
        {
            var s = new State<T>(key);
            manager.Add(key, s);
            return s;
        }
        public static BaseState<T> Enter<T>(this BaseState<T> state, Action action) where T : Enum
        {   
            (state as State<T>).OnEnterState = action;
            return state;
        }
        public static BaseState<T> Update<T>(this BaseState<T> state, Action action) where T : Enum
        {
            (state as State<T>).OnUpdateState = action;
            return state;
        }
        public static BaseState<T> Exit<T>(this BaseState<T> state, Action action) where T : Enum
        {
            (state as State<T>).OnExitState = action;
            return state;
        }
    }
}
 