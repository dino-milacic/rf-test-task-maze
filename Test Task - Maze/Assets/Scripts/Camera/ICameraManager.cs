using UnityEngine;

namespace RFTestTaskMaze
{
    public interface ICameraManager : IGameService
    {
        Camera Camera { get; }
        void Initialize(float minSize, float maxSize, Rect field);
        void SetTarget(Vector2 targetPos);
        void SetZoom(float zoom);
        void Recenter();
    }
}