﻿using Fusee.Engine.Core;
using Fusee.Math.Core;
using Fusee.Pointcloud.Common;
using Fusee.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using Fusee.Xene;
using Fusee.Engine.Core.Scene;

namespace Fusee.Pointcloud.OoCFileReaderWriter
{
    public class PtOctreeFileReader<TPoint>
    {
        private readonly string _fileFolderPath;

        public int NumberOfOctants { get; private set; }

        /// <summary>
        /// Creates a new instance of type PtOctantFileReader.
        /// </summary>
        /// <param name="pathToNodeFileFolder">The path the files are written to.</param>
        public PtOctreeFileReader(string pathToNodeFileFolder)
        {
            _fileFolderPath = pathToNodeFileFolder;
        }

        /// <summary>
        /// Reads the meta.json and .hierarchy files and returns an octree.
        /// </summary>
        /// <param name="ptAccessor">Point accessor to get the actual point information.</param>
        /// <returns></returns>
        public PtOctree<TPoint> GetOctree(PointAccessor<TPoint> ptAccessor)
        {
            var pathToMetaJson = _fileFolderPath + "\\meta.json";
            JObject jsonObj;

            using (StreamReader sr = new StreamReader(pathToMetaJson))
            {
                jsonObj = (JObject)JToken.ReadFrom(new JsonTextReader(sr));
            }

            var jsonCenter = (JArray)jsonObj["octree"]["rootNode"]["center"];
            var center = new double3((double)jsonCenter[0], (double)jsonCenter[1], (double)jsonCenter[2]);
            var jsonSize = (JValue)jsonObj["octree"]["rootNode"]["size"];
            var size = (double)jsonSize;
            var jsonNoOfPts = (JValue)jsonObj["octree"]["maxNoOfPointsInBucket"];
            var maxNoOfPointsInBucket = (int)jsonNoOfPts;
            var jsonMaxLvl = (JValue)jsonObj["octree"]["maxLevel"];
            var maxLvl = (int)jsonMaxLvl;


            var root = new PtOctant<TPoint>(center, size);
            var octree = new PtOctree<TPoint>(root, ptAccessor, maxNoOfPointsInBucket)
            {
                MaxLevel = maxLvl
            };

            ReadHierarchy(octree);
            return octree;
        }

        /// <summary>
        /// Reads the meta.json and .hierarchy files and returns an octree.
        /// </summary>
        /// <param name="effect">Shader effect the points shall be rendered with.</param>
        /// <returns></returns>
        public SceneNode GetScene(ShaderEffect effect)
        {
            var pathToMetaJson = _fileFolderPath + "\\meta.json";
            JObject jsonObj;

            using (StreamReader sr = new StreamReader(pathToMetaJson))
            {
                jsonObj = (JObject)JToken.ReadFrom(new JsonTextReader(sr));
            }

            var jsonCenter = (JArray)jsonObj["octree"]["rootNode"]["center"];
            var center = new double3((double)jsonCenter[0], (double)jsonCenter[1], (double)jsonCenter[2]);
            var jsonSize = (JValue)jsonObj["octree"]["rootNode"]["size"];
            var size = (double)jsonSize;
            //var jsonNoOfPts = (JValue)jsonObj["octree"]["maxNoOfPointsInBucket"];
            //var maxNoOfPointsInBucket = (int)jsonNoOfPts;
            //var jsonMaxLvl = (JValue)jsonObj["octree"]["maxLevel"];
            //var maxLvl = (int)jsonMaxLvl;

            var rootSnc = new SceneNode
            {
                Components = new List<SceneComponent>
                {
                    new Transform
                    {
                        Scale = float3.One,
                        //Translation = (float3) center
                    },
                    effect,
                    new OctantComponent() 
                    { 
                        Octant = new OctantD()
                        {
                            PosInParent = -1, //root!
                            Center = center,
                            Size = size,
                            //Resolution = size/128
                        } 
                    }

                }
            };
            //scene.Children.Add(rootSnc);
            NumberOfOctants++;

            ReadHierarchyToScene(rootSnc, effect);
            return rootSnc;
        }

