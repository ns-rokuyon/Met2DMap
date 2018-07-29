using UnityEngine;
using System.Collections;


namespace Met2DMap {
    // Sample room class
    [ExecuteInEditMode]
    public class Room : MonoBehaviour, IRoom {
        [SerializeField]
        private int index;

        [SerializeField]
        private int priority = 0;

        public int RoomIndex {
            get { return index; }
        }

        public int Priority {
            get { return priority; }
        }

        private MeshCollider meshCollider;
        public MeshCollider MeshCollider {
            get {
                return meshCollider ? meshCollider : meshCollider = GetComponent<MeshCollider>();
            }
        }

        public Vector2 Center {
            get { return transform.position; }
        }

        public Vector2 Size {
            get { return transform.localScale; }
        }

        public bool IsIn(Vector2 pos) {
            return IsIn(new Vector3(pos.x, pos.y, transform.position.z));
        }

        public bool IsIn(Vector3 pos) {
            return MeshCollider.bounds.Contains(pos);
        }
    }

    public interface IRoom {
        // Room index number
        int RoomIndex { get; }

        // Priority order
        int Priority { get; }

        // Center position in the world
        Vector2 Center { get; }

        // Width and height
        Vector2 Size { get; }

        // Return whether the given point is in room or not
        bool IsIn(Vector2 pos);
    }
}
