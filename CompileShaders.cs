﻿using OpenTK.Graphics.OpenGL;
using System;
using System.IO;

namespace DefferedRendering
{
    class CompileShaders
    {
        public static int Compile(StreamReader fragment_shader, StreamReader vertex_shader)
        {
            string vertex_shader_code = vertex_shader.ReadToEnd();
            string fragment_shader_code = fragment_shader.ReadToEnd();

            int shader_program;

            int vert_shader = GL.CreateShader(ShaderType.VertexShader);
            int frag_shader = GL.CreateShader(ShaderType.FragmentShader);
            
            GL.ShaderSource(frag_shader, fragment_shader_code);
            GL.CompileShader(frag_shader);

            Console.WriteLine(GL.GetShaderInfoLog(frag_shader));

            GL.ShaderSource(vert_shader, vertex_shader_code);
            GL.CompileShader(vert_shader);

            Console.WriteLine(GL.GetShaderInfoLog(vert_shader));

            shader_program = GL.CreateProgram();

            GL.AttachShader(shader_program, vert_shader);
            GL.AttachShader(shader_program, frag_shader);

            GL.LinkProgram(shader_program);

            Console.WriteLine(GL.GetProgramInfoLog(shader_program));

            GL.DeleteShader(vert_shader);
            GL.DeleteShader(frag_shader);

            return shader_program;
        }

        public static int CompileComputeShader(StreamReader compute_shader)
        {
            string compute_shader_code = compute_shader.ReadToEnd();

            int comp_shader = GL.CreateShader(ShaderType.ComputeShader);
            GL.ShaderSource(comp_shader, compute_shader_code);
            GL.CompileShader(comp_shader);

            Console.WriteLine(GL.GetShaderInfoLog(comp_shader));

            int shader_program = GL.CreateProgram();

            GL.AttachShader(shader_program, comp_shader);
            GL.LinkProgram(shader_program);

            Console.WriteLine(GL.GetProgramInfoLog(shader_program));

            GL.DeleteShader(comp_shader);

            return shader_program;
        }
    }
}
