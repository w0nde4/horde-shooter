using UnityEngine;

namespace _Project.Scripts.Core.Data
{
    public sealed class RespawnCamera
    {
        public Camera Camera { get; }
        public AudioListener Listener { get; }

        public RespawnCamera(Camera camera)
        {
            Camera = camera;
            Listener = camera != null ? camera.GetComponent<AudioListener>() : null;
        }
    }
}