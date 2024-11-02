using AppMotorGrafico.figuras3d;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;

namespace AppMotorGrafico.Pantalla
{
    public class Camara3D
    {
        private double oldX, oldY;

        public double AngX { get; set; }
        public double AngY { get; set; }
        public double TlsX { get; set; }
        public double TlsY { get; set; }
        public double Scale { get; set; }

        private Matrix4 projectionMatrix;
        private Matrix4 modelViewMatrix;

        public void IniciarMatrices(int width, int height)
        {
            projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(45.0f),
                (float)width / height,
                0.1f, 100.0f);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projectionMatrix);
        }

        public void ConfigurarMatrices()
        {
            modelViewMatrix = Matrix4.Identity;
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Translate(TlsX, TlsY, -10.0f * Scale);
            GL.Rotate((float)AngX, 1.0f, 0.0f, 0.0f);
            GL.Rotate((float)AngY, 0.0f, 1.0f, 0.0f);
            GL.GetFloat(GetPName.ModelviewMatrix, out modelViewMatrix);
        }

        public Camara3D()
        {
            AngX = AngY = 0.0;
            TlsX = TlsY = 0.0;
            oldX = oldY = 0.0;
            Scale = 1.0;
        }


        public Matrix4 GetModelViewProjectionMatrix()
        {
            return modelViewMatrix * projectionMatrix;
        }
        public void OnMouseDown(MouseEventArgs e)
        {
            oldX = e.X;
            oldY = e.Y;
        }

        public void OnMouseMove(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                double deltaX = e.X - oldX;
                double deltaY = e.Y - oldY;

                AngX += deltaY * 0.5;
                AngY += deltaX * 0.5;

                oldX = e.X;
                oldY = e.Y;
            }
            else if (e.Button == MouseButtons.Right)
            {
                double deltaX = e.X - oldX;
                double deltaY = e.Y - oldY;

                TlsX += deltaX * 0.01;
                TlsY -= deltaY * 0.01;

                oldX = e.X;
                oldY = e.Y;
            }
        }

        public void OnMouseUp(MouseEventArgs e)
        {
            // Puedes implementar lógica adicional si es necesario
        }

        public void OnMouseWheel(MouseEventArgs e)
        {
            double zoomSpeed = 0.1;
            Scale += e.Delta * zoomSpeed * 0.001;
            if (Scale < 0.1) Scale = 0.1; // Evitar zoom negativo o demasiado pequeño
        }
    }
}
