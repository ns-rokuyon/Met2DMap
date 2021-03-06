﻿using UnityEngine;
using System.Collections;


namespace Met2DMap {
    public enum ShapeType {
        BLANK, SQUARE, WHOLE,
        LEFT_EDGE, RIGHT_EDGE, TOP_EDGE, BOTTOM_EDGE,
        LEFT_RIGHT_EDGE, TOP_BOTTOM_EDGE,
        LEFT_CORNER, RIGHT_CORNER, TOP_CORNER, BOTTOM_CORNER,
        LEFT_TOP_CORNER, LEFT_BOTTOM_CORNER, RIGHT_TOP_CORNER, RIGHT_BOTTOM_CORNER
    }
}