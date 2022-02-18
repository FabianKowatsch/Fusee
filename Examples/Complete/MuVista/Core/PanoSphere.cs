
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Engine.Core.Effects;
using Fusee.Engine.Core.Scene;
using Fusee.Engine.Core.ShaderShards;
using Fusee.Engine.Core.ShaderShards.Fragment;
using Fusee.Engine.Core.ShaderShards.Vertex;
using Fusee.Math.Core;
using System.Collections.Generic;


namespace Fusee.Examples.MuVista.Core
{

    public class PanoSphere : SceneNode
    {
        public Transform sphereTransform;

        public float radius = 20;

        private SurfaceEffect _surfaceEffect;

        private Texture _texture;

        public PanoSphere previous = null;

        public PanoSphere next = null;
        public PanoSphere(string imageName)
        {
            Name = "PanoSphere";
            _texture = new Texture(AssetStorage.Get<ImageData>("Panos\\" + imageName), true, TextureFilterMode.LinearMipmapLinear);

            Sphere sphere = new Sphere(radius, 20, 50);


            _surfaceEffect = MakeEffect.FromUnlitOpacity(
                albedoColor: float4.One,
                albedoTex: _texture,
                texTiles: float2.One,
                albedoMix: 1.0f,
                texOpacity: 1f         
            );

            sphereTransform = new Transform
            {
                Rotation = new float3(0, 0, 0),
                Scale = new float3(1, 1, 1),
                Translation = new float3(0, 0, 0)
            };

            Components = new List<SceneComponent>()
            {
                new RenderLayer()
                {
                    Layer = RenderLayers.Layer02
                },
                sphereTransform,
                _surfaceEffect,
                sphere,
            };
        }
    }
}