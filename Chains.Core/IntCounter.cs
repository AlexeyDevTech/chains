using System.Diagnostics;

namespace Chains.Core.StateManager
{
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
            Debug.WriteLine($"[Counter from {state.StateKey}] >> Counter value = {Counter}");

            if (!(predicate?.Invoke(Counter)) ?? true)
            {
                Debug.WriteLine($"[Counter from {state.StateKey}] >> Counter is valid. Transition to {Next}");
                state.To(Next);
                Reset();
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
}
