using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RT_Rimtroid
{
	public enum TileState
    {
		StartingCell,
		Cell,
		Empty
    }
	public class ExpandableProjectileShapeDef : Def
	{
		public List<string> shape;
        public SimpleCurve widthCurve;
	}
    public class TileShape
    {
        public Rot4 rotation;
        public CellRect cellRect;
        public Map map;
        public Dictionary<IntVec3, TileState> roomContents;
        public Dictionary<int, List<IntVec3>> cellsX;
        public Dictionary<int, List<IntVec3>> cellsZ;
        public List<KeyValuePair<int, List<IntVec3>>> curRotatedRoom;
        public ExpandableProjectileShapeDef def;
        public TileShape(ExpandableProjectileShapeDef def, CellRect cellRect, Rot4 rotation, Map map)
        {
            this.rotation = rotation;
            this.cellRect = cellRect;
            this.def = def;
            this.map = map;
            PreInit();
        }
        public void PreInit()
        {
            this.roomContents = new Dictionary<IntVec3, TileState>();
            this.cellsX = new Dictionary<int, List<IntVec3>>();
            this.cellsZ = new Dictionary<int, List<IntVec3>>();
            foreach (var cell in cellRect.Cells)
            {
                if (cellsX.ContainsKey(cell.x))
                {
                    cellsX[cell.x].Add(cell);
                }
                else
                {
                    cellsX[cell.x] = new List<IntVec3> { cell };
                }
            }

            foreach (var cell in cellRect.Cells)
            {
                if (cellsZ.ContainsKey(cell.z))
                {
                    cellsZ[cell.z].Add(cell);
                }
                else
                {
                    cellsZ[cell.z] = new List<IntVec3> { cell };
                }
            }
            curRotatedRoom = RotatedBy(rotation);
            FillRoomData();
        }
        public void MoveRoom(IntVec2 offset)
        {
            this.cellRect = this.cellRect.MovedBy(offset);
            PreInit();
        }

        public void RotateRoom(RotationDirection rotationDirection)
        {
            var prevRotation = this.rotation;
            this.rotation.Rotate(rotationDirection);
            if ((rotation == Rot4.East || rotation == Rot4.West) && (prevRotation == Rot4.South || prevRotation == Rot4.North))
            {
                this.cellRect = new CellRect(cellRect.minX, cellRect.minZ, cellRect.Height, cellRect.Width);
            }
            else if ((rotation == Rot4.North || rotation == Rot4.South) && (prevRotation == Rot4.East || prevRotation == Rot4.West))
            {
                this.cellRect = new CellRect(cellRect.minX, cellRect.minZ, cellRect.Height, cellRect.Width);
            }
            PreInit();
        }
        public List<KeyValuePair<int, List<IntVec3>>> RotatedBy(Rot4 rotation)
        {
            if (rotation == Rot4.North)
            {
                return cellsZ.OrderBy(x => x.Key).ToList();
            }
            else if (rotation == Rot4.South)
            {
                return cellsZ.OrderByDescending(x => x.Key).ToList();
            }
            else if (rotation == Rot4.West)
            {
                return cellsX.OrderByDescending(x => x.Key).ToList();
            }
            else
            {
                return cellsX.OrderBy(x => x.Key).ToList();
            }
        }

        public void FillRoomData()
        {
            for (int i = 0; i < curRotatedRoom.Count; i++)
            {
                for (int j = 0; j < curRotatedRoom[i].Value.Count; j++)
                {
                    var state = ResolveSymbol(this.def.shape[i][j]);
                    roomContents[curRotatedRoom[i].Value[j]] = state;
                }
            }
        }
        public Rot4 GetDirection(IntVec3 cell)
        {
            if (!cellRect.Contains(cell.North()))
            {
                return Rot4.North;
            }
            else if (!cellRect.Contains(cell.South()))
            {
                return Rot4.South;
            }
            else if (!cellRect.Contains(cell.West()))
            {
                return Rot4.West;
            }
            else
            {
                return Rot4.East;
            }
        }
        private TileState ResolveSymbol(char symbol)
        {
            switch (symbol)
            {
                case 'E': return TileState.StartingCell;
                case 'S': return TileState.Cell;
                case 'X': return TileState.Empty;
                default: return TileState.Empty;
            }
        }
    }
    public static class Utils
    {
        public static IntVec3 North(this IntVec3 intVec3)
        {
            return intVec3 + IntVec3.North;
        }

        public static IntVec3 South(this IntVec3 intVec3)
        {
            return intVec3 + IntVec3.South;
        }
        public static IntVec3 West(this IntVec3 intVec3)
        {
            return intVec3 + IntVec3.West;
        }
        public static IntVec3 East(this IntVec3 intVec3)
        {
            return intVec3 + IntVec3.East;
        }
    }
}
