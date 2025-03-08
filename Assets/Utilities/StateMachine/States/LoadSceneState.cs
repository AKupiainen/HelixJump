using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Volpi.Entertaiment.SDK.Utilities
{
    [CreateAssetMenu(fileName = "LoadSceneState", menuName = "FSM/States/LoadScene")]
    public class LoadSceneState : AsyncState
    {
        [SerializeField] private string _sceneName;

        public override async Task EnterState()
        {
            Debug.Log($"[FSM] Entering Load Scene State - Loading Scene: {_sceneName}");
            await Task.Yield();
        }

        public override async Task ExecuteState()
        {
            if (string.IsNullOrEmpty(_sceneName))
            {
                Debug.LogError("[FSM] Scene name is not set!");
                return;
            }

            Debug.Log($"[FSM] Loading Scene: {_sceneName}...");
            
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(_sceneName);
            asyncLoad.allowSceneActivation = false;

            while (!asyncLoad.isDone)
            {
                Debug.Log($"[FSM] Scene Loading Progress: {asyncLoad.progress * 100}%");

                if (asyncLoad.progress >= 0.9f)
                {
                    Debug.Log("[FSM] Scene Load Almost Complete...");
                    asyncLoad.allowSceneActivation = true;
                }

                await Task.Delay(100);
            }

            Debug.Log("[FSM] Scene Loaded Successfully!");
        }

        public override async Task ExitState()
        {
            Debug.Log("[FSM] Exiting Load Scene State...");
            await Task.Yield();
        }
    }
}