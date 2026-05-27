using UnityEngine;
using UnityEngine.SceneManagement;
using Util;
using System.IO;

namespace StateManager
{
    public class GameOverState : GameState
    {
        public GameObject window;

        [SerializeField] private Vector3 visiblePosition = Vector3.zero;
        [SerializeField] private Vector3 hiddenPosition = new Vector3(0f, 730f, 0f);

        public override void Enter()
        {
            MoveWindow(visiblePosition);
        }

        public override void Exit()
        {
            MoveWindow(hiddenPosition);
        }

        public void GoToTitleScreen()
        {
            DeleteSave();
            SceneManager.LoadScene("Scenes/MainMenu");
        }

        public void GoToMainMenu()
        {
            GoToTitleScreen();
        }

        private void MoveWindow(Vector3 targetPosition)
        {
            if (window == null)
                return;

            if (window.TryGetComponent(out EasePosition easePosition))
            {
                easePosition.SendToLocation(targetPosition);
                return;
            }

            if (window.TryGetComponent(out LerpPosition lerpPosition))
            {
                lerpPosition.targetLocation = targetPosition;
                return;
            }

            window.transform.localPosition = targetPosition;
        }

        private void DeleteSave()
        {
            string savePath = Path.Combine(Application.persistentDataPath, "save1.json");

            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                Debug.Log("Save file deleted: " + savePath);
            }
        }
    }
}
