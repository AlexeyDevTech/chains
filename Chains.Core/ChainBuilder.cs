using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chains.Core
{
    
    public class StateManager
    {
        public State Root { get; set; }

        public State CreateInitialState(string name)
        {
            var r = new State(name);
            Root = r;
            return r;
        }
        public void Active(State state) => state.Active = true;
    }
    public class State
    {
        private bool _active;

        internal State Parent { get; set; }
        internal Action OnEnter { get; set; }
        internal Action OnLeave { get; set; }
        internal Action OnUpdate { get; set; }
        internal bool Active 
        { 
            get
            {
                return _active;

            }
            set
            {
                _active = value;
                if (value)
                {
                    if(Parent != null)
                     Parent.Active = false;
                    Enter();
                }
                else
                    Leave();
            }
        }

        public string Name { get; set; } = "New State";
        internal List<Condition> Conditions { get; set; }

        public State(string Name)
        {
            this.Name = Name;
            Conditions = new List<Condition>();
        }
      
        internal void AddCondition(Condition condition) => Conditions.Add(condition);
        public void UpdateState()
        {
            for(int i = 0; i < Conditions.Count; i++)
            {
                Conditions[i].Update();       
            }
            OnUpdate?.Invoke();
        }
        internal void SetUpdateAction(Action action) => OnUpdate = action;
        internal void SetEnterAction(Action action) => OnEnter = action;
        internal void SetLeaveAction(Action action) => OnLeave = action;
        internal void Enter() => OnEnter?.Invoke();
        internal void Leave() => OnLeave?.Invoke();

        public override string ToString()
        {
            return $"State {Name}";
        }

    }
    public class Condition
    {
        public Func<bool> Predicate { get; set; }
        public Action Action { get; set; }
        public State Next { get; set; }
        public void Update()
        {
            
            if (Predicate?.Invoke() ?? true)
            {
                Action?.Invoke();
                Next.Active = true;
            }

        }

    }

    public static class StateExtensions
    {
        public static State OnState(this State parent, string Name)
        {
            var s = new State(Name);
            s.Parent = parent;
            return s;
        }
        public static State Enter(this State state, Action action)
        {
            state.SetEnterAction(action);
            return state;
        }
        public static State Leave(this State state, Action action)
        {
            state.SetLeaveAction(action);
            return state;
        }
        public static State Update(this State state, Action action)
        {
            state.SetUpdateAction(action);
            return state;
        }
        public static State If(this State state, Func<bool> predict, Action action)
        {
            state.AddCondition(new Condition { Predicate = predict, Action = action });
            return state;
        }
        public static State Active(this State state)
        {
            state.Active = true;
            return state;
        }
        

    }
}
