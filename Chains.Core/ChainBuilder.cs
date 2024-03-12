using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace ANG24.Core.Services
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

    public class BaseStateManager<TState> : StateManager<TState> where TState : Enum { }

    public abstract class BaseState<TState> where TState : Enum
    {
        private TState _next;



        internal protected Action<BaseState<TState>> OnEnterState { get; set; }
        internal protected Action<BaseState<TState>> OnUpdateState { get; set; }
        internal protected Action<BaseState<TState>> OnExitState { get; set; }
        public TState Next
        {
            get
            {
                return _next;
            }
            set
            {
                _next = value;

            }

        }

        protected BaseState(TState key)
        {
            StateKey = key;
        }
        /// <summary>
        /// адрес объекта текущего состояния
        /// </summary>
        public TState StateKey { get; private set; }
        public List<Condition<TState>> Conditions = new List<Condition<TState>>();
        internal List<ICounter<int, TState>> locales { get; set; } = new List<ICounter<int, TState>>();


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

        public abstract void EnterState();
        public abstract void UpdateState();
        public abstract void ExitState();
        public abstract TState GetNextState();
    }

    public class State<TState> : BaseState<TState> where TState : Enum
    {
        public State(TState key) : base(key) { Next = key; }




        public override void EnterState() => OnEnterState?.Invoke(this);
        public override void ExitState() => OnExitState?.Invoke(this);
        public override void UpdateState()
        {
            var conditionSuccess = false;
            var localeSuccess = false;
            if (locales != null && locales.Count > 0)
            {
                for (int i = 0; i < locales.Count; i++)
                {
                    if (locales[i].Update())
                    {
                        localeSuccess = true;
                        break;
                    }
                }
            }
            if (Conditions != null && Conditions.Count > 0)
            {
                for (int i = 0; i < Conditions.Count; i++)
                {
                    if (Conditions[i].Update())
                    {
                        conditionSuccess = true;
                        break;
                    }
                }
            }
            if (!conditionSuccess || !localeSuccess)
                OnUpdateState?.Invoke(this);
        }
        public override TState GetNextState() => Next;

    }

    public class Condition<TState> where TState : Enum
    {
        protected TState KeyNextState;
        public Func<bool> Predicate { get; set; }
        public BaseState<TState> State { get; set; }

        public Condition(BaseState<TState> state, Func<bool> predicate, TState keyNextState)
        {
            State = state;
            Predicate = predicate;
            KeyNextState = keyNextState;
            State.Conditions.Add(this);
        }
        protected Condition(Func<bool> predicate, TState keyNextState)
        {
            Predicate = predicate;
            KeyNextState = keyNextState;
        }

        public virtual bool Update()
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
            {
                State.To(KeyNextState);
                return true;
            }
            return false;
        }
    }

    public class IntCounter<TState> : ICounter<int, TState> where TState : Enum
    {
        int startValue;
        bool isPause = false;
        public int Counter { get; set; }
        public int CountTicker { get; set; }
        public Func<int, bool> predicate { get; set; }
        public TState StateId { get; set; }
        public TState Next { get; set; }
        public BaseState<TState> state { get; set; }

        public IntCounter(BaseState<TState> state, int startValue, int increment, Func<int, bool> predicate, TState next)
        {

            this.state = state;
            Counter = startValue;
            this.startValue = startValue;
            CountTicker = increment;
            this.predicate = predicate;
            Next = next;
        }

        public bool Update()
        {
            if (!isPause)
                Counter += CountTicker;

            if (predicate?.Invoke(Counter) ?? false)
            {
                state.To(Next);
                return true;
            }
            return false;
        }

        public void Reset()
        {
            Counter = startValue;
        }

        public void Pause() => isPause = true;
        public void Resume() => isPause = false;
        public void Zero() => Counter = 0;
    }

    public class GlobalCondition<TState> : Condition<TState> where TState : Enum
    {
        public GlobalCondition(Func<bool> predicate, TState keyNextState) : base(predicate, keyNextState) { }

        public override bool Update()
        {
            if (Predicate?.Invoke() ?? false)
            {
                return true;
            }
            else return false;
        }
        public TState GetNext() => KeyNextState;

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
                manager.Set(s);
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
        public static BaseState<T> Set<T>(this StateManager<T> manager, BaseState<T> state) where T : Enum
        {
            manager.Current = state;
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
