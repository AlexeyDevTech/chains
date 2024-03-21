namespace Chains.Core.StateManager
{
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
}
