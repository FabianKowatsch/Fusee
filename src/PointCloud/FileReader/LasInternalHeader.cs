﻿namespace Fusee.PointCloud.FileReader.LasReader
{
    internal struct LasInternalHeader
    {
        public byte PointDataFormat;

        public long PointCnt;

        public double ScaleFactorX;
        public double ScaleFactorY;
        public double ScaleFactorZ;

        public double OffsetX;
        public double OffsetY;
        public double OffsetZ;
    }
}