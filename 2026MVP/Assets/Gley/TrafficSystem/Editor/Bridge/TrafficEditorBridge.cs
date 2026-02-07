using Gley.UrbanSystem;
using Gley.UrbanSystem.Editor;
using UnityEditor;
using UnityEngine;

namespace Gley.TrafficSystem.Editor
{
    public class TrafficEditorBridge : ITrafficEditorBridge
    {
        public void AppendGroundLayers(ref LayerMask groundLayers)
        {
            var layers = FileCreator.LoadOrCreateLayers<LayerSetup>(
                TrafficSystemConstants.layerPath);
            if (layers != null)
            {
                groundLayers |= layers.roadLayers;
            }
        }

        public void ApplyWaypoints()
        {
            var converter = new TrafficWaypointsConverter();
            converter.ConvertWaypoints();
        }

        public void ConvertIntersections()
        {
            var converter = new IntersectionConverter();
            converter.ConvertAllIntersections();
        }

        public IGenericIntersectionSettings[] GetAllIntersections()
        {
            return new IntersectionEditorData().GetAllIntersections();
        }
    }
    [InitializeOnLoad]
    public static class TrafficEditorBridgeInitializer
    {
        static TrafficEditorBridgeInitializer()
        {
            TrafficEditorBridgeRegistry.Register (new TrafficEditorBridge());
        }
    }
}