        /// <summary>
        /// Creates the octree hierarchy structure by reading in the .hierarchy file.
        /// </summary>
        private void ReadHierarchy(PtOctree<TPoint> octree)
        {
            var pathToHierarchy = _fileFolderPath + "\\octree.hierarchy";

            FileStream fileStream = File.Open(pathToHierarchy, FileMode.Open, FileAccess.Read);

            using (BinaryReader br = new BinaryReader(fileStream))
            {
                CreateNode(octree.Root, br);
            }

            fileStream.Dispose();
        }

        /// <summary>
        /// Creates the scene structure by reading in the octree.hierarchy file.
        /// </summary>
        private void ReadHierarchyToScene(SceneNode rootSnc, ShaderEffect effect)
        {
            var pathToHierarchy = _fileFolderPath + "\\octree.hierarchy";

            FileStream fileStream = File.Open(pathToHierarchy, FileMode.Open, FileAccess.Read);

            using (BinaryReader br = new BinaryReader(fileStream))
            {
                CreateSceneNode(rootSnc, effect, br);
            }

            fileStream.Dispose();
        }

        /// <summary>
        /// Processes the current node with the given set of children as bit set.
        /// </summary>
        /// <param name="node">The current node to process.</param>
        /// <param name="binaryReader">The binary reader to read bytes from. A byte indicating which of the given node's children exist.</param>
        private void CreateSceneNode(SceneNode nodeSnc, ShaderEffect effect, BinaryReader binaryReader)
        {
            try
            {
                var octantcomp = nodeSnc.GetComponent<OctantComponent>();

                // loadable properties
                byte[] guidBytes = new byte[16];
                binaryReader.Read(guidBytes, 0, 16);
                octantcomp.Guid = new Guid(guidBytes);
                octantcomp.Octant.Level = binaryReader.ReadInt32();                
                octantcomp.Octant.IsLeaf = binaryReader.ReadBoolean();

                //node.StreamPosition = binaryReader.ReadInt64();

                // create children
                byte children = binaryReader.ReadByte();

                for (byte index = 0; index < 8; index++)
                {
                    bool childExists = (children & (1 << index)) != 0;

                    if (childExists)
                    {
                        var childSnc = CreateSncForChildNode(index);
                        var childOctantComp = childSnc.GetComponent<OctantComponent>();
                        childOctantComp.Octant.Size = octantcomp.Octant.Size / 2;
                        childOctantComp.Octant.Center = PtOctant<TPoint>.CalcCildCenterAtPos(index, octantcomp.Octant.Size, octantcomp.Octant.Center);
                        nodeSnc.Children.Add(childSnc);
                        NumberOfOctants++;

                        CreateSceneNode(childSnc, effect, binaryReader);
                    }
                }
            }
            catch (EndOfStreamException e)
            {

            }
        }

        private SceneNode CreateSncForChildNode(int posInParent)
        {
            return new SceneNode
            {
                Components = new List<SceneComponent>
                {
                    new OctantComponent()
                    {
                        Octant = new OctantD()
                        {
                            PosInParent = posInParent 
                        }
                    },
                    new Transform
                    {
                        Scale = float3.One /** (float)(node.Size / parentSize)*/,
                        //Translation = (float3) (node.Center - parentCenter)
                    },
                }
            };
        }

        /// <summary>
        /// Processes the current node with the given set of children as bit set.
        /// </summary>
        /// <param name="node">The current node to process.</param>
        /// <param name="binaryReader">The binary reader to read bytes from. A byte indicating which of the given node's children exist.</param>
        private void CreateNode(PtOctant<TPoint> node, BinaryReader binaryReader)
        {
            try
            {
                // loadable properties
                byte[] guidBytes = new byte[16];
                binaryReader.Read(guidBytes, 0, 16);
                node.Guid = new Guid(guidBytes);
                node.Level = binaryReader.ReadInt32();
                node.Resolution = binaryReader.ReadDouble();
                node.IsLeaf = binaryReader.ReadBoolean();
                //node.StreamPosition = binaryReader.ReadInt64();

                // create children
                byte children = binaryReader.ReadByte();

                for (byte index = 0; index < 8; index++)
                {
                    bool childExists = (children & (1 << index)) != 0;

                    if (childExists)
                    {
                        PtOctant<TPoint> child = node.CreateChild(index);
                        node.Children[index] = child;
                        CreateNode(child, binaryReader);
                    }
                }
            }
            catch (EndOfStreamException e)
            {

            }
        }
    }
}
