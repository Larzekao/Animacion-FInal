using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using System;

namespace AppMotorGrafico.Pantalla
{
    public class PlanoCartesiano
    {
        private double escala;
        private double width;

        public PlanoCartesiano(double escala = 1f, double width = 10f)
        {
            this.escala = escala;
            this.width = width;
        }

        public void Dibujar()
        {
            PrimitiveType tipo = PrimitiveType.LineLoop;

            // Dibujar eje X
            GL.Begin(tipo);
            GL.Color4(Color4.Red);
            GL.Vertex3(4.0, 0.0, 0.0);
            GL.Vertex3(-4.0, 0.0, 0.0);
            GL.End();

            for (double i = -4.0; i <= 4.0; i += escala)
            {
                GL.Begin(PrimitiveType.Lines);
                GL.Color4(Color4.Red);
                GL.Vertex3(i, -width, 0.0);
                GL.Vertex3(i, width, 0.0);
                GL.End();
            }

            // Dibujar eje Y
            GL.Begin(tipo);
            GL.Color4(Color4.Green);
            GL.Vertex3(0.0, 4.0, 0.0);
            GL.Vertex3(0.0, -4.0, 0.0);
            GL.End();

            for (double i = -4.0; i <= 4.0; i += escala)
            {
                GL.Begin(PrimitiveType.Lines);
                GL.Color4(Color4.Green);
                GL.Vertex3(-width, i, 0.0);
                GL.Vertex3(width, i, 0.0);
                GL.End();
            }

            // Dibujar eje Z
            GL.Begin(tipo);
            GL.Color4(Color4.Blue);
            GL.Vertex3(0.0, 0.0, 4.0);
            GL.Vertex3(0.0, 0.0, -4.0);
            GL.End();

            for (double i = -4.0; i <= 4.0; i += escala)
            {
                GL.Begin(PrimitiveType.Lines);
                GL.Color4(Color4.Blue);
                GL.Vertex3(0.0, -width, i);
                GL.Vertex3(0.0, width, i);
                GL.End();
            }
        }
    }
}
