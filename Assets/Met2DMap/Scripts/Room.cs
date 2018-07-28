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

        private Bounds boundingBox;
        public Bounds BoundingBox {
            get {
                if ( boundingBox != null )
                    return boundingBox;
                MeshFilter mf = GetComponent<MeshFilter>();
                boundingBox = mf.sharedMesh.bounds;
                return boundingBox;
            }
        }

        public Vector2 Center {
            get { return transform.position + BoundingBox.center; }
        }

        public Vector2 Size {
            get { return Vector3.Scale(transform.localScale, BoundingBox.size); }
        }

        public bool IsIn(Vector2 pos) {
            return IsIn(new Vector3(pos.x, pos.y, transform.position.z));
        }

        public bool IsIn(Vector3 pos) {
            if ( MeshCollider ) {
                Debug.Log("IsIn: " + pos);
                return MeshCollider.bounds.Contains(pos);
            }

            float harfX = BoundingBox.extents.x;
            float harfY = BoundingBox.extents.y;
            float harfZ = BoundingBox.extents.z;
            Vector3 areaCenter = transform.position;
            if ( areaCenter.x - harfX > pos.x || areaCenter.x + harfX < pos.x ) {
                return false;
            }
            if ( areaCenter.y - harfY > pos.y || areaCenter.y + harfY < pos.y ) {
                return false;
            }
            if ( areaCenter.z - harfZ > pos.z || areaCenter.z + harfZ < pos.z ) {
                return false;
            }
            return true;
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

    public interface IRoom3D : IRoom {
        Bounds BoundingBox { get; }
    }

}
