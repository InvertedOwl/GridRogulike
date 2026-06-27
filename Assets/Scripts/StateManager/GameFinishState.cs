using Serializer;
using UnityEngine;
using UnityEngine.SceneManagement;
using Util;

namespace StateManager
{
    public class GameFinishState : GameState
    {
        public GameObject window;

        [SerializeField] private Vector3 visiblePosition = Vector3.zero;
        [SerializeField] private Vector3 hiddenPosition = new Vector3(0f, 730f, 0f);

        public override void Enter()
        {
            PlayWindowInSound();
            MoveWindow(visiblePosition);
        }

        public override void Exit()
        {
            PlayWindowOutSound();
            MoveWindow(hiddenPosition);
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
            SaveGameObject.DeleteSaveFiles();
        }
    }
}
