using UnityEngine;
using System.Collections.Generic;
using System.Linq;


namespace Met2DMap {
    public class MapGenerator : MonoBehaviour {
        // Target canvas to generate map
        [SerializeField]
        private Canvas canvas;

        // Root object which has rooms
        [SerializeField]
        private GameObject searchRoot;

        // Prefab of MapChip
        [SerializeField]
        private MapChip mapChipPrefab;

        // Cell size to quantize world coordinates to grid
        [SerializeField]
        private Vector2 worldCellSize = new Vector2(50.0f, 50.0f);

        // Outer margin length of world coordinates
        [SerializeField]
        private Vector2 worldMargin = new Vector2(100.0f, 100.0f);

        // If true, Setup() and Generate() will be called in Start()
        [SerializeField]
        private bool enableToGenerateOnStart;

        // All rooms this generator handles
        public Dictionary<int, IRoom> Rooms { get; private set; }

        // World size
        public Vector2 WorldSize { get; private set; }

        // Grid size of MapChip matrix
        public Vector2 GridSize { get; private set; }

        // Edge point in the world coordinates
        public float LeftEdge { get; private set; }
        public float RightEdge { get; private set; }
        public float TopEdge { get; private set; }
        public float BottomEdge { get; private set; }

        // Steps to quantize world coordinates
        public List<float> XGridSteps { get; private set; }
        public List<float> YGridSteps { get; private set; }

        // World cell center points
        public Vector2[][] WorldCellPoints { get; private set; }

        // Map matrix array
        // (row, col)=(0, 0): Left bottom, (row, col)=(NRow-1, NCol-1): Right top
        public MapCell[][] MapGrid { get; private set; }
        public int[][] RoomMatrix { get; private set; }

        // Cell groups aggregated by room index
        public Dictionary<int, List<MapCell>> CellGroups { get; private set; }

        // Grid cell size
        public Vector2 CellSize {
            get { return mapChipPrefab.GridCellSize; }
        }

        // Alias
        public int NRow { get { return (int)GridSize.y; } }
        public int NCol { get { return (int)GridSize.x; } }

        private void Start() {
            if ( enableToGenerateOnStart ) {
                Setup();
                Generate();
            }
        }

        // Setup generator
        public void Setup() {
            Rooms = new Dictionary<int, IRoom>();

            // Get all rooms in SearchRoot
            searchRoot.GetComponentsInChildren<IRoom>().ToList().ForEach(room => {
                Rooms[room.RoomIndex] = room;
            });

            if ( Rooms.Count == 0 )
                return;

            // Edge point in the world
            LeftEdge = Rooms.Values.ToList().Select(r => (r.Center - r.Size / 2.0f).x).Min() - worldMargin.x;
            RightEdge = Rooms.Values.ToList().Select(r => (r.Center + r.Size / 2.0f).x).Max() + worldMargin.x;
            TopEdge = Rooms.Values.ToList().Select(r => (r.Center + r.Size / 2.0f).y).Max() + worldMargin.y;
            BottomEdge = Rooms.Values.ToList().Select(r => (r.Center - r.Size / 2.0f).y).Min() - worldMargin.y;

            // World size
            WorldSize = new Vector2(RightEdge - LeftEdge, TopEdge - BottomEdge);

            // Grid size
            GridSize = new Vector2(Mathf.Ceil(WorldSize.x / worldCellSize.x),
                                   Mathf.Ceil(WorldSize.y / worldCellSize.y));

            // Grid steps
            XGridSteps = new List<float>();
            for ( int i = 0; i < NCol; i++ ) {
                if ( i == GridSize.x - 1 )
                    XGridSteps.Add(RightEdge);
                else
                    XGridSteps.Add(LeftEdge + worldCellSize.x * (i + 1));
            }

            YGridSteps = new List<float>();
            for ( int i = 0; i < NRow; i++ ) {
                if ( i == GridSize.y - 1 )
                    YGridSteps.Add(TopEdge);
                else
                    YGridSteps.Add(BottomEdge + worldCellSize.y * (i + 1));
            }

            // Initialize world cell points
            WorldCellPoints = new Vector2[NRow][];
            for ( int j = 0; j < NRow; j++ ) {
                WorldCellPoints[j] = new Vector2[NCol];
                for ( int i = 0; i < NCol; i++ ) {
                    WorldCellPoints[j][i] = Grid2SampledWorldPoint(j, i);
                }
            }

            // Initialize map grid
            MapGrid = new MapCell[NRow][];
            RoomMatrix = new int[NRow][];
            for ( int j = 0; j < NRow; j++ ) {
                MapGrid[j] = new MapCell[NCol];

                RoomMatrix[j] = new int[NCol];
                for ( int i = 0; i < NCol; i++ ) {
                    RoomMatrix[j][i] = -1;
                    foreach ( IRoom room in Rooms.Values.OrderByDescending(r => r.Priority) ) {
                        if ( room.IsIn(WorldCellPoints[j][i]) ) {
                            RoomMatrix[j][i] = room.RoomIndex;
                            break;
                        }
                    }
                }
            }

            CellGroups = new Dictionary<int, List<MapCell>>();
        }

