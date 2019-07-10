using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using OpenTK.Input;
using System.Drawing.Imaging;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

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

        public Game() : base(window_width, window_height, GraphicsMode.Default, "DefferedRendering")
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
        int GbufferAlbedoTex, GbufferMetallicTex, GbufferRoughnessTex;
        int GbufferNormalTex, GbufferPositionTex;

        int quadVAO, quadVBO;

        protected override void OnLoad(EventArgs E)
        {
            base.OnLoad(E);

            GL.ClearColor(Color.Black);

            #region shaders
           
            lightingPassShader = CompileShaders.Compile(new System.IO.StreamReader("frag_shader_lighting_pass.glsl"), new System.IO.StreamReader("vert_shader_lighting_pass.glsl"));
            GL.UseProgram(lightingPassShader);

            GL.Uniform1(GL.GetUniformLocation(lightingPassShader, "g_position"), 0);
            GL.Uniform1(GL.GetUniformLocation(lightingPassShader, "g_normal"), 1);
            GL.Uniform1(GL.GetUniformLocation(lightingPassShader, "g_albedo"), 2);
            GL.Uniform1(GL.GetUniformLocation(lightingPassShader, "g_metallic"), 3);
            GL.Uniform1(GL.GetUniformLocation(lightingPassShader, "g_roughness"), 4);

            geometryPassShader = CompileShaders.Compile(new System.IO.StreamReader("frag_shader_geom_pass.glsl"), new System.IO.StreamReader("vert_shader_geom_pass.glsl"));
            GL.UseProgram(geometryPassShader);

            GL.UniformMatrix4(GL.GetUniformLocation(geometryPassShader, "model_mat"), false, ref model);
            GL.UniformMatrix4(GL.GetUniformLocation(geometryPassShader, "projection_mat"), false, ref projection);

            GL.Uniform1(GL.GetUniformLocation(geometryPassShader, "AlbedoNormalsMaps"), 8);
            GL.Uniform1(GL.GetUniformLocation(geometryPassShader, "MetallRoughnessMaps"), 9);
            #endregion

            #region screen quad
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
            #endregion

            #region Gbuffer
            Gbuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, Gbuffer);
            {
                void SetTexParametersNearest()
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Nearest);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Nearest);
                }

                GbufferPositionTex = GL.GenTexture();
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, GbufferPositionTex);
                SetTexParametersNearest();
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, window_width, window_height, 0, PixelFormat.Rgb, PixelType.Float, IntPtr.Zero);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, GbufferPositionTex, 0);

                GbufferNormalTex = GL.GenTexture();
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, GbufferNormalTex);
                SetTexParametersNearest();
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, window_width, window_height, 0, PixelFormat.Rgb, PixelType.Float, IntPtr.Zero);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, GbufferNormalTex, 0);


                GbufferAlbedoTex = GL.GenTexture();
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2D, GbufferAlbedoTex);
                SetTexParametersNearest();
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, window_width, window_height, 0, PixelFormat.Rgb, PixelType.Float, IntPtr.Zero);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment2, TextureTarget.Texture2D, GbufferAlbedoTex, 0);

                GbufferMetallicTex = GL.GenTexture();
                GL.ActiveTexture(TextureUnit.Texture3);
                GL.BindTexture(TextureTarget.Texture2D, GbufferMetallicTex);
                SetTexParametersNearest();
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, window_width, window_height, 0, PixelFormat.Red, PixelType.Float, IntPtr.Zero);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment3, TextureTarget.Texture2D, GbufferMetallicTex, 0);

                GbufferRoughnessTex = GL.GenTexture();
                GL.ActiveTexture(TextureUnit.Texture4);
                GL.BindTexture(TextureTarget.Texture2D, GbufferRoughnessTex);
                SetTexParametersNearest();
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, window_width, window_height, 0, PixelFormat.Red, PixelType.Float, IntPtr.Zero);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment4, TextureTarget.Texture2D, GbufferRoughnessTex, 0);

                int depth_renderbuffer = GL.GenRenderbuffer();
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depth_renderbuffer);
                GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent32f, window_width, window_height);
                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, depth_renderbuffer);

                var attachments = new DrawBuffersEnum[] 
                {
                    DrawBuffersEnum.ColorAttachment0,
                    DrawBuffersEnum.ColorAttachment1,
                    DrawBuffersEnum.ColorAttachment2,
                    DrawBuffersEnum.ColorAttachment3,
                    DrawBuffersEnum.ColorAttachment4
                };

                GL.DrawBuffers(5, attachments);
            }
            #endregion

            #region PBR textures

            Bitmap albedo_map = (Bitmap)Image.FromFile("textures/albedo.bmp");
            albedo_map.RotateFlip(RotateFlipType.RotateNoneFlipY);

            Bitmap roughness_map = (Bitmap)Image.FromFile("textures/roughness.bmp");
            roughness_map.RotateFlip(RotateFlipType.RotateNoneFlipY);

            Bitmap metall_map = (Bitmap)Image.FromFile("textures/metall.bmp");
            metall_map.RotateFlip(RotateFlipType.RotateNoneFlipY);

            Bitmap normals_map = (Bitmap)Image.FromFile("textures/normals.bmp");
            normals_map.RotateFlip(RotateFlipType.RotateNoneFlipY);

            int map_width = albedo_map.Width;
            int map_height = albedo_map.Height;

            var alb_data    = albedo_map.LockBits(new Rectangle(0, 0, map_width, map_height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            var rough_data  = roughness_map.LockBits(new Rectangle(0, 0, map_width, map_height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            var met_data    = metall_map.LockBits(new Rectangle(0, 0, map_width, map_height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            var norm_data   = normals_map.LockBits(new Rectangle(0, 0, map_width, map_height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            int AlbedoNormals = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture8);
            GL.BindTexture(TextureTarget.Texture2DArray, AlbedoNormals);

            GL.TexStorage3D(TextureTarget3d.Texture2DArray, 1, SizedInternalFormat.Rgba8, map_width, map_height, 2);

            GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, 0, map_width, map_height, 1, PixelFormat.Bgr, PixelType.UnsignedByte, alb_data.Scan0);
            GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, 1, map_width, map_height, 1, PixelFormat.Bgr, PixelType.UnsignedByte, norm_data.Scan0);

            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)All.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)All.Linear);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2DArray);

            albedo_map.UnlockBits(alb_data);
            normals_map.UnlockBits(norm_data);


            int MetallRoughness = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture9);
            GL.BindTexture(TextureTarget.Texture2DArray, MetallRoughness);

            GL.TexStorage3D(TextureTarget3d.Texture2DArray, 1, SizedInternalFormat.R8, map_width, map_height, 2);

            GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, 0, map_width, map_height, 1, PixelFormat.Bgr, PixelType.UnsignedByte, met_data.Scan0);
            GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, 1, map_width, map_height, 1, PixelFormat.Bgr, PixelType.UnsignedByte, rough_data.Scan0);

            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)All.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)All.Linear);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2DArray);

            metall_map.UnlockBits(met_data);
            roughness_map.UnlockBits(rough_data);
            #endregion

            dragon.Load("wrenches.obj");
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
            
            camera.Update(0.05f);

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