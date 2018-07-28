using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;


namespace Met2DMap {
    [CustomEditor(typeof(MapGenerator), true)]
    public class MapGeneratorEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            MapGenerator generator = target as MapGenerator;

            if ( GUILayout.Button("Generate") ) {
                // Generate map objects to canvas
                generator.Setup();
                generator.Generate();

                if ( generator.Rooms.Count > 0 )
                    generator.MapGrid.ToList().ForEach(mapCols => {
                        mapCols.Where(cell => cell != null).ToList().ForEach(cell => EditorUtility.SetDirty(cell.Chip));
                    });
            }

            if ( GUILayout.Button("Clear") ) {
                // Remove map objects from canvas
                var chips = FindObjectsOfType<MapChip>();
                foreach ( var chip in chips ) {
                    DestroyImmediate(chip.gameObject);
                }
            }
        }
    }
}
