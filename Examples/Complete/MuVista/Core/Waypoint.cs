using Fusee.Base.Common;
using Fusee.Engine.Core;
using Fusee.Engine.Core.Scene;
using Fusee.Math.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusee.Examples.MuVista.Core
{
    public class Waypoint : SceneNode
    {
        private PanoSphere _sphere;
        public Waypoint(PanoSphere sphere)
        {
            Name = "Waypoint";
            float3 sphereFuseeTransform = new float3(sphere.GetTransform(0).Translation.x, sphere.GetTransform(0).Translation.z, sphere.GetTransform(0).Translation.y);
            Components = new List<SceneComponent>
            {
                    new Transform { Translation = sphereFuseeTransform, Scale = new float3(1, 1, 1) },
                    MakeEffect.FromDiffuseSpecular((float4) ColorUint.Green, 0f, 4.0f, 1f),
                    new Sphere(0.3f, 20, 50)
            };
            _sphere = sphere;
        }

        public PanoSphere GetPanoSphere()
        {
            return _sphere;
        }
    }
}
