
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


        public PanoSphere(string imageName)
        {


            _texture = new Texture(AssetStorage.Get<ImageData>("Panos\\" + imageName + ".jpg"));

            Sphere sphere = new Sphere(10, 20, 50);

            TextureInputOpacity colorInput = new TextureInputOpacity()
            {
                Albedo = float4.One,
                TexOpacity = 0.5f,
                //Emission = float4.Zero,
                //Shininess = 1.0f,
                //SpecularStrength = 0.0f,
                AlbedoMix = 1.0f,
                AlbedoTex = _texture,
                TexTiles = float2.One,
                Roughness = 0.0f
            };

            /*
             *  var sphereTex = new Texture(AssetStorage.Get<ImageData>("ladybug_18534664_20210113_GEO2111300_5561.jpg"));

            sphereTex2 = new Texture(AssetStorage.Get<ImageData>("LadyBug_C1P1.jpg"));

            Sphere sphere = new Sphere(10, 20, 50);
            GridPlane plane = new GridPlane(20, 50, _planeHeight, _planeWidth, DistancePlaneCamera);

            TextureInputOpacity colorInput = new TextureInputOpacity()
            {
                Albedo = float4.One,
                TexOpacity = 0.5f,
                //Emission = float4.Zero,
                //Shininess = 1.0f,
                //SpecularStrength = 0.0f,
                AlbedoMix = 1.0f,
                AlbedoTex = sphereTex,
                TexTiles = float2.One,
                Roughness = 0.0f
            };
            colorInput2 = new TextureInputOpacity()
            {
                Albedo = float4.One,
                TexOpacity = 0.0f,
                AlbedoMix = 1.0f,
                AlbedoTex = sphereTex2,
                TexTiles = float2.One,
                Roughness = 0.0f
            };
            var lightingSetup = LightingSetupFlags.Unlit | LightingSetupFlags.AlbedoTex | LightingSetupFlags.AlbedoTexOpacity;

            _animationEffect = new VertexAnimationSurfaceEffect(lightingSetup, colorInput, FragShards.SurfOutBody_Textures(lightingSetup), VertShards.SufOutBody_PosAnimation)
            {
                PercentPerVertex = 1.0f,
                PercentPerVertex1 = 0.0f
            };
             * */



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
