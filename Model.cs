using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace DefferedRendering
{
    class Model
    {
        int VAO, VBO;

        public void Draw()
        {
            GL.BindVertexArray(VAO);
            {
                GL.DrawArrays(PrimitiveType.Triangles, 0, vertices);
            }
            GL.BindVertexArray(0);
        }

        int vertices;

        public void Load(string path)
        {
            LoadObj loadObj = new LoadObj();

            loadObj.Load(new System.IO.StreamReader(path), LoadObj.LoadData.VerticesNormalsAndTexCoords);

            vertices = loadObj.Vertices.Count;

            VAO = GL.GenVertexArray();
            VBO = GL.GenBuffer();

            GL.BindVertexArray(VAO);
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);

                GL.BufferData(BufferTarget.ArrayBuffer, vertices * sizeof(float) * 14, loadObj.Vertices.ToArray(), BufferUsageHint.StaticDraw);

                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 14 * sizeof(float), 0);
                GL.EnableVertexAttribArray(0);// position
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 14 * sizeof(float), 3 * sizeof(float));
                GL.EnableVertexAttribArray(1);// normal
                GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 14 * sizeof(float), 6 * sizeof(float));
                GL.EnableVertexAttribArray(2);// tangent
                GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, 14 * sizeof(float), 9 * sizeof(float));
                GL.EnableVertexAttribArray(3);// bitangent
                GL.VertexAttribPointer(4, 2, VertexAttribPointerType.Float, false, 14 * sizeof(float), 12 * sizeof(float));
                GL.EnableVertexAttribArray(4);// tex_coord

                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }
            GL.BindVertexArray(0);
        }
    }
}
