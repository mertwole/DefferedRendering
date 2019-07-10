using OpenTK;
using OpenTK.Input;
using System;
using static System.Math;

namespace DefferedRendering
{
    public class Camera
    {
        public Matrix4 Matrix { get { return RefreshMatrix(); } }

        public Vector3 Pos { get { return pos; } }

        public Vector3 Dir
        {
            get
            {
                Vector3 front;
                front.X = (float)(Cos(pitch) * Cos(yaw));
                front.Y = (float)Sin(pitch);
                front.Z = (float)(Cos(pitch) * Sin(yaw));

                return front.Normalized();
            }
        }

        public Camera(Vector3 Position, float pitch, float yaw)
        {
            pos = Position;
            this.yaw = yaw;
            this.pitch = pitch;
        }

        Matrix4 RefreshMatrix()
        {
            Vector3 front;
            front.X = (float)(Cos(pitch) * Cos(yaw));
            front.Y = (float)Sin(pitch);
            front.Z = (float)(Cos(pitch) * Sin(yaw));

            return Matrix4.LookAt(pos, pos + front, new Vector3(0, 1, 0));
        }

        Vector3 pos;

        float pitch;
        float yaw;

        public void MouseEvents(KeyboardKeyEventArgs e)
        {
            if (e.Keyboard.IsKeyDown(Key.W))
                local_z_dir = 1;
            else if (e.Keyboard.IsKeyDown(Key.S))
                local_z_dir = -1;
            else
                local_z_dir = 0;

            if (e.Keyboard.IsKeyDown(Key.D))
                local_x_dir = 1;
            else if (e.Keyboard.IsKeyDown(Key.A))
                local_x_dir = -1;
            else
                local_x_dir = 0;

            if (e.Keyboard.IsKeyDown(Key.Up))
                rotate_updown_key = -1;
            else if (e.Keyboard.IsKeyDown(Key.Down))
                rotate_updown_key = 1;
            else
                rotate_updown_key = 0;

            if (e.Keyboard.IsKeyDown(Key.Right))
                rotate_leftright_key = 1;
            else if (e.Keyboard.IsKeyDown(Key.Left))
                rotate_leftright_key = -1;
            else
                rotate_leftright_key = 0;
        }

        float local_z_dir;
        float local_x_dir;

        float rotate_leftright_key;
        float rotate_updown_key;

        public void Update(float speed)
        {
            //move
            Vector3 front;
            front.X = (float)(Cos(pitch) * Cos(yaw));
            front.Y = (float)Sin(pitch);
            front.Z = (float)(Cos(pitch) * Sin(yaw));

            pos += local_z_dir * speed * front.Normalized() * move_sensetivity;

            Vector3 right = Vector3.Cross(front, new Vector3(0, 1, 0));

            pos += local_x_dir * speed * right.Normalized() * move_sensetivity;

            //rotate
            pitch -= rotate_sensetivity * rotate_updown_key * speed;
            yaw += rotate_sensetivity * rotate_leftright_key * speed;

            if (pitch > PI / 2)
                pitch = (float)PI / 2 - 0.01f;

            if (pitch < - PI / 2)
                pitch = -(float)PI / 2 + 0.01f;
        }

        public float move_sensetivity = 2;
        public float rotate_sensetivity = 2;
    }
}
