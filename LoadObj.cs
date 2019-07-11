using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;

namespace DefferedRendering
{
    class LoadObj
    {
        List<Vector3> normals;
        List<Vector2> tex_coords;
        List<Vector3> positions;

        public List<Vertex> Vertices;

        public struct Vertex
        {         
            public Vector3 position;
            public Vector3 normal;
            public Vector3 tangent;
            public Vector3 bitangent;
            public Vector2 tex_coord;
        }

        public enum LoadData { OnlyVertices, VerticesAndNormals, VerticesAndTexCoords, VerticesNormalsAndTexCoords };

        public void Load(StreamReader stream, LoadData mode)//3Ds max format
        {
            positions = new List<Vector3>();
            normals = new List<Vector3>();
            tex_coords = new List<Vector2>();

            Vertices = new List<Vertex>();

            verticeParser vertParser = VertParser_VertsOnly;

            switch(mode)
            {
                case LoadData.OnlyVertices :                { vertParser = VertParser_VertsOnly;                break; }
                case LoadData.VerticesAndNormals:           { vertParser = VertParser_VertsAndNormals;          break; }
                case LoadData.VerticesAndTexCoords:         { vertParser = VertParser_VertsAndTexCoords;        break; }
                case LoadData.VerticesNormalsAndTexCoords:  { vertParser = VertParser_VertsNormalsAndTexCoords; break; }
            }

            while (true)
            {
                string curr_line = stream.ReadLine();

                if (curr_line == null)
                    break;

                if (curr_line.Length < 3)
                    continue;

                if (curr_line.Substring(0, 2) == "v ")
                {//vertex
                    IEnumerator<float> vert = ParseStringToEnumarator(curr_line);
                    Vector3 vert_vec = new Vector3();
                    vert.MoveNext(); vert_vec.X = vert.Current;
                    vert.MoveNext(); vert_vec.Y = vert.Current;
                    vert.MoveNext(); vert_vec.Z = vert.Current;

                    positions.Add(vert_vec);
                }
                else if (curr_line.Substring(0, 3) == "vn ")
                {//normal
                    IEnumerator<float> norm = ParseStringToEnumarator(curr_line);
                    Vector3 norm_vec = new Vector3();
                    norm.MoveNext(); norm_vec.X = norm.Current;
                    norm.MoveNext(); norm_vec.Y = norm.Current;
                    norm.MoveNext(); norm_vec.Z = norm.Current;

                    normals.Add(norm_vec);
                }
                else if (curr_line.Substring(0, 3) == "vt ")
                {// tex coord
                    IEnumerator<float> tex = ParseStringToEnumarator(curr_line);
                    Vector2 tex_vec = new Vector2();
                    tex.MoveNext(); tex_vec.X = tex.Current;
                    tex.MoveNext(); tex_vec.Y = tex.Current;

                    tex_coords.Add(tex_vec);
                }
                else if (curr_line.Substring(0, 2) == "f ")
                {//face
                    IEnumerator<int> verts = ParseStringToEnumaratorI(curr_line);

                    Vertices.AddRange(vertParser(verts));
                }
            }

            for(int i = 0; i < Vertices.Count; i+= 3)
            {
                var deltaUV0 = Vertices[i].tex_coord - Vertices[1 + 1].tex_coord;
                var deltaPos0 = Vertices[i].position - Vertices[i + 1].position;
                var deltaUV1 = Vertices[i].tex_coord - Vertices[i + 2].tex_coord;
                var deltaPos1 = Vertices[i].position - Vertices[i + 2].position;

                float denominator = 1 / (deltaUV0.X * deltaUV1.Y - deltaUV0.Y * deltaUV1.X);
                var tangent = (deltaPos0 * deltaUV1.Y - deltaPos1 * deltaUV0.Y) * denominator;
                var bitangent = (deltaPos1 * deltaUV0.X - deltaPos0 * deltaUV1.X) * denominator;

                for (int j = i; j < i + 2; j++)
                {
                    var vert = Vertices[j];
                    vert.tangent = tangent;
                    vert.bitangent = bitangent;
                    Vertices[j] = vert;
                }
            }
        }

        delegate List<Vertex> verticeParser(IEnumerator<int> data);

        List<Vertex> VertParser_VertsOnly(IEnumerator<int> data)
        {
            List<Vertex> face = new List<Vertex>(3);

            for (int i = 0; i < 3; i++)
            {
                data.MoveNext();
                Vertex new_vert = new Vertex() { position = positions[data.Current - 1] };
                face.Add(new_vert);
            }

            return face;
        }

        List<Vertex> VertParser_VertsAndNormals(IEnumerator<int> data)
        {
            List<Vertex> face = new List<Vertex>(3);

            for (int i = 0; i < 3; i++)
            {
                data.MoveNext();
                Vertex new_vert = new Vertex() { position = positions[data.Current - 1] };
                data.MoveNext();
                new_vert.normal = normals[data.Current - 1];
                face.Add(new_vert);
            }

            return face;
        }

        List<Vertex> VertParser_VertsAndTexCoords(IEnumerator<int> data)
        {
            List<Vertex> face = new List<Vertex>(3);

            for (int i = 0; i < 3; i++)
            {
                data.MoveNext();
                Vertex new_vert = new Vertex() { position = positions[data.Current - 1] };
                data.MoveNext();
                new_vert.tex_coord = tex_coords[data.Current - 1];
                face.Add(new_vert);
            }

            return face;
        }

        List<Vertex> VertParser_VertsNormalsAndTexCoords(IEnumerator<int> data)
        {
            List<Vertex> face = new List<Vertex>(3);

            for (int i = 0; i < 3; i++)
            {
                data.MoveNext();
                Vertex new_vert = new Vertex() { position = positions[data.Current - 1] };
                data.MoveNext();
                new_vert.tex_coord = tex_coords[data.Current - 1];
                data.MoveNext();
                new_vert.normal = normals[data.Current - 1];
                face.Add(new_vert);
            }

            return face;
        }

        static List<char> allowed_chars = new List<char>() { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ',', '-'};

        static IEnumerator<float> ParseStringToEnumarator(string input)
        {
            input = input.Replace('.', ',') + " ";

            List<char> curr_num = new List<char>();

            foreach(char ch in input)
            {
                if (allowed_chars.Contains(ch))
                    curr_num.Add(ch);
                else if(curr_num.Count != 0)
                {
                    float.TryParse(new string(curr_num.ToArray()), out float i);
                    yield return i;
                    curr_num = new List<char>();
                }              
            }
        }

        static List<char> allowed_charsI = new List<char>() { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};

        static IEnumerator<int> ParseStringToEnumaratorI(string input)
        {
            input = input + " ";

            List<char> curr_num = new List<char>();

            foreach (char ch in input)
            {
                if (allowed_charsI.Contains(ch))
                    curr_num.Add(ch);
                else if (curr_num.Count != 0)
                {
                    int.TryParse(new string(curr_num.ToArray()), out int i);
                    yield return i;
                    curr_num = new List<char>();
                }
            }
        }
    }
}
