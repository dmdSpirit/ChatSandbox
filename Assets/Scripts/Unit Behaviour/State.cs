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

        // TODO: I need to be able to stop only current state event if it has parent state.
        // TODO: Do all states need an additional stop condition to be checked every update?
        public virtual void StopState()
        {
            Finish();
            parentState?.Finish();
        }


        protected virtual void PushState(State state, bool saveParent = true)
        {
            if (saveParent)
                state.parentState = this;
            OnPushState?.Invoke(state);
        }
    }
}