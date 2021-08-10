using System;
using System.Collections.Generic;

using UnityEngine;

namespace Framework.StateMachines
{
    /// <summary>
    /// A state machine that exists as a component in a scene.
    /// </summary>
    /// <typeparam name="TStateMachine">The type of the subclass.</typeparam>
    public abstract class StateMachineComponent<TStateMachine, TState> : MonoBehaviour
        where TStateMachine : StateMachineComponent<TStateMachine, TState>
        where TState : StateComponent<TStateMachine, TState>
    {
        private readonly List<TState> m_states = new List<TState>();

        /// <summary>
        /// The default state to enter.
        /// </summary>
        protected abstract TState InitialState { get; }

        /// <summary>
        /// The current state.
        /// </summary>
        public TState CurrentState { get; set; } = null;


        protected virtual void OnDisable()
        {
            if (CurrentState != null)
            {
                CurrentState.OnExit(null);
                CurrentState = null;
            }
        }

        /// <summary>
        /// Adds a state to the state machine.
        /// </summary>
        /// <param name="state">The state to register. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="state"/> is null.</exception>
        internal void RegisterState(TState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (!m_states.Contains(state))
            {
                m_states.Add(state);
                state.enabled = false;
            }
        }

        /// <summary>
        /// Removes a state from the state machine.
        /// </summary>
        /// <param name="state">The state to deregister. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="state"/> is null.</exception>
        internal void DeregisterState(TState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (m_states.Remove(state) && CurrentState == state)
            {
                CurrentState = null;
            }
        }

        /// <summary>
        /// Gets the first instance of a state by type.
        /// </summary>
        /// <typeparam name="T">The type of the state to find.</typeparam>
        /// <returns>The first state of the given type, or null if none are
        /// registered to this state machine.</returns>
        public T GetState<T>() where T : TState
        {
            foreach (var state in m_states)
            {
                if (state is T castedState)
                {
                    return castedState;
                }
            }

            return null;
        }

        /// <summary>
        /// Update gameplay logic for the current state.
        /// </summary>
        public virtual void OnFixedUpdate()
        {
            EvaluateTransitions();

            if (CurrentState != null)
            {
                CurrentState.OnFixedUpdate();
            }
        }

        /// <summary>
        /// Update pre-animation visuals for the current state.
        /// </summary>
        public virtual void OnUpdate()
        {
            if (CurrentState != null)
            {
                CurrentState.OnUpdate();
            }
        }

        /// <summary>
        /// Update post-animation visuals for the current state.
        /// </summary>
        public virtual void OnLateUpdate()
        {
            if (CurrentState != null)
            {
                CurrentState.OnLateUpdate();
            }
        }

        private void EvaluateTransitions()
        {
            if (CurrentState == null)
            {
                CurrentState = InitialState;
            }

            if (CurrentState != null)
            {
                var nextState = CurrentState.GetNextState();

                if (nextState == CurrentState || nextState == null)
                {
                    return;
                }
                if (!m_states.Contains(nextState))
                {
                    Debug.LogError($"Cannot transition to state {nextState.name}, it is not registered to state machine {name}!");
                    return;
                }

                CurrentState.OnExit(nextState);
                CurrentState.enabled = false;

                var previousState = CurrentState;
                CurrentState = nextState;

                CurrentState.enabled = true;
                CurrentState.OnEnter(previousState);
            }
        }
    }
}
