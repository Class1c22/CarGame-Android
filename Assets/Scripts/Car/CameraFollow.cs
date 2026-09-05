using UnityEngine;

namespace CarTurretGame.Gameplay
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 8f, -6f);
        [SerializeField] private float followSmoothTime = 0.15f;
        [SerializeField] private bool followX = false;

        private float _velocityZ;
        private float _velocityX;
        private float _velocityY;

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 pos = transform.position;

            float targetZ = target.position.z + offset.z;
            float targetY = target.position.y + offset.y;

            pos.z = Mathf.SmoothDamp(pos.z, targetZ, ref _velocityZ, followSmoothTime);
            pos.y = Mathf.SmoothDamp(pos.y, targetY, ref _velocityY, followSmoothTime);

            if (followX)
            {
                float targetX = target.position.x + offset.x;
                pos.x = Mathf.SmoothDamp(pos.x, targetX, ref _velocityX, followSmoothTime);
            }

            transform.position = pos;
        }
    }
}