using OpenTK.Graphics.OpenGL;

namespace AppMotorGrafico.figuras3d
{
    public class UncPunto
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public UncPunto() { }


        public UncPunto(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

       

        public void Dibujar()
        {
            GL.Vertex3(X, Y, Z);
        }
    }
}
