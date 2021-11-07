using Fusee.Engine.Core.Scene;
using Fusee.Math.Core;
using Fusee.PointCloud.Common;
using Fusee.Structures;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace Fusee.Examples.MuVista.Core
{
    class OctreePanoSynchronizer
    {

        private readonly string _metaFolderPath;

        /// <summary>
        /// Creates a new instance of type PtOctantFileReader.
        /// </summary>
        /// <param name="pathToMetaFileFolder">The path the files are written to.</param>
        public OctreePanoSynchronizer(string pathToMetaFileFolder)
        {
            _metaFolderPath = pathToMetaFileFolder;
        }
        public double3[] GetMeta()
        {
            var pathToMetaJson = _metaFolderPath + "\\meta.json";
            JObject jsonObj;

            using (StreamReader sr = new StreamReader(pathToMetaJson))
            {
                jsonObj = (JObject)JToken.ReadFrom(new JsonTextReader(sr));
            }

            var jsonCenter = (JArray)jsonObj["octree"]["rootNode"]["center"];
            var center = new double3((double)jsonCenter[0], (double)jsonCenter[1], (double)jsonCenter[2]);
            var jsonOffsetX = (JValue)jsonObj["metaInfo"]["offsetX"];
            var jsonOffsetY = (JValue)jsonObj["metaInfo"]["offsetY"];
            var jsonOffsetZ = (JValue)jsonObj["metaInfo"]["offsetZ"];
            var offset = new double3((double)jsonOffsetX, (double)jsonOffsetZ, (double)jsonOffsetY);
            var jsonScaleX = (JValue)jsonObj["metaInfo"]["scaleX"];
            var jsonScaleY = (JValue)jsonObj["metaInfo"]["scaleY"];
            var jsonScaleZ = (JValue)jsonObj["metaInfo"]["scaleZ"];
            var scale = new double3((double)jsonScaleX, (double)jsonScaleZ, (double)jsonScaleY);

            double3[] array = new double3[3];
            array[0] = center;
            array[1] = offset;
            array[3] = scale;
            return array;
        }
    }
}
