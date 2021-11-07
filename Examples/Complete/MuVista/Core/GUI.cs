using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Engine.Core.Effects;
using Fusee.Engine.Core.Scene;
using Fusee.Engine.Core.ShaderShards;
using Fusee.Engine.GUI;
using Fusee.Math.Core;
using Fusee.Xene;
using System.Collections.Generic;
using System.Linq;

namespace Fusee.Examples.MuVista.Core
{
    public class GUI : SceneContainer
    {
        public GUIButton _btnZoomOut;
        public GUIButton _btnZoomIn;

        public GUIButton _btnMiniMap;

        public float2 _zoomInBtnPosition;
        public float2 _zoomOutBtnPosition;

        public float2 _miniMapBtnPosition;

        public GUI(int width, int height, CanvasRenderMode canvasRenderMode, Transform mainCamTransform, Camera guiCam)
        {
            var vsTex = AssetStorage.Get<string>("texture.vert");
            var psTex = AssetStorage.Get<string>("texture.frag");
            var psText = AssetStorage.Get<string>("text.frag");
            var vsNineSlice = AssetStorage.Get<string>("nineSlice.vert");
            var psNineSlice = AssetStorage.Get<string>("nineSliceTile.frag");


            var canvasWidth = width / 100f;
            var canvasHeight = height / 100f;

            var btnFuseeLogo = new GUIButton
            {
                Name = "Canvas_Button"
            };
            btnFuseeLogo.OnMouseEnter += BtnLogoEnter;
            btnFuseeLogo.OnMouseExit += BtnLogoExit;
            //btnFuseeLogo.OnMouseDown += BtnLogoDown;

            Texture guiFuseeLogo = new Texture(AssetStorage.Get<ImageData>("FuseeText.png"));
            TextureNode fuseeLogo = new TextureNode(
                "fuseeLogo",
                vsTex,
                psTex,
                //Set the albedo texture you want to use.
                guiFuseeLogo,
                //Define anchor points. They are given in percent, seen from the lower left corner, respectively to the width/height of the parent.
                //In this setup the element will stretch horizontally but stay the same vertically if the parent element is scaled.
                UIElementPosition.GetAnchors(AnchorPos.TopTopLeft),
                //Define Offset and therefor the size of the element.
                UIElementPosition.CalcOffsets(AnchorPos.TopTopLeft, new float2(0, canvasHeight - 0.5f), canvasHeight, canvasWidth, new float2(1.75f, 0.5f)),
                float2.One
                );
            fuseeLogo.AddComponent(btnFuseeLogo);

            var fontLato = AssetStorage.Get<Font>("Lato-Black.ttf");
            var guiLatoBlack = new FontMap(fontLato, 24);

            var text = new TextNode(
                "FUSEE Spherical Image Viewer",
                "ButtonText",
                vsTex,
                psText,
                UIElementPosition.GetAnchors(AnchorPos.StretchHorizontal),
                UIElementPosition.CalcOffsets(AnchorPos.StretchHorizontal, new float2(canvasWidth / 2 - 4, 0), canvasHeight, canvasWidth, new float2(8, 1)),
                guiLatoBlack,
                ColorUint.Tofloat4(ColorUint.Greenery),
                HorizontalTextAlignment.Center,
                VerticalTextAlignment.Center);

            _btnZoomOut = new GUIButton
            {
                Name = "Zoom_Out_Button"
            };

            _btnZoomIn = new GUIButton
            {
                Name = "Zoom_In_Button"
            };

            _btnMiniMap = new GUIButton
            {
                Name = "MiniMap"
            };

            _zoomInBtnPosition = new float2(canvasWidth - 1f, 1f);
            var zoomInNode = new TextureNode(
                "ZoomInLogo",
                vsTex,
                psTex,
                new Texture(AssetStorage.Get<ImageData>("FuseePlusIcon.png")),
                UIElementPosition.GetAnchors(AnchorPos.DownDownRight),
                UIElementPosition.CalcOffsets(AnchorPos.DownDownRight, _zoomInBtnPosition, canvasHeight, canvasWidth, new float2(0.5f, 0.5f)),
                float2.One
                );
            zoomInNode.Components.Add(_btnZoomIn);

            _zoomOutBtnPosition = new float2(canvasWidth - 1f, 0.4f);
            var zoomOutNode = new TextureNode(
                "ZoomOutLogo",
                vsTex,
                psTex,
                new Texture(AssetStorage.Get<ImageData>("FuseeMinusIcon.png")),
                UIElementPosition.GetAnchors(AnchorPos.DownDownRight),
                UIElementPosition.CalcOffsets(AnchorPos.DownDownRight, _zoomOutBtnPosition, canvasHeight, canvasWidth, new float2(0.5f, 0.5f)),
                float2.One
                );
            zoomOutNode.Components.Add(_btnZoomOut);

            _miniMapBtnPosition = new float2(canvasWidth - 3f, canvasHeight - 2f);
            TextureNode minimapNode = new TextureNode(
                "MinimapRahmen",
                vsTex,
                psTex,
                new Texture(AssetStorage.Get<ImageData>("Fusee-Minimap.png")),
                UIElementPosition.GetAnchors(AnchorPos.TopTopRight),
                UIElementPosition.CalcOffsets(AnchorPos.TopTopRight, _miniMapBtnPosition, canvasHeight, canvasWidth, new float2(3f, 2f)),
                float2.One
                );
            minimapNode.Components.Add(_btnMiniMap);




            var canvas = new CanvasNode(
                "Canvas",
                canvasRenderMode,
                new MinMaxRect
                {
                    Min = new float2(-canvasWidth / 2, -canvasHeight / 2f),
                    Max = new float2(canvasWidth / 2, canvasHeight / 2f)
                })
            {
                Children = new ChildList()
                {
                    //Simple Texture Node, contains the fusee logo.
                    fuseeLogo,
                    text,
                    zoomInNode,
                    zoomOutNode,
                    minimapNode
                }
            };


            Children = new List<SceneNode>
                {
                    new SceneNode
                    {
                        Name = "GuiCam",
                        Components = new List<SceneComponent>()
                    {
                        mainCamTransform,
                        guiCam
                    }
                },
                    
                //Add canvas.
                canvas
            };

        }

        public void BtnLogoEnter(CodeComponent sender)
        {
            var effect = this.Children.FindNodes(node => node.Name == "fuseeLogo").First().GetComponent<Effect>();
            effect.SetFxParam(UniformNameDeclarations.Albedo, (float4)ColorUint.Black);
            effect.SetFxParam(UniformNameDeclarations.AlbedoMix, 0.8f);
        }

        public void BtnLogoExit(CodeComponent sender)
        {
            var effect = this.Children.FindNodes(node => node.Name == "fuseeLogo").First().GetComponent<Effect>();
            effect.SetFxParam(UniformNameDeclarations.Albedo, float4.One);
            effect.SetFxParam(UniformNameDeclarations.AlbedoMix, 1f);
        }

        public void BtnLogoDown(CodeComponent sender)
        {
            //OpenLink("http://fusee3d.org");
        }
    }
}