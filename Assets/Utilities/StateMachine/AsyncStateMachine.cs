using System.Threading.Tasks;
using UnityEngine;

namespace Volpi.Entertaiment.SDK.Utilities
{
    public class AsyncStateMachine : Singleton<AsyncStateMachine>
    {
        [SerializeField] private AsyncState _initialState;
        private AsyncState _currentState;

        private async void Start()
        {
            if (_initialState != null)
            {
                await ChangeState(_initialState);
            }
        }

        public async Task ChangeState(AsyncState newState)
        {
            if (_currentState != null)
            {
                await _currentState.ExitState();
            }

            _currentState = newState;

            if (_currentState != null)
            {
                await _currentState.EnterState();
                await _currentState.ExecuteState();
            }
        }
    }
}
