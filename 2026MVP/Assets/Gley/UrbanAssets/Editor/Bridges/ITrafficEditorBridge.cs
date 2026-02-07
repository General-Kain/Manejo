using UnityEngine;

namespace Gley.UrbanSystem.Editor
{
    public interface ITrafficEditorBridge 
    {
        void AppendGroundLayers(ref LayerMask groundLayers);
        void ApplyWaypoints();
        void ConvertIntersections();
        IGenericIntersectionSettings[] GetAllIntersections();
    }
}
