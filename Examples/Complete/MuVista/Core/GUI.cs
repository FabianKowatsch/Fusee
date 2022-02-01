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
        public float2 _zoomInBtnPosition;
        public float2 _zoomOutBtnPosition;

        public GuiButton _btnMiniMap;
        public float2 _miniMapBtnPosition;


        public TextureNode _panoAlphaHandle;
        public GuiButton _btnPanoAlphaUp;
        public GuiButton _btnPanoAlphaDown;
        public bool _movePanoAlphaHandler = false;

        public TextureNode _pointSizeHandle;
        public GuiButton _btnPointSizeUp;
        public GuiButton _btnPointSizeDown;
        public bool _movePointSizeHandler = false;

        public float _velocity;

        

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
            var guiLatoBlackSmall = new FontMap(fontLato, 16);

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
            _btnPanoAlphaUp = new GuiButton
            {
                Name = "Pano_Alpha_Up"
            };
            _btnPanoAlphaDown = new GuiButton
            {
                Name = "Pano_Alpha_Down"
            };
            _btnPointSizeUp = new GuiButton
            {
                Name = "Point_Size_Up"
            };
            _btnPointSizeDown = new GuiButton
            {
                Name = "Point_Size_Down"
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

            _panoAlphaHandle = this.CreateHandle(2.4f - 2, 0.2f, "Pano \nAlpha", guiLatoBlackSmall, canvasHeight, canvasWidth, _btnPanoAlphaUp, _btnPanoAlphaDown);
            _pointSizeHandle = this.CreateHandle(1.8f, 0.2f, "Point \nSize", guiLatoBlackSmall, canvasHeight, canvasWidth, _btnPointSizeUp, _btnPointSizeDown);

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
                    _panoAlphaHandle,
                    _pointSizeHandle
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

        public void OnPointSizeUp(CodeComponent sender)
        {
            _movePointSizeHandler = true;
            _velocity = 1f;
        }

        public void OnPointSizeDown(CodeComponent sender)
        {
            _movePointSizeHandler = true;
            _velocity = -1;
        }

        public void OnPointSizeStop(CodeComponent sender)
        {
            _movePointSizeHandler = false;
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

        public void DeactivatePanoAlphaHandle()
        {
            _panoAlphaHandle.GetComponent<RectTransform>().Offsets.Min.x -= 2f;
        }

        public void ActivatePanoAlphaHandle()
        {
            _panoAlphaHandle.GetComponent<RectTransform>().Offsets.Min.x += 2f;
        }

        private TextureNode CreateHandle(float _xPos, float _yPos, string _title, FontMap _font, float _canvasHeight, float _canvasWidth, GuiButton _btnUp, GuiButton _btnDown)
        {
            float2 handlePos = new float2(0.375f - 2, 2.8f);
            TextureNode handle = TextureNode.Create(
                _title + "Handler",
                new Texture(AssetStorage.Get<ImageData>("AlphaHandle.png")),
                GuiElementPosition.GetAnchors(AnchorPos.DownDownLeft),
                GuiElementPosition.CalcOffsets(AnchorPos.DownDownLeft, handlePos, _canvasHeight, _canvasWidth, new float2(0.5f, 0.2f)),
                float2.One
                );

            float2 upPos = new float2(0.5f - 2, 3f);
            TextureNode up = TextureNode.Create(
                _title + "Up",
                new Texture(AssetStorage.Get<ImageData>("AlphaUp.png")),
                GuiElementPosition.GetAnchors(AnchorPos.DownDownLeft),
                GuiElementPosition.CalcOffsets(AnchorPos.DownDownLeft, upPos, _canvasHeight, _canvasWidth, new float2(0.25f, 0.25f)),
                float2.One
                );
            up.Components.Add(_btnUp);

            float2 downPos = new float2(0.5f - 2, 0.25f);
            TextureNode down = TextureNode.Create(
                _title + "Down",
                new Texture(AssetStorage.Get<ImageData>("AlphaDown.png")),
                GuiElementPosition.GetAnchors(AnchorPos.DownDownLeft),
                GuiElementPosition.CalcOffsets(AnchorPos.DownDownLeft, downPos, _canvasHeight, _canvasWidth, new float2(0.25f, 0.25f)),
                float2.One
                );
            down.Components.Add(_btnDown);

            float2 titlePos = new float2(0.375f - 2, 3.2f);
            TextNode title = TextNode.Create(
                _title,
                _title + "Title",
                GuiElementPosition.GetAnchors(AnchorPos.DownDownLeft),
                GuiElementPosition.CalcOffsets(AnchorPos.DownDownLeft, titlePos, _canvasHeight, _canvasWidth, new float2(0.5f, 0.5f)),
                _font,
                (float4)ColorUint.Greenery,
                HorizontalTextAlignment.Center,
                VerticalTextAlignment.Center
            );

            float2 backPos = new float2(0.575f - 2, 0.5f);
            TextureNode background = TextureNode.Create(
                _title + "Background",
                new Texture(AssetStorage.Get<ImageData>("AlphaBackground.png")),
                GuiElementPosition.GetAnchors(AnchorPos.DownDownLeft),
                GuiElementPosition.CalcOffsets(AnchorPos.DownDownLeft, backPos, _canvasHeight, _canvasWidth, new float2(0.1f, 2.5f)),
                float2.One
                );

            float2 handleNodePos = new float2(_xPos, _yPos);
            TextureNode handleNode = TextureNode.Create(
                _title + "test",
                new Texture(AssetStorage.Get<ImageData>("leer.png")),
                GuiElementPosition.GetAnchors(AnchorPos.DownDownLeft),
                GuiElementPosition.CalcOffsets(AnchorPos.DownDownLeft, handleNodePos, _canvasHeight, _canvasWidth, new float2(1, 1f)),
                float2.One
            );
            handleNode.Children.Add(background);
            handleNode.Children.Add(down);
            handleNode.Children.Add(up);
            handleNode.Children.Add(handle);
            handleNode.Children.Add(title);

            return handleNode;
        }

    }
}