using System;
using System.Collections.Generic;
using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.SpecialEffects
{
    [DisallowMultipleComponent]
    public class ScreenBlurController : Singleton.BehaniourBase, IPEGI
    {
        [SerializeField] protected Camera MyCamera;
        [SerializeField] protected Shader copyShader;
        [SerializeField] protected Shader blurShader;
        [SerializeField] protected Shader washAwayShader;
        [SerializeField] protected Shader zoomOutShader;
        [SerializeField] protected int postProcessIteration = 100;

        private Shader ProcessShader
        { 
            get 
            {
                switch (processCommand) 
                {
                    case ProcessCommand.Blur: return blurShader; 
                    case ProcessCommand.WashAway: return washAwayShader;
                    case ProcessCommand.ZoomOut: return zoomOutShader;
                    default: QcLog.CaseNotImplemented(processCommand, context: "Get Process Shader"); return null;
                }
            }
        }

        [Header("Config")]
        public bool allowScreenGrabToRt;
        public bool useSecondBufferForRenderTextureScreenGrab;

        public ProcessCommand CurrentProcessCommand => processCommand;

        [NonSerialized] protected ProcessCommand processCommand;
        [NonSerialized] protected BlurStep step = BlurStep.Off;
        [NonSerialized] protected Material effectMaterialInstance;
        [NonSerialized] protected List<Action> onFirstRenderList = new List<Action>();
        [NonSerialized] protected int blurIteration;

        protected readonly ShaderProperty.TextureValue screenGradTexture = new ShaderProperty.TextureValue("_qcPp_Global_Screen_Read");
        protected readonly ShaderProperty.TextureValue processedScreenTexture = new ShaderProperty.TextureValue("_qcPp_Global_Screen_Effect");
        protected readonly ShaderProperty.FloatValue SCREEN_BLUR_ITERATION = new ShaderProperty.FloatValue("_qcPp_Screen_Blur_Iteration");


        public void RequestUpdate(Action onFirstRendered = null, ProcessCommand afterScreenGrab = ProcessCommand.Blur)
        {
            if (onFirstRendered != null)
            {
                onFirstRenderList.Add(onFirstRendered);
            }

            if (step != BlurStep.Off)
            {
                // If any of the commands wanted Blur, use Blur:
                if (afterScreenGrab != ProcessCommand.Nothing)
                    processCommand = afterScreenGrab;
            } else 
            {
                processCommand = afterScreenGrab;
            }
            step = BlurStep.Requested;
        }

        #region Textures and buffers

        [NonSerialized] protected Texture2D _screenReadTexture2D;
        protected Texture2D ScreenReadTexture2D
        {
            get
            {
                if (!_screenReadTexture2D || _screenReadTexture2D.width != Screen.width || _screenReadTexture2D.height != Screen.height)
                {
                    if (_screenReadTexture2D)
                    {
                        Destroy(_screenReadTexture2D);
                    }

                    _screenReadTexture2D = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, mipChain: false)
                    {
                        name = "Screen Grab"
                    };
                }

                return _screenReadTexture2D;
            }
        }

        [NonSerialized] protected RenderTexture _screenReadRenderTexture;
        protected RenderTexture ScreenReadRenderTexture
        {
            get
            {
                if (!_screenReadRenderTexture || _screenReadRenderTexture.width != Screen.width || _screenReadRenderTexture.height != Screen.height)
                {
                    if (_screenReadRenderTexture)
                    {
                        Destroy(_screenReadRenderTexture);
                    }
                    _screenReadRenderTexture = new RenderTexture(width: (int)(Screen.width), height: (int)(Screen.height), 32) { name = "Screen Grab Tmp" };
                }
                return _screenReadRenderTexture;
            }
        }

        protected virtual Texture CurrentEffectTexture => MainEffectTexture;

        protected Texture CurrentScreenReadTexture => allowScreenGrabToRt
            ? (useSecondBufferForRenderTextureScreenGrab ? ScreenReadSecondBufferRt : (Texture)ScreenReadRenderTexture)
            : ScreenReadTexture2D;

        #endregion

        #region Swap Textures
        private bool _latestIsZero;
        protected void Swap() => _latestIsZero = !_latestIsZero;

        [SerializeField] protected List<RenderTexture> _renderTextures = new List<RenderTexture>();
        protected virtual RenderTexture MainEffectTexture => (_latestIsZero ? _renderTextures[0] : _renderTextures[1]);
        protected virtual RenderTexture PreviousTexture => (_latestIsZero ? _renderTextures[1] : _renderTextures[0]);
        #endregion

        protected virtual Material EffectMaterial
        {
            get
            {
                if (!effectMaterialInstance)
                {
                    if (!copyShader)
                    {
                        Debug.LogError("{0} for {1} not found. Here comes the Exception".F(nameof(copyShader), nameof(ScreenBlurController)));
                    }

                    effectMaterialInstance = new Material(copyShader);
                }
                return effectMaterialInstance;
            }
        }

        protected void InvokeOnCaptured()
        {
            if (onFirstRenderList.Count > 0)
            {
                var invokes = new List<Action>(onFirstRenderList);
                onFirstRenderList.Clear();

                foreach (var act in invokes)
                {
                    try
                    {
                        act.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }
        }

        protected void OnPostRender()
        {
            if (!allowScreenGrabToRt && step == BlurStep.Requested)
            {
                step = BlurStep.ReturnedFromCamera;
                var tex = ScreenReadTexture2D;
                tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, false);
                tex.Apply();
            }
        }

        protected RenderTexture _screenReadSecondBuffer;
        protected RenderTexture ScreenReadSecondBufferRt
        {
            get
            {
                if (!_screenReadSecondBuffer || _screenReadSecondBuffer.width != Screen.width || _screenReadSecondBuffer.height != Screen.height)
                {
                    if (_screenReadSecondBuffer)
                    {
                        Destroy(_screenReadSecondBuffer);
                    }
                    _screenReadSecondBuffer = new RenderTexture(width: (int)(Screen.width), height: (int)(Screen.height), 0)
                    {
                        name = "Screen Rt" //, filterMode = FilterMode.Point
                    };
                }
                return _screenReadSecondBuffer;
            }
        }

        protected virtual void BlitBetweenEffectBuffers(Shader shader)
        {
            var mat = EffectMaterial;
            mat.shader = shader;

            Swap();
            Graphics.Blit(PreviousTexture, MainEffectTexture, mat);

            processedScreenTexture.GlobalValue = MainEffectTexture;
        }

        protected void BlitToEffectBuffer(Texture tex, Shader shader = null)
        {
            Blit(tex, MainEffectTexture, shader);

            processedScreenTexture.GlobalValue = MainEffectTexture;
        }

        protected virtual void Blit(Texture from, RenderTexture to, Shader shader)
        {
            if (shader)
            {
                EffectMaterial.shader = shader;

                Graphics.Blit(from, to, EffectMaterial);
            }
            else
                Graphics.Blit(from, to);
        }

        protected static void BlitGL(Texture source, RenderTexture destination, Material mat)
        {
            RenderTexture.active = destination;
            mat.SetTexture("_MainTex", source);
            GL.PushMatrix();
            GL.LoadOrtho();
            GL.invertCulling = true;
            mat.SetPass(0);
            GL.Begin(GL.QUADS);
            GL.MultiTexCoord2(0, 0.0f, 0.0f);
            GL.Vertex3(0.0f, 0.0f, 0.0f);
            GL.MultiTexCoord2(0, 1.0f, 0.0f);
            GL.Vertex3(1.0f, 0.0f, 0.0f);
            GL.MultiTexCoord2(0, 1.0f, 1.0f);
            GL.Vertex3(1.0f, 1.0f, 1.0f);
            GL.MultiTexCoord2(0, 0.0f, 1.0f);
            GL.Vertex3(0.0f, 1.0f, 0.0f);
            GL.End();
            GL.invertCulling = false;
            GL.PopMatrix();
        }

        protected virtual void Update()
        {
            switch (step)
            {
                case BlurStep.Requested:

                    if (allowScreenGrabToRt)
                    {
                        step = BlurStep.ReturnedFromCamera;

                        MyCamera.enabled = false;
                        MyCamera.targetTexture = ScreenReadRenderTexture;

                        MyCamera.Render();
                        MyCamera.targetTexture = null;
                        MyCamera.enabled = true;

                        if (useSecondBufferForRenderTextureScreenGrab)
                        {
                            Blit(ScreenReadRenderTexture, ScreenReadSecondBufferRt, copyShader);
                        }

                        goto case BlurStep.ReturnedFromCamera;
                    }
                    break;

                case BlurStep.ReturnedFromCamera:

                    BlitToEffectBuffer(CurrentScreenReadTexture, copyShader);

                    screenGradTexture.GlobalValue = CurrentScreenReadTexture;
                    processedScreenTexture.GlobalValue = CurrentScreenReadTexture;

                    step = BlurStep.Blurring;
                    blurIteration = 0;

                    if (processCommand == ProcessCommand.Nothing)
                    {
                        step = BlurStep.Off;
                    }

                    InvokeOnCaptured();

                    break;
                case BlurStep.Blurring:

                    blurIteration++;
                    SCREEN_BLUR_ITERATION.GlobalValue = blurIteration;

                    var shade = ProcessShader;

                    if (shade)
                    {
                        BlitBetweenEffectBuffers(shade);
                    }

                    if (blurIteration > postProcessIteration)
                    {
                        step = BlurStep.Off;
                    }

                    break;
            }
        }


        void Reset()
        {
            MyCamera = GetComponent<Camera>();
            step = BlurStep.Off;
        }

        public enum ProcessCommand
        {
            Blur, Nothing, WashAway, ZoomOut
        }

        protected enum BlurStep
        {
            Off,
            Requested,
            ReturnedFromCamera,
            Blurring
        }


        public override string InspectedCategory => Utils.Singleton.Categories.RENDERING;

        public override void Inspect()
        {
            pegi.nl();

            if ("Grab Screenshot".PegiLabel().Click())
                RequestUpdate(afterScreenGrab: ProcessCommand.Nothing);

            if ("Grab & Blur".PegiLabel().Click())
                RequestUpdate(afterScreenGrab: ProcessCommand.Blur);

            pegi.nl();

            if ("Settings".PegiLabel().isFoldout().nl())
            {
                "Blur Iterations".PegiLabel().edit(ref postProcessIteration);
                pegi.FullWindow.DocumentationClickOpen(() => "For how many frames the Blur operation will be executed.");


                pegi.nl();
                "Grab To Render Texture".PegiLabel().toggleIcon(ref allowScreenGrabToRt).nl();
                if (allowScreenGrabToRt)
                {
                    "Use 2 Buffers".PegiLabel().toggleIcon(ref useSecondBufferForRenderTextureScreenGrab);

                    pegi.FullWindow.DocumentationClickOpen(() => "If you plan to take a screen shot while screen shot is already on the screen, you will to enable this option" +
                                                                 "as same texture can't be read from and written to at the same time");

                    pegi.nl();
                }
            }

            if (pegi.IsFoldedOut || _renderTextures.Count < 2 || !_renderTextures[0] || !_renderTextures[1])
            {
                if (_renderTextures.Count < 2)
                {
                    "You need to assignt 2 render textures for blur to render between".PegiLabel().writeWarning();
                }

                "Render Textures".PegiLabel().edit_List_UObj(_renderTextures).nl();
            }

            if ("Debug".PegiLabel().isFoldout().nl())
            {
                "Screen Read Texture 2D".PegiLabel().write(ScreenReadTexture2D);
            }

        }
    }


    [PEGI_Inspector_Override(typeof(ScreenBlurController))]
    internal class ScreenBlurControllerDrawer : PEGI_Inspector_Override { }


}