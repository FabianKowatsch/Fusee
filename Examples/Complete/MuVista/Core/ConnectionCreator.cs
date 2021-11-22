using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Engine.Core.Effects;
using Fusee.Engine.Core.Scene;
using Fusee.Engine.Core.ShaderShards;
using Fusee.Engine.Core.ShaderShards.Fragment;
using Fusee.Engine.Core.ShaderShards.Vertex;
using Fusee.Engine.GUI;
using Fusee.Math.Core;
using Fusee.PointCloud.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Fusee.Examples.MuVista.Core
{
    class ConnectionCreator
    {
        private Texture activePano;
        private float3 thisPosition;
        private float acceptedRadius = 10;
        private Texture[] activeConnections;
        private float3[] activeConPos;

        private static ConnectionCreator instance = null;


        public static ConnectionCreator Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ConnectionCreator();
                }
                instance.readInfoJson();
                return instance;
            }
        }

        public void readInfoJson()
        {
            //D:\Studium\4._Semester\Projektmodul\test\Fusee\Examples\Complete\MuVista\Core\ConnectionCreator.cs
            //D:\Studium\4._Semester\Projektmodul\test\Fusee\Examples\Complete\MuVista\Core\input\data.json

        }

        /*public void showAllItems()
        {
            for (int i = 0; i < allObjects.Count; i++)
            {
                Diagnostics.Debug(allObjects[i]["filename"] + "|" + allObjects[i]["X"]);
            }
        }*/


        public void clickedConnection(PickResult _picked)
        {
            /*if(Mouse.LeftButton)*/

            if ((bool)_picked?.Node.Name.Contains("_connection"))
            {
                Diagnostics.Debug(_picked?.Node.Name);
            }
        }

        public void changeActivePosition(Texture _newTex, float3 _newPos)
        {
            this.thisPosition = _newPos;
            this.activePano = _newTex;
        }

        public ChildList getAllConnections(String imageName)
        {
            ChildList result = new ChildList();
            List<PanoImage> panoImages = PanoSphereFactory.readJSONImageData();
            PanoImage thisImage = null;
            foreach (PanoImage panoImage in panoImages)
            {
                if (panoImage.filename == imageName)
                {
                    thisImage = panoImage;
                }
            }

            foreach (PanoImage panoImage in panoImages)
            {
                if (panoImage.filename != imageName)
                {
                    float3 connectionVektor = new float3((float)(panoImage.X - thisImage.X), (float)(panoImage.Y - thisImage.Y), (float)(panoImage.Z - thisImage.Z));
                    /*if(MathF.Sqrt(connectionVektor.x*connectionVektor.x + connectionVektor.y * connectionVektor.y + connectionVektor.z * connectionVektor.z) < 100)
                    {
                        result.Add(this.createArrow(new float3(9,0,0), panoImage.filename + "_connection"));
                    }*/

                }
            }
            result.Add(this.createArrow(new float3(9, 0, 0), "test_connection"));
            return result;
        }

        public SceneNode createArrow(float3 pos, String imageName)
        {
            SceneContainer blenderScene = AssetStorage.Get<SceneContainer>("arrow2.fus");
            SceneNode arrow = blenderScene.Children[0];
            arrow.Name = "connection_" + imageName;
            arrow.GetComponent<Transform>(0).Translation = pos;
            arrow.GetComponent<Transform>(0).Rotation = new float3(-2, -2, 0);
            return arrow;
        }
    }
}