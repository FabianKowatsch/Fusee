
using Fusee.Base.Core;
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


        private DefaultSurfaceEffect _surfaceEffect;


        private Texture _texture;


        public PanoSphere()
        {


            _texture = new Texture(AssetStorage.Get<ImageData>("Panos\\ladybug_18534664_20210113_GEO2111300_5561.jpg"));

            Sphere sphere = new Sphere(10, 20, 50);

            TextureInputSpecular colorInput = new TextureInputSpecular()
            {
                Albedo = float4.One,
                //Emission = float4.Zero,
                //Shininess = 1.0f,
                //SpecularStrength = 0.0f,
                AlbedoMix = 1.0f,
                AlbedoTex = _texture,
                TexTiles = float2.One,
                Roughness = 0.0f
            };

            var lightingSetup = LightingSetupFlags.Unlit | LightingSetupFlags.AlbedoTex;

            _surfaceEffect = new DefaultSurfaceEffect(lightingSetup, colorInput, FragShards.SurfOutBody_Textures(lightingSetup), VertShards.SufOutBody_PosAnimation);

            sphereTransform = new Transform()
            {
                Rotation = new float3(0, 0, 0),
                Scale = new float3(1, 1, 1),
                Translation = new float3(0, 0, 0)
            };

            Mesh sphereMesh = new Mesh()
            {
                Vertices = sphere.Vertices,
                Normals = sphere.Normals,
                UVs = sphere.UVs,
                Triangles = sphere.Triangles
            };


            Components = new List<SceneComponent>()
            {
                new RenderLayer()
                {
                    Layer = RenderLayers.Layer02
                },
                sphereTransform,
                _surfaceEffect,
                sphereMesh,
            };
        }
    }
}
