using UnityEngine;

namespace Framework.StateMachines
{
    /// <summary>
    /// A state that belongs to a specific type of state machine and exists as a component
    /// in a scene.
    /// </summary>
    /// <typeparam name="TStateMachine">The type of state machine this state can be a part of.</typeparam>
    public abstract class StateComponent<TStateMachine, TState> : MonoBehaviour
        where TStateMachine : StateMachineComponent<TStateMachine, TState>
        where TState : StateComponent<TStateMachine, TState>
    {
        /// <summary>
        /// The state machine this state belongs to.
        /// </summary>
        public TStateMachine StateMachine { get; private set; }

        protected virtual void Awake()
        {
            StateMachine = GetComponentInParent<TStateMachine>();

            if (StateMachine != null)
            {
                StateMachine.RegisterState(this as TState);
            }
            else
            {
                Debug.LogError($"{GetType().Name} cannot find a parent state machine of type {typeof(TStateMachine).Name}!");
            }
        }

        protected virtual void OnDestroy()
        {
            if (StateMachine != null)
            {
                StateMachine.RegisterState(this as TState);
            }
        }

        /// <summary>
        /// Called to evaluate if the state machine should transition out of this state.
        /// </summary>
        /// <returns>Returns the state to transition to, or null to remain in this state.</returns>
        public abstract TState GetNextState();

        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        /// <param name="previousState">The state that was previously active, 
        /// or null if this is the initial state.</param>
        public virtual void OnEnter(TState previousState)
        {
        }

        /// <summary>
        /// Called just before the state is exited.
        /// </summary>
        /// <param name="nextState">The state that will be next active, or null
        /// if the state machine is being disabled.</param>
        public virtual void OnExit(TState nextState)
        {
        }

        /// <summary>
        /// Called when the state should process game logic.
        /// </summary>
        public virtual void OnFixedUpdate()
        {
        }

        /// <summary>
        /// Called when the state should process pre-animation visuals.
        /// </summary>
        public virtual void OnUpdate()
        {
        }

        /// <summary>
        /// Called when the state should process post-animation visuals.
        /// </summary>
        public virtual void OnLateUpdate()
        {
        }
    }
}
