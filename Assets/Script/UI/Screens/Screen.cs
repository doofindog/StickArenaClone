using UnityEngine;

namespace PixelArena.UI
{
    public abstract class Screen : MonoBehaviour
    {
        public abstract void Init();
        public abstract void OnEnter();
        public abstract void OnExit();
    }
}
