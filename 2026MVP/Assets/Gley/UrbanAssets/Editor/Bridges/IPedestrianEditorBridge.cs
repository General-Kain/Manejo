using System.Collections.Generic;
using UnityEngine;

namespace Gley.UrbanSystem.Editor
{
    public interface IPedestrianEditorBridge 
    {
        delegate void WaypointClicked(WaypointSettingsBase clickedWaypoint, bool leftClick);
        event WaypointClicked OnWaypointClicked;
        void TriggerWaypointClickedEvent(WaypointSettingsBase clickedWaypoint, bool leftClick);

        void AppendGroundLayers(ref LayerMask groundLayers);
        void ApplyWaypoints();

        List<WaypointSettingsBase> GetAllPedestrianWaypoints();

        public int[] GetWaypointIndices(List<WaypointSettingsBase> selectedWaypoints);

        void ShowIntersectionWaypoints(Color waypointColor);
        void DrawPossibleDirectionWaypoints(List<WaypointSettingsBase> waypoints, Color waypointColor);
    }
}
