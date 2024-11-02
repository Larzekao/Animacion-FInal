using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics.OpenGL;


namespace AppMotorGrafico.figuras3d      
{
    public class UncPoligono : Figura3D
    {
        public Dictionary<string, UncPunto> Puntos { get; set; }
        public Color4 Color { get; set; }
        public bool IsSelected { get; set; } = false;
        public UncPunto Centro { get; set; }


        public UncPoligono() { }
        public UncPoligono(Color4 color)
        {
            Puntos = new Dictionary<string, UncPunto>();
            Color = color;
        }

        public void AñadirVertice(string id, UncPunto punto)
        {
            Puntos[id] = punto;
        }
        public void CambiarColor(Color4 nuevoColor)
        {
            this.Color = nuevoColor;
        }
        // Implementación de ObtenerElemento
        public Figura3D ObtenerElemento(string id)
        {
            //if (this.Puntos.ContainsKey(id))
            //    return this.Puntos[id];
            //else
               return null;
        }

        public bool EliminarVertice(string id)
        {
            return Puntos.Remove(id);
        }
        // Nuevo método para obtener un vértice por su ID
        public UncPunto ObtenerVertice(string id)
        {
            if (Puntos.TryGetValue(id, out UncPunto punto))
            {
                return punto;
            }
            else
            {
                Console.WriteLine($"El vértice con ID '{id}' no existe en este polígono.");
                return null;
            }
        }

        public UncPunto CalcularCentroDeMasa()
        {
            if (Puntos.Count == 0)
                return new UncPunto();

            double xProm = Puntos.Values.Average(p => p.X);
            double yProm = Puntos.Values.Average(p => p.Y);
            double zProm = Puntos.Values.Average(p => p.Z);
            this.Centro = new UncPunto(xProm,yProm,zProm);
            return new UncPunto(xProm, yProm, zProm);
        }


        private void TransformarPuntos(Matrix4 matriz)
        {
            foreach (var punto in Puntos.Values)
            {
                Vector4 vector = new Vector4((float)punto.X, (float)punto.Y, (float)punto.Z, 1.0f);
                Vector4 resultado = Vector4.Transform(vector, matriz);
                punto.X = resultado.X;
                punto.Y = resultado.Y;
                punto.Z = resultado.Z;
            }
        }
     
        public void Trasladar(double tx, double ty, double tz)
        {
            Matrix4 matrizTraslacion = Matrix4.CreateTranslation((float)tx, (float)ty, (float)tz);
            TransformarPuntos(matrizTraslacion);
        }

        public void Escalar(double factor)
        {
            UncPunto centro = CalcularCentroDeMasa();
            Escalar(factor, centro);
        }

        public void Escalar(double factor, UncPunto centro)
        {
            Matrix4 trasladarOrigen = Matrix4.CreateTranslation(-(float)centro.X, -(float)centro.Y, -(float)centro.Z);
            Matrix4 matrizEscala = Matrix4.CreateScale((float)factor);
            Matrix4 regresar = Matrix4.CreateTranslation((float)centro.X, (float)centro.Y, (float)centro.Z);
            Matrix4 matrizTransformacion = trasladarOrigen * matrizEscala * regresar;

            TransformarPuntos(matrizTransformacion);
        }

        public void Rotar(double anguloX, double anguloY, double anguloZ)
        {
            UncPunto centro = CalcularCentroDeMasa();
            Rotar(anguloX, anguloY, anguloZ, centro);
        }

        public void Rotar(double anguloX, double anguloY, double anguloZ, UncPunto centro)
        {
            Matrix4 trasladarOrigen = Matrix4.CreateTranslation(-(float)centro.X, -(float)centro.Y, -(float)centro.Z);
            Matrix4 rotarX = Matrix4.CreateRotationX(MathHelper.DegreesToRadians((float)anguloX));
            Matrix4 rotarY = Matrix4.CreateRotationY(MathHelper.DegreesToRadians((float)anguloY));
            Matrix4 rotarZ = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians((float)anguloZ));
            Matrix4 regresar = Matrix4.CreateTranslation((float)centro.X, (float)centro.Y, (float)centro.Z);
            Matrix4 matrizTransformacion = trasladarOrigen * rotarX * rotarY * rotarZ * regresar;

            TransformarPuntos(matrizTransformacion);
        }
        public void Escalar(double factorX, double factorY, double factorZ, UncPunto centro)
        {
            Matrix4 trasladarOrigen = Matrix4.CreateTranslation(-(float)centro.X, -(float)centro.Y, -(float)centro.Z);
            Matrix4 matrizEscala = Matrix4.CreateScale((float)factorX, (float)factorY, (float)factorZ);
            Matrix4 regresar = Matrix4.CreateTranslation((float)centro.X, (float)centro.Y, (float)centro.Z);
            Matrix4 matrizTransformacion = trasladarOrigen * matrizEscala * regresar;

            TransformarPuntos(matrizTransformacion);
        }

        public void Dibujar()
        {
            if (IsSelected)
            {
                GL.Color4(Color4.White); // Resaltar si está seleccionado
            }
            else
            {
                GL.Color4(Color); // Dibujar con el color original
            }

            GL.Begin(PrimitiveType.Lines);
            foreach (var punto in Puntos.Values)
            {
                GL.Vertex3(punto.X, punto.Y, punto.Z);
            }
            GL.End();
        }

    }
}
