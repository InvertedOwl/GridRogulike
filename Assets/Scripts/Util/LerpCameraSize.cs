using UnityEngine;

namespace Util
{
    public class LerpCameraSize : MonoBehaviour
    {
        public float speed = 1;
        public float targetHeight = 4f;
        public Camera camera;
        
        void Update()
        {
            camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, targetHeight, speed * Time.deltaTime);
        }
    }
}
