using UnityEngine;

namespace UnobservedRooms.Gameplay
{
    public sealed class SpinOrbit : MonoBehaviour
    {
        private Vector3 axis = Vector3.up;
        private float degreesPerSecond = 90f;
        public void Configure(Vector3 rotationAxis, float speed) { axis = rotationAxis; degreesPerSecond = speed; }
        private void Update() => transform.Rotate(axis, degreesPerSecond * Time.deltaTime, Space.Self);
    }
}
