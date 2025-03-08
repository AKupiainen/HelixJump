using System.Threading.Tasks;

namespace Volpi.Entertaiment.SDK.Utilities
{
    public interface IAsyncState
    {
        Task EnterState();
        Task ExecuteState();
        Task ExitState();
    }
}