        public void Generate() {
            if ( Rooms == null )
                Setup();

            if ( Rooms.Count == 0 ) {
                Debug.LogWarning("MapGenerator has no rooms");
                return;
            }

            // Create MapChip
            for ( int j = 0; j < NRow; j++ ) {
                for ( int i = 0; i < NCol; i++ ) {
                    int index = RoomMatrix[j][i];
                    Debug.Log("Rooms index = " + index);
                    if ( index < 0 )
                        continue;

                    MapChip chip = Instantiate(mapChipPrefab);
                    chip.transform.SetParent(canvas.transform, false);
                    chip.RectTransform.anchoredPosition = new Vector2(i * CellSize.x, j * CellSize.y);

                    MapCell cell = new MapCell(chip, index, j, i);
                    MapGrid[j][i] = cell;

                    if ( !CellGroups.ContainsKey(index) )
                        CellGroups[index] = new List<MapCell>();
                    CellGroups[index].Add(cell);
                }
            }

            // Determine shape
            for ( int j = 0; j < NRow; j++ ) {
                for ( int i = 0; i < NCol; i++ ) {
                    int index = RoomMatrix[j][i];
                    if ( index < 0 )
                        continue;

                    MapGrid[j][i].Shape = GuessShapeType(j, i);
                }
            }
        }

        public ShapeType GuessShapeType(int row, int col) {
            int index = RoomMatrix[row][col];
            if ( index < 0 ) {
                return ShapeType.BLANK;
            }

            int[] ns = GetConnectedNeighbors(row, col);
            int nConnect = ns.Count(x => x == index);
            if ( nConnect == 0 ) {
                return ShapeType.SQUARE;
            }
            else if ( nConnect == 1 ) {
                if ( ns[0] == index )
                    return ShapeType.BOTTOM_CORNER;
                else if ( ns[1] == index )
                    return ShapeType.RIGHT_CORNER;
                else if ( ns[2] == index )
                    return ShapeType.LEFT_CORNER;
                else
                    return ShapeType.TOP_CORNER;
            }
            else if ( nConnect == 2 ) {
                if ( ns[0] == index && ns[1] == index )
                    return ShapeType.RIGHT_BOTTOM_CORNER;
                else if ( ns[0] == index && ns[2] == index )
                    return ShapeType.LEFT_BOTTOM_CORNER;
                else if ( ns[3] == index && ns[1] == index )
                    return ShapeType.RIGHT_TOP_CORNER;
                else if ( ns[3] == index && ns[2] == index )
                    return ShapeType.LEFT_TOP_CORNER;
                else if ( ns[0] == index && ns[3] == index )
                    return ShapeType.LEFT_RIGHT_EDGE;
                else
                    return ShapeType.TOP_BOTTOM_EDGE;
            }
            else if ( nConnect == 3 ) {
                if ( ns[0] != index )
                    return ShapeType.TOP_EDGE;
                else if ( ns[1] != index )
                    return ShapeType.LEFT_EDGE;
                else if ( ns[2] != index )
                    return ShapeType.RIGHT_EDGE;
                else
                    return ShapeType.BOTTOM_EDGE;
            }
            else {
                return ShapeType.WHOLE;
            }
        }

        public int World2GridX(Vector3 pos) {
            int col = XGridSteps.FindIndex(stepX => stepX - worldCellSize.x <= pos.x && pos.x <= stepX);
            return col;
        }

        public int World2GridY(Vector3 pos) {
            int row = YGridSteps.FindIndex(stepY => stepY - worldCellSize.y <= pos.y && pos.y <= stepY);
            return row;
        }

        public Vector2 Grid2SampledWorldPoint(int row, int col) {
            return new Vector2(LeftEdge + col * worldCellSize.x + worldCellSize.x / 2,
                               BottomEdge + row * worldCellSize.y + worldCellSize.y / 2);
        }

        private int [] GetConnectedNeighbors(int row, int col) {
            /* Get top, left, right and bottom neighbors at (row, col)
                 |   0   |
                 | 1   2 |
                 |   3   |
            */
            return new int[4] {
                RoomMatrix[row + 1][col],
                RoomMatrix[row][col - 1],
                RoomMatrix[row][col + 1],
                RoomMatrix[row - 1][col]
            };
        }

        private int[] GetNeighbors(int row, int col) {
            /* Get around neighbors at (row, col)
                 | 0 1 2 |
                 | 3 4 5 |
                 | 6 7 8 |
            */
            return new int[9] {
                RoomMatrix[row + 1][col - 1], RoomMatrix[row + 1][col], RoomMatrix[row + 1][col + 1],
                RoomMatrix[row][col - 1], RoomMatrix[row][col], RoomMatrix[row][col + 1],
                RoomMatrix[row - 1][col - 1], RoomMatrix[row - 1][col], RoomMatrix[row - 1][col + 1]
            };
        }
    }


    public class MapCell {
        // MapChip object corresponding to this cell
        public MapChip Chip { get; private set; }

        // Room index
        public int RoomId { get; private set; }

        // Coordinates
        public int Row { get; private set; }
        public int Col { get; private set; }

        // Cell Shape
        public ShapeType Shape {
            get { return Chip.Shape; }
            set { Chip.Shape = value; }
        }

        public MapCell(MapChip chip, int roomId, int row, int col) {
            Chip = chip;
            RoomId = roomId;
            Row = row;
            Col = col;
        }
    }

}
