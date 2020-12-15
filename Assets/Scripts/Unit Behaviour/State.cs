using System;

namespace dmdspirit
{
    public abstract class State
    {
        public event Action<State> OnStateFinish;
        public event Action<State> OnPushState;

        public State parentState;

        public abstract void Update();

        protected virtual void Finish() => OnStateFinish?.Invoke(this);

        public virtual void StopState()
        {
            Finish();
            parentState?.Finish();
        }

        protected virtual void PushState(State state)
        {
            state.parentState = this;
            OnPushState?.Invoke(state);
        }
    }
}