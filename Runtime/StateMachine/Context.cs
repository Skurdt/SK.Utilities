/* MIT License

 * Copyright (c) 2020 Skurdt
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:

 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.

 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE. */

using System.Collections.Generic;

namespace SK.Utilities.StateMachine
{
    public abstract class Context<T> where T : State<T>
    {
        private readonly List<T> _states = new List<T>();

        private T _currentState  = null;
        private T _previousState = null;

        public void Start() => OnStart();

        public void Update(float dt)
        {
            OnUpdate(dt);
            _currentState?.OnUpdate(dt);
        }

        public void FixedUpdate(float dt)
        {
            OnFixedUpdate(dt);
            _currentState?.OnFixedUpdate(dt);
        }

        public void TransitionTo<U>() where U : T
        {
            T foundState = _states.Find(x => x.GetType() == typeof(U));
            if (foundState != null)
            {
                _currentState?.OnExit();
                _previousState = _currentState;
                _currentState = foundState;
                _currentState.OnEnter();
            }
            else
            {
                U newState = System.Activator.CreateInstance(typeof(U), this) as U;
                _states.Add(newState);
                TransitionTo<U>();
            }
        }

        public void TransitionToPrevious()
        {
            if (_previousState == null)
                return;

            _currentState.OnExit();
            Algorithms.Swap(ref _currentState, ref _previousState);
            _currentState.OnEnter();
        }

        protected virtual void OnStart()
        {
        }

        protected virtual void OnUpdate(float dt)
        {
        }

        protected virtual void OnFixedUpdate(float dt)
        {
        }
    }
}
