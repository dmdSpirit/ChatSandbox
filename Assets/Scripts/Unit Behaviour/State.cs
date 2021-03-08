using System;
using UnityEngine;

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
        public virtual void StopState(bool stopParent = true)
        {
            // TODO: Not working right when called from constructor.
            Finish();
            if (stopParent)
                parentState?.Finish();
        }


        protected void PushState(State state, bool saveParent = true)
        {
            if (saveParent)
                state.parentState = this;
            OnPushState?.Invoke(state);
        }

        protected MoveState PushMoveState(Unit unit, Vector3 moveDestination, float stopDistance, ObjectRadius objectRadius = null)
        {
            if (objectRadius != null) moveDestination = objectRadius.GetClosestPoint(moveDestination);
            var moveState = new MoveState(unit, moveDestination, stopDistance);
            PushState(moveState);
            return moveState;
        }
    }
}