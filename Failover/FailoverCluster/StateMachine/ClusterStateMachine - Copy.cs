using System;
using Automatonymous;

namespace StateMachines
{
    public abstract class InstanceState
    {
        public int ComponentId { get; set; }
        public bool IsValid { get; set; }
    }

    public class MyState : InstanceState
    {
        public State CurrentState { get; set; }
    }

    public class MyStateMachine : AutomatonymousStateMachine<MyState>
    {
        public MyStateMachine()
        {
            InstanceState(x => x.CurrentState);

            State(() => Loading);
            State(() => Valid);
            State(() => Invalid);
            State(() => Stopped);

            Event(() => Start);
            Event(() => Update);
            Event(() => Stop);

            Initially(When(Start).TransitionTo(Loading));

            DuringAny(When(Update).Then((state, callerState) =>
            {
                //calls from same instance are illegal
                if (state.ComponentId == callerState.ComponentId)
                    this.RaiseEvent(state, Stop);
            }).Finalize());

            During(Loading,
                When(Update, callerState => callerState.IsValid)
                    .Then((state, isvalid) => state.IsValid = true)
                    .TransitionTo(Valid),
                When(Update, callerState => !callerState.IsValid)
                    .Then((state, isvalid) => state.IsValid = false)
                    .TransitionTo(Invalid));

            DuringAny(When(Stop).TransitionTo(Stopped).Finalize());
        }


        public State Loading { get; private set; }
        public State Valid { get; private set; }
        public State Invalid { get; private set; }
        public State Stopped { get; private set; }

        public Event Start { get; private set; }
        public Event<InstanceState> Update { get; private set; }
        public Event Stop { get; private set; }
    }

}