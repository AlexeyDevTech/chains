namespace Chains.Core.StateManager
{
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
}
