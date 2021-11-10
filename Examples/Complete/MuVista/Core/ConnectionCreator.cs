using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Engine.Core.Effects;
using Fusee.Engine.Core.Scene;
using Fusee.Engine.Core.ShaderShards;
using Fusee.Engine.Core.ShaderShards.Fragment;
using Fusee.Engine.Core.ShaderShards.Vertex;
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
        private String pathToInfoJson = "D:/Studium/4._Semester/Projektmodul/test/Fusee/Examples/Complete/MuVista/Core/input/data.json";
        public List<JObject> allObjects;

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
            using (StreamReader sr = new StreamReader(pathToInfoJson))
            {
                string json = sr.ReadToEnd();
                allObjects = JsonConvert.DeserializeObject<List<JObject>>(json);
            }
        }

        public void showAllItems()
        {
            for (int i = 0; i < allObjects.Count; i++)
            {
                Diagnostics.Debug(allObjects[i]["filename"] + "|" + allObjects[i]["X"]);
            }
        }


        public void searchConnections()
        {
            /*float3 allPositions = fetch from text Datei
             * for(int i = 0; i < allPositions.count(); i++) {
             *  if((Math.pow(allPositions[i].x - thisPosition.x) + Math.pow(allPositions[i].y - thisPosition.y)) <= Math.pow(this.acceptedRadius))
             *  {
             *      this.activeConnections.push(allPositions[i]);
             *  }
             * }
            */
        }

        public void changeActivePosition(Texture _newTex, float3 _newPos)
        {
            this.thisPosition = _newPos;
            this.activePano = _newTex;
        }

        public void createArrows()
        {
            /*
             * creates all theSprites with the arrow Texture
             * Position is where the ray from the camera to the connection hits the sphere
             */
        }
    }
}