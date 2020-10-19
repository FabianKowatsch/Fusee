﻿using Fusee.Engine.Core;
using Fusee.Math.Core;
using System;
using System.Collections.Generic;

namespace Fusee.Pointcloud.OoCFileReaderWriter
{
    public class PtOctant<TPoint> : Octant<TPoint>
    {
        //The Resolution of an Octant is defined by the minimum distance (spacing) between points.
        //If the minimum distance between a point and its nearest neighbor is smaller then this distance, it will fall into a child octant.
        public double Resolution;

        public Guid Guid { get; set; }

        public PtOctant(double3 center, double size, Octant<TPoint>[] children = null)
        {
            Center = center;
            Size = size;

            if (children == null)
                Children = new Octant<TPoint>[8];
            else
                Children = children;

            Payload = new List<TPoint>();
        }
        protected PtOctant() {}

        public PtOctant<TPoint> CreateChild(int posInParent)
        {
            var childCenter = CalcCildCenterAtPos(posInParent);

            var childRes = Size / 2d;
            var child = new PtOctant<TPoint>(childCenter, childRes)
            {
                Resolution = Resolution / 2d,
                Level = Level + 1
            };
            return child;
        }

        internal double3 CalcCildCenterAtPos(int posInParent)
        {
            return CalcCildCenterAtPos(posInParent, Size, Center);
        }

        public static double3 CalcCildCenterAtPos(int posInParent, double parentSize, double3 parentCenter)
        {
            double3 childCenter;
            var childsHalfSize = parentSize / 4d;
            switch (posInParent)
            {
                default:
                case 0:
                    childCenter = new double3(parentCenter.x - childsHalfSize, parentCenter.y - childsHalfSize, parentCenter.z - childsHalfSize);
                    break;
                case 1:
                    childCenter = new double3(parentCenter.x + childsHalfSize, parentCenter.y - childsHalfSize, parentCenter.z - childsHalfSize);
                    break;
                case 2:
                    childCenter = new double3(parentCenter.x - childsHalfSize, parentCenter.y - childsHalfSize, parentCenter.z + childsHalfSize);
                    break;
                case 3:
                    childCenter = new double3(parentCenter.x + childsHalfSize, parentCenter.y - childsHalfSize, parentCenter.z + childsHalfSize);
                    break;
                case 4:
                    childCenter = new double3(parentCenter.x - childsHalfSize, parentCenter.y + childsHalfSize, parentCenter.z - childsHalfSize);
                    break;
                case 5:
                    childCenter = new double3(parentCenter.x + childsHalfSize, parentCenter.y + childsHalfSize, parentCenter.z - childsHalfSize);
                    break;
                case 6:
                    childCenter = new double3(parentCenter.x - childsHalfSize, parentCenter.y + childsHalfSize, parentCenter.z + childsHalfSize);
                    break;
                case 7:
                    childCenter = new double3(parentCenter.x + childsHalfSize, parentCenter.y + childsHalfSize, parentCenter.z + childsHalfSize);
                    break;
            }

            return childCenter;
        }
    }

    public class PtOctantWrite<TPoint> : PtOctant<TPoint>
    {
        public PtGrid<TPoint> Grid;

        public PtOctantWrite(double3 center, double size, Octant<TPoint>[] children = null)
        {
            Guid = Guid.NewGuid();

            Center = center;
            Size = size;

            if (children == null)
                Children = new Octant<TPoint>[8];
            else
                Children = children;

            Payload = new List<TPoint>();
        }

        public new PtOctantWrite<TPoint> CreateChild(int posInParent)
        {
            var childCenter = CalcCildCenterAtPos(posInParent);

            var childRes = Size / 2d;
            var child = new PtOctantWrite<TPoint>(childCenter, childRes)
            {
                Resolution = Resolution / 2d,
                Level = Level + 1
            };
            return child;
        }
    }
}
