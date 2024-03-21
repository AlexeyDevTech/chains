namespace Chains.Core.StateManager
{
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
}
