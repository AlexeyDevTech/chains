using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace Chains.Core.StateManager
{
    public abstract class StateManager<TState> where TState : Enum
    {


        internal Dictionary<TState, BaseState<TState>> States { get; set; } = new Dictionary<TState, BaseState<TState>>();

        internal List<GlobalCondition<TState>> Conditions { get; set; } = new List<GlobalCondition<TState>>();


        public BaseState<TState> Current { get; set; }
        public bool IsTransitionState { get; set; }
        public StateManager() { }
        public void Start()
        {
            Current.EnterState();
        }
        public void Update()
        {
            //для глобальных условий
            var conditionSuccess = false;
            if (Conditions != null && Conditions.Count > 0)
            {
                for (int i = 0; i < Conditions.Count; i++)
                {
                    if (Conditions[i].Update())
                    {
                        TransitionToState(Conditions[i].GetNext());
                        return;
                    }
                }
            }



            Current.UpdateState();
            TState nsKey = Current.GetNextState();
            if (!nsKey.Equals(Current.StateKey) && !IsTransitionState)
                TransitionToState(nsKey);

        }
        public void TransitionToState(TState state)
        {
            IsTransitionState = true;
            Current.Next = Current.StateKey;
            Current.ExitState();
            Current = States[state];
            Current.EnterState();
            IsTransitionState = false;
        }
        internal void Add(TState key, BaseState<TState> State) => States.Add(key, State);


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

        public static StateManager<T> If<T>(this StateManager<T> manager, Func<bool> predicate, T next) where T : Enum
        {
            var cond = new GlobalCondition<T>(predicate, next);
            manager.Conditions.Add(cond);
            return manager;
        }

        public static BaseState<T> While<T>(this BaseState<T> state, int startValue, int increment, Func<int, bool> predicate, T next) where T : Enum
        {
            state.locales.Add(new IntCounter<T>(state, startValue, increment, predicate, next));
            return state;
        }

        /// <summary>
        /// Возвращает объект состояния. 
        /// Если объекта нет -- создает его и добавляет в список состояний менеджера
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="manager"></param>
        /// <param name="key"></param>
        /// <param name="Activate">если указано true -- устанавливает состояние как текущее активное</param>
        /// <returns></returns>
        public static BaseState<T> From<T>(this StateManager<T> manager, T key, bool Activate = false) where T : Enum
        {
            BaseState<T> s = null;
            if (!manager.States.ContainsKey(key))
            {
                s = new State<T>(key);
                manager.Add(key, s);

            }
            else
            {
                s = manager.States[key];

            }
            if (Activate)
                manager.Set(s, true);
            return s;

        }
        public static BaseState<T> Enter<T>(this BaseState<T> state, Action<BaseState<T>> action) where T : Enum
        {
            (state as State<T>).OnEnterState = action;
            return state;
        }
        public static BaseState<T> Update<T>(this BaseState<T> state, Action<BaseState<T>> action) where T : Enum
        {
            (state as State<T>).OnUpdateState = action;
            return state;
        }
        public static BaseState<T> Exit<T>(this BaseState<T> state, Action<BaseState<T>> action) where T : Enum
        {
            (state as State<T>).OnExitState = action;
            return state;
        }
        /// <summary>
        /// Устанавливает состояние как текущее
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="manager"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static BaseState<T> Set<T>(this StateManager<T> manager, T state, bool activate = false) where T : Enum
        {
            manager.Current = manager.States[state];
            if(activate)
                manager.Current.EnterState();
            return manager.States[state];
        }
        private static BaseState<T> Set<T>(this StateManager<T> manager, BaseState<T> state, bool activate = false) where T : Enum
        {
            manager.Current = state;
            if(activate)
                manager.Current.EnterState();
            return state;
        }
    }


    public interface ICounter<TCount, TState> where TState : Enum
    {
        BaseState<TState> state { get; set; }
        TCount Counter { get; set; }
        TCount CountTicker { get; set; }
        TState Next { get; set; }
        Func<int, bool> predicate { get; set; }
        bool Update();
        void Reset();
        void Zero();
        void Pause();
        void Resume();

    }
}
