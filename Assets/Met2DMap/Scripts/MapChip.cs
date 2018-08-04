using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UniRx;
using UniRx.Triggers;


namespace Met2DMap {
    public enum ItemState {
        NO,
        ACQUIRED,
        UNACQUIRED
    }

    [ExecuteInEditMode]
    public class MapChip : MonoBehaviour {
        // Edge width
        [SerializeField]
        protected int EdgeWidth = 2;

        // Reference to the edge image component
        [SerializeField]
        protected Image EdgeImage;

        // Reference to the inner image component
        [SerializeField]
        protected Image InsideImage;

        // Reference to the inner gameobject for acquired item
        [SerializeField]
        protected GameObject AcquiredItemImage;

        // Reference to the inner gameobject for unacquired item
        [SerializeField]
        protected GameObject UnacquiredItemImage;

        [SerializeField]
        protected ItemState ItemState;

        // Shape type of this chip
        [SerializeField]
        public ShapeType Shape;

        private RectTransform rectTransform;
        public RectTransform RectTransform {
            get {
                return rectTransform ? rectTransform : rectTransform = GetComponent<RectTransform>();
            }
        }

        public Vector2 GridCellSize
        {
            get {
                return RectTransform.sizeDelta;
            }
        }

        private void Awake() {
            if ( !EdgeImage )
                EdgeImage = GetComponent<Image>();
        }

        private void Start() {
            this.ObserveEveryValueChanged(_ => Shape)
                .Subscribe(s => OnChangeShape(s))
                .AddTo(this);

            this.ObserveEveryValueChanged(_ => ItemState)
                .Subscribe(state => {
                    switch ( state ) {
                        case ItemState.ACQUIRED: {
                                if ( AcquiredItemImage )
                                    AcquiredItemImage.SetActive(true);
                                if ( UnacquiredItemImage )
                                    UnacquiredItemImage.SetActive(false);
                                break;
                            }
                        case ItemState.UNACQUIRED: {
                                if ( AcquiredItemImage )
                                    AcquiredItemImage.SetActive(false);
                                if ( UnacquiredItemImage )
                                    UnacquiredItemImage.SetActive(true);
                                break;
                            }
                        case ItemState.NO: {
                                if ( AcquiredItemImage )
                                    AcquiredItemImage.SetActive(false);
                                if ( UnacquiredItemImage )
                                    UnacquiredItemImage.SetActive(false);
                                break;
                            }
                    }
                })
                .AddTo(this);
        }

        protected void changeEdge(float left, float bottom, float right, float top) {
            InsideImage.rectTransform.offsetMin = new Vector2(left, bottom);
            InsideImage.rectTransform.offsetMax = new Vector2(-right, -top);
        }

        public virtual void OnChangeShape(ShapeType shape) {
            switch ( shape ) {
                case ShapeType.WHOLE:
                    changeEdge(0, 0, 0, 0); break;
                case ShapeType.SQUARE:
                    changeEdge(EdgeWidth, EdgeWidth, EdgeWidth, EdgeWidth); break;
                case ShapeType.LEFT_EDGE:
                    changeEdge(EdgeWidth, 0, 0, 0); break;
                case ShapeType.RIGHT_EDGE:
                    changeEdge(0, 0, EdgeWidth, 0); break;
                case ShapeType.BOTTOM_EDGE:
                    changeEdge(0, EdgeWidth, 0, 0); break;
                case ShapeType.TOP_EDGE:
                    changeEdge(0, 0, 0, EdgeWidth); break;
                case ShapeType.LEFT_RIGHT_EDGE:
                    changeEdge(EdgeWidth, 0, EdgeWidth, 0); break;
                case ShapeType.TOP_BOTTOM_EDGE:
                    changeEdge(0, EdgeWidth, 0, EdgeWidth); break;
                case ShapeType.LEFT_CORNER:
                    changeEdge(EdgeWidth, EdgeWidth, 0, EdgeWidth); break;
                case ShapeType.RIGHT_CORNER:
                    changeEdge(0, EdgeWidth, EdgeWidth, EdgeWidth); break;
                case ShapeType.BOTTOM_CORNER:
                    changeEdge(EdgeWidth, EdgeWidth, EdgeWidth, 0); break;
                case ShapeType.TOP_CORNER:
                    changeEdge(EdgeWidth, 0, EdgeWidth, EdgeWidth); break;
                case ShapeType.LEFT_TOP_CORNER:
                    changeEdge(EdgeWidth, 0, 0, EdgeWidth); break;
                case ShapeType.LEFT_BOTTOM_CORNER:
                    changeEdge(EdgeWidth, EdgeWidth, 0, 0); break;
                case ShapeType.RIGHT_TOP_CORNER:
                    changeEdge(0, 0, EdgeWidth, EdgeWidth); break;
                case ShapeType.RIGHT_BOTTOM_CORNER:
                    changeEdge(0, EdgeWidth, EdgeWidth, 0); break;
            }
        }
    }

}
