using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Engine.Core.Effects;
using Fusee.Engine.Core.Scene;
using Fusee.Engine.Core.ShaderShards;
using Fusee.Engine.Gui;
using Fusee.Math.Core;
using Fusee.Xene;
using System.Collections.Generic;
using System.Linq;

namespace Fusee.Examples.MuVista.Core
{
    public class GUI : SceneContainer
    {
        public GuiButton _btnZoomOut;
        public GuiButton _btnZoomIn;

        public GuiButton _btnMiniMap;

        public GuiButton _btnPanoAlpha;
        public GuiButton _btnPanoAlphaUp;
        public GuiButton _btnPanoAlphaDown;

        public float2 _zoomInBtnPosition;
        public float2 _zoomOutBtnPosition;

        public float2 _miniMapBtnPosition;

        public float2 _btnPanoAlphaPosition;
        public float2 _btnPanoAlphaUpPosition;
        public float2 _btnPanoAlphaDownPosition;
        public TextureNode _panoAlphaNode;
        public bool _movePanoAlphaHandler = false;
        public float _velocity;

        public TextNode _panoAlphaPercent;
        public float _lastStep = 100;

        public GUI(int width, int height, CanvasRenderMode canvasRenderMode, Transform mainCamTransform, Camera guiCam)
        {
            var vsNineSlice = AssetStorage.Get<string>("nineSlice.vert");
            var psNineSlice = AssetStorage.Get<string>("nineSliceTile.frag");


            var canvasWidth = width / 100f;
            var canvasHeight = height / 100f;

            var btnFuseeLogo = new GuiButton
            {
                Name = "Canvas_Button"
            };
            btnFuseeLogo.OnMouseEnter += BtnLogoEnter;
            btnFuseeLogo.OnMouseExit += BtnLogoExit;
            //btnFuseeLogo.OnMouseDown += BtnLogoDown;

            Texture guiFuseeLogo = new Texture(AssetStorage.Get<ImageData>("FuseeText.png"));
            TextureNode fuseeLogo = TextureNode.Create(
                "fuseeLogo",
                //Set the albedo texture you want to use.
                guiFuseeLogo,
                //Define anchor points. They are given in percent, seen from the lower left corner, respectively to the width/height of the parent.
                //In this setup the element will stretch horizontally but stay the same vertically if the parent element is scaled.
                GuiElementPosition.GetAnchors(AnchorPos.TopTopLeft),
                //Define Offset and therefor the size of the element.
                GuiElementPosition.CalcOffsets(AnchorPos.TopTopLeft, new float2(0, canvasHeight - 0.5f), canvasHeight, canvasWidth, new float2(1.75f, 0.5f)),
                float2.One
                );
            fuseeLogo.AddComponent(btnFuseeLogo);

            var fontLato = AssetStorage.Get<Font>("Lato-Black.ttf");
            var guiLatoBlack = new FontMap(fontLato, 24);

            var text = TextNode.Create(
                "MuVista Pointcloud and Panorama Viewer",
                "ButtonText",
                GuiElementPosition.GetAnchors(AnchorPos.StretchHorizontal),
                GuiElementPosition.CalcOffsets(AnchorPos.StretchHorizontal, new float2(canvasWidth / 2 - 4, 0), canvasHeight, canvasWidth, new float2(8, 1)),
                guiLatoBlack,
                (float4)ColorUint.Greenery,
                HorizontalTextAlignment.Center,
                VerticalTextAlignment.Center);

            _btnZoomOut = new GuiButton
            {
                Name = "Zoom_Out_Button"
            };

            _btnZoomIn = new GuiButton
            {
                Name = "Zoom_In_Button"
            };

            _btnMiniMap = new GuiButton
            {
                Name = "MiniMap"
            };

            _btnPanoAlpha = new GuiButton
            {
                Name = "Pano_Alpha"
            };
            _btnPanoAlphaUp = new GuiButton
            {
                Name = "Pano_Alpha_Up"
            };
            _btnPanoAlphaDown = new GuiButton
            {
                Name = "Pano_Alpha_Down"
            };

            _zoomInBtnPosition = new float2(canvasWidth - 1f, 1f);
            TextureNode zoomInNode = TextureNode.Create(
                "ZoomInLogo",
                new Texture(AssetStorage.Get<ImageData>("FuseePlusIcon.png")),
                GuiElementPosition.GetAnchors(AnchorPos.DownDownRight),
                GuiElementPosition.CalcOffsets(AnchorPos.DownDownRight, _zoomInBtnPosition, canvasHeight, canvasWidth, new float2(0.5f, 0.5f)),
                float2.One
                );
            zoomInNode.Components.Add(_btnZoomIn);

            _zoomOutBtnPosition = new float2(canvasWidth - 1f, 0.4f);
            var zoomOutNode =  TextureNode.Create(
                "ZoomOutLogo",
                new Texture(AssetStorage.Get<ImageData>("FuseeMinusIcon.png")),
                GuiElementPosition.GetAnchors(AnchorPos.DownDownRight),
                GuiElementPosition.CalcOffsets(AnchorPos.DownDownRight, _zoomOutBtnPosition, canvasHeight, canvasWidth, new float2(0.5f, 0.5f)),
                float2.One
                );
            zoomOutNode.Components.Add(_btnZoomOut);

            _miniMapBtnPosition = new float2(canvasWidth - 3f, canvasHeight - 2f);
            Texture tex = new Texture(AssetStorage.Get<ImageData>("Fusee-Minimap.png"));
            TextureNode minimapNode =  TextureNode.Create(
                "MinimapRahmen",
                tex,
                GuiElementPosition.GetAnchors(AnchorPos.TopTopRight),
                GuiElementPosition.CalcOffsets(AnchorPos.TopTopRight, _miniMapBtnPosition, canvasHeight, canvasWidth, new float2(3f, 2f)),
                float2.One
                );
            minimapNode.Components.Add(_btnMiniMap);

            _btnPanoAlphaPosition = new float2(0.375f - 2, 2.8f);
            _panoAlphaNode = TextureNode.Create(
                "AlphaPanoHandler",
                new Texture(AssetStorage.Get<ImageData>("AlphaHandle.png")),
                GuiElementPosition.GetAnchors(AnchorPos.DownDownLeft),
                GuiElementPosition.CalcOffsets(AnchorPos.DownDownLeft, _btnPanoAlphaPosition, canvasHeight, canvasWidth, new float2(0.5f, 0.2f)),
                float2.One
                );
            _panoAlphaNode.Components.Add(_btnPanoAlpha);

            _btnPanoAlphaUpPosition = new float2(0.5f - 2, 3f);
            var panoAlphaUpNode = TextureNode.Create(
                "AlphaPanoUpHandler",
                new Texture(AssetStorage.Get<ImageData>("AlphaUp.png")),
                GuiElementPosition.GetAnchors(AnchorPos.DownDownLeft),
                GuiElementPosition.CalcOffsets(AnchorPos.DownDownLeft, _btnPanoAlphaUpPosition, canvasHeight, canvasWidth, new float2(0.25f, 0.25f)),
                float2.One
                );
            panoAlphaUpNode.Components.Add(_btnPanoAlphaUp);

            _btnPanoAlphaDownPosition = new float2(0.5f - 2, 0.25f);
            var panoAlphaDownNode = TextureNode.Create(
                "AlphaPanoDownHandler",
                new Texture(AssetStorage.Get<ImageData>("AlphaDown.png")),
                GuiElementPosition.GetAnchors(AnchorPos.DownDownLeft),
                GuiElementPosition.CalcOffsets(AnchorPos.DownDownLeft, _btnPanoAlphaDownPosition, canvasHeight, canvasWidth, new float2(0.25f, 0.25f)),
                float2.One
                );
            panoAlphaDownNode.Components.Add(_btnPanoAlphaDown);

            _panoAlphaPercent = TextNode.Create(
                "100%",
                "PanoAlphaPercent",
                GuiElementPosition.GetAnchors(AnchorPos.DownDownLeft),
                GuiElementPosition.CalcOffsets(AnchorPos.DownDownLeft, new float2(0.375f - 2, 3.2f), canvasHeight, canvasWidth, new float2(0.5f, 0.5f)),
                guiLatoBlack,
                (float4)ColorUint.Greenery,
                HorizontalTextAlignment.Center,
                VerticalTextAlignment.Center
            );

            TextureNode panoAlphaBackground = TextureNode.Create(
                "PanoAlphaBackground",
                new Texture(AssetStorage.Get<ImageData>("AlphaBackground.png")),
                GuiElementPosition.GetAnchors(AnchorPos.DownDownLeft),
                GuiElementPosition.CalcOffsets(AnchorPos.DownDownLeft, new float2(0.575f - 2, 0.5f), canvasHeight, canvasWidth, new float2(0.1f, 2.5f)),
                float2.One
                );

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
                    minimapNode,
                    panoAlphaBackground,
                    panoAlphaDownNode,
                    panoAlphaUpNode,
                    _panoAlphaNode,
                    //_panoAlphaPercent,
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

        public void OnPanoAlphaUp(CodeComponent sender)
        {
            _movePanoAlphaHandler = true;
            _velocity = 1f;
        }

        public void OnPanoAlphaDown(CodeComponent sender)
        {
            _movePanoAlphaHandler = true;
            _velocity = -1;
        }

        public void OnPanoAlphaStop(CodeComponent sender)
        {
            _movePanoAlphaHandler = false;
        }

        public void replaceTextForPercent(string percent, float width, float height)
        {
            for(int i = 0; i < this.Children[1].Children.Count; i++)
            {
                if(this.Children[1].Children[i].Name == "PanoAlphaPercent")
                {
                    this.Children[1].Children.Remove(this.Children[1].Children[i]);
                }
            }

            var canvasWidth = width / 100f;
            var canvasHeight = height/ 100f;

            var fontLato = AssetStorage.Get<Font>("Lato-Black.ttf");
            var guiLatoBlack = new FontMap(fontLato, 24);

            _panoAlphaPercent = TextNode.Create(
                percent,
                "PanoAlphaPercent",
                GuiElementPosition.GetAnchors(AnchorPos.DownDownLeft),
                GuiElementPosition.CalcOffsets(AnchorPos.DownDownLeft, new float2(0.375f, 3.2f), canvasHeight, canvasWidth, new float2(0.5f, 0.5f)),
                guiLatoBlack,
                (float4)ColorUint.Greenery,
                HorizontalTextAlignment.Center,
                VerticalTextAlignment.Center
            );

            this.Children[1].Children.Add(_panoAlphaPercent);
        }

        public void DeactivatePanoAlphaHandle()
        {
            
            for (int i = 0; i < this.Children[1].Children.Count; i++)
            {
                if (this.Children[1].Children[i].Name == "PanoAlphaBackground" || this.Children[1].Children[i].Name == "PanoAlphaPercent" || this.Children[1].Children[i].Name == "AlphaPanoDownHandler" || this.Children[1].Children[i].Name == "AlphaPanoUpHandler" || this.Children[1].Children[i].Name == "AlphaPanoHandler")
                {
                    if (this.Children[1].Children[i].GetComponent<RectTransform>().Offsets.Min.x > 0f)
                    {
                        this.Children[1].Children[i].GetComponent<RectTransform>().Offsets.Min.x -= 2f;
                        this.Children[1].Children[i].GetComponent<RectTransform>().Offsets.Max.x -= 2f;
                    }
                }
            }
        }

        public void ActivatePanoAlphaHandle()
        {
            
            for(int i = 0; i < this.Children[1].Children.Count; i++)
            {
                if (this.Children[1].Children[i].Name == "PanoAlphaBackground" || this.Children[1].Children[i].Name == "PanoAlphaPercent" || this.Children[1].Children[i].Name == "AlphaPanoDownHandler" || this.Children[1].Children[i].Name == "AlphaPanoUpHandler" || this.Children[1].Children[i].Name == "AlphaPanoHandler")
                {
                    if (this.Children[1].Children[i].GetComponent<RectTransform>().Offsets.Min.x < 0f)
                    {
                        this.Children[1].Children[i].GetComponent<RectTransform>().Offsets.Min.x += 2f;
                        this.Children[1].Children[i].GetComponent<RectTransform>().Offsets.Max.x += 2f;
                    }
                }
            }
        }

    }
}