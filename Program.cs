using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using OpenTK.Input;

namespace DefferedRendering
{
    class Game : GameWindow
    {
        [STAThread]
        static void Main()
        {
            Game game = new Game();
            game.Run();
        }

        static int window_width = 600;
        static int window_height = 600;

        public Game() : base(window_width, window_height, GraphicsMode.Default, "Sample")
        {
            VSync = VSyncMode.On;
        }

        protected override void OnResize(EventArgs E)
        {
            base.OnResize(E);
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
        }

        int geometryPassShader, lightingPassShader;

        int Gbuffer;
        int GbufferAlbedoSpecularTex;
        int GbufferNormalTex;
        int GbufferPositionTex;
        int quadVAO, quadVBO;

        protected override void OnLoad(EventArgs E)
        {
            base.OnLoad(E);

            GL.ClearColor(Color.Black);
            //**************shaders****************
            geometryPassShader = CompileShaders.Compile(new System.IO.StreamReader("frag_shader_geom_pass.glsl"), new System.IO.StreamReader("vert_shader_geom_pass.glsl"));
            GL.UseProgram(geometryPassShader);

            GL.UniformMatrix4(GL.GetUniformLocation(geometryPassShader, "model_mat"), false, ref model);
            GL.UniformMatrix4(GL.GetUniformLocation(geometryPassShader, "projection_mat"), false, ref projection);

            lightingPassShader = CompileShaders.Compile(new System.IO.StreamReader("frag_shader_lighting_pass.glsl"), new System.IO.StreamReader("vert_shader_lighting_pass.glsl"));
            GL.UseProgram(lightingPassShader);

            GL.Uniform1(GL.GetUniformLocation(lightingPassShader, "g_position"), 0);
            GL.Uniform1(GL.GetUniformLocation(lightingPassShader, "g_albedospecular"), 1);
            GL.Uniform1(GL.GetUniformLocation(lightingPassShader, "g_normal"), 2);
            //*************screen quad*************
            float[] vertices =
            {
                -1, -1,  0, 0,
                -1, 1,   0, 1,
                1, 1,    1, 1,
                1, -1,   1, 0
            };

            quadVAO = GL.GenVertexArray();
            quadVBO = GL.GenBuffer();

            GL.BindVertexArray(quadVAO);
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, quadVBO);
                GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

                GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
                GL.EnableVertexAttribArray(1);

                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }
            GL.BindVertexArray(0);
            //********Gbuffer*****************
            Gbuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, Gbuffer);
            {
                GbufferPositionTex = GL.GenTexture();
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, GbufferPositionTex);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Nearest);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, window_width, window_height, 0, PixelFormat.Rgb, PixelType.Float, IntPtr.Zero);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, GbufferPositionTex, 0);

                GbufferAlbedoSpecularTex = GL.GenTexture();
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, GbufferAlbedoSpecularTex);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Nearest);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, window_width, window_height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, GbufferAlbedoSpecularTex, 0);

                GbufferNormalTex = GL.GenTexture();
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2D, GbufferNormalTex);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Nearest);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, window_width, window_height, 0, PixelFormat.Rgb, PixelType.Float, IntPtr.Zero);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment2, TextureTarget.Texture2D, GbufferNormalTex, 0);

                int depth_renderbuffer = GL.GenRenderbuffer();
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depth_renderbuffer);
                GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent32f, window_width, window_height);
                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, depth_renderbuffer);

                var attachments = new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2 };

                GL.DrawBuffers(3, attachments);
            }
            //**************************************
            dragon.Load("stanford-dragon.obj");
        }

        Model dragon = new Model();
        Camera camera = new Camera(new Vector3(0, 0, 10), 0, -(float)Math.PI / 2);

        Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, (float)window_width / (float)window_height, 0.1f, 100);
        Matrix4 model = Matrix4.Identity;

        protected override void OnRenderFrame(FrameEventArgs E)
        {
            base.OnRenderFrame(E);

            GL.UseProgram(geometryPassShader);
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, Gbuffer);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);                
                GL.Enable(EnableCap.DepthTest);

                Matrix4 view = camera.Matrix;              
                GL.UniformMatrix4(GL.GetUniformLocation(geometryPassShader, "view_mat"), false, ref view);

                dragon.Draw();
            }
            
            GL.UseProgram(lightingPassShader);
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                GL.Disable(EnableCap.DepthTest);

                GL.BindVertexArray(quadVAO);
                {
                    GL.DrawArrays(PrimitiveType.Quads, 0, 4);
                }
            }
            
            camera.Update(0.01f);

            SwapBuffers();
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);

            camera.MouseEvents(e);
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            base.OnKeyUp(e);

            camera.MouseEvents(e);
        }
    }
}