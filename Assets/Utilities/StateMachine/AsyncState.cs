using System.Threading.Tasks;
using UnityEngine;

namespace Volpi.Entertaiment.SDK.Utilities
{
    public abstract class AsyncState : ScriptableObject, IAsyncState
    {
        public abstract Task EnterState();
        public abstract Task ExecuteState();
        public abstract Task ExitState();
    }
}
