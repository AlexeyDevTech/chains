namespace Chains.Core.StateManager
{
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
        public void ResetLocales()
        {
            for(int i = 0; i <  locales.Count; i++)
            {
                locales[i].Reset();
            }
        }
    }
}
