using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using System.Text.Json;
using System.IO;
using System;

namespace AppMotorGrafico.figuras3d
{
    public class UncObjeto : Figura3D
    {
        public Dictionary<string, UncParte> Partes { get;  set; }
        public Color4 Color { get; set; }
        public bool IsSelected { get; set; } = false;

        public UncPunto Centro { get; set; }
        public UncObjeto()
        {
            // No inicializar Partes aquí
        }
        public UncObjeto(Color4 color)
        {
            Partes = new Dictionary<string, UncParte>();
            Color = color;

        }

        public void AñadirParte(string id, UncParte parte)
        {
            Partes[id] = parte;
            this.CalcularCentroDeMasa();
        }

        public bool EliminarParte(string id)
        {
            return Partes.Remove(id);
        }
        // Nuevo método para obtener una parte por su ID
        public UncParte ObtenerParte(string id)
        {
            if (Partes.TryGetValue(id, out UncParte parte))
            {
                return parte;
            }
            else
            {
                Console.WriteLine($"La parte con ID '{id}' no existe en este objeto.");
                return null;
            }
        }

        public UncPunto CalcularCentroDeMasa()
        {
            if (Partes == null || Partes.Count == 0)
                return new UncPunto();

            var centros = Partes.Values.Select(p => p.CalcularCentroDeMasa()).ToList();

            double xProm = centros.Average(p => p.X);
            double yProm = centros.Average(p => p.Y);
            double zProm = centros.Average(p => p.Z);

            return new UncPunto(xProm, yProm, zProm);
        }


        public void Trasladar(double tx, double ty, double tz)
        {
            foreach (var parte in Partes.Values)
            {
                parte.Trasladar(tx, ty, tz);
            }
        }

        public void Escalar(double factor)
        {
             Centro = CalcularCentroDeMasa();
            Escalar(factor, Centro);
        }

        public void Escalar(double factor, UncPunto centro)
        {
            foreach (var parte in Partes.Values)
            {
                parte.Escalar(factor, centro);
            }
        }

        public void Rotar(double anguloX, double anguloY, double anguloZ)
        {
             Centro = CalcularCentroDeMasa();
            Rotar(anguloX, anguloY, anguloZ, Centro);
        }

        public  void Rotar(double anguloX, double anguloY, double anguloZ, UncPunto centro)
        {
            // Rotar cada parte alrededor del centro del objeto
            foreach (var parte in Partes.Values)
            {
                parte.Trasladar(-centro.X, -centro.Y, -centro.Z);
                parte.Rotar(anguloX, anguloY, anguloZ, new UncPunto(0, 0, 0)); // Rotar en torno al origen
                parte.Trasladar(centro.X, centro.Y, centro.Z); // Regresar a su lugar
            }
        }

        public Figura3D ObtenerElemento(string id)
        {
            if (Partes.ContainsKey(id))
                return Partes[id];
            else
                return null;
        }
        public void Normalizar(double tamañoObjetivo)
        {
            // Obtener el bounding box del objeto
            double minX = double.MaxValue, minY = double.MaxValue, minZ = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue, maxZ = double.MinValue;

            foreach (var parte in Partes.Values)
            {
                foreach (var poligono in parte.Poligonos.Values)
                {
                    foreach (var punto in poligono.Puntos.Values)
                    {
                        if (punto.X < minX) minX = punto.X;
                        if (punto.Y < minY) minY = punto.Y;
                        if (punto.Z < minZ) minZ = punto.Z;
                        if (punto.X > maxX) maxX = punto.X;
                        if (punto.Y > maxY) maxY = punto.Y;
                        if (punto.Z > maxZ) maxZ = punto.Z;
                    }
                }
            }

            // Calcular el centro del bounding box
            double centroX = (minX + maxX) / 2.0;
            double centroY = (minY + maxY) / 2.0;
            double centroZ = (minZ + maxZ) / 2.0;

            // Calcular el tamaño del objeto
            double tamMax = Math.Max(maxX - minX, Math.Max(maxY - minY, maxZ - minZ));

            // Calcular el factor de escala
            double escala = 1.0;
            if (tamMax > 0)
            {
                escala = tamañoObjetivo / tamMax;
            }

            // Trasladar y escalar todas las partes
            Trasladar(-centroX, -centroY, -centroZ);
            Escalar(escala, new UncPunto(0, 0, 0));
        }

        private UncParte ConvertirObjetoEnParte(UncObjeto objeto)
        {
            // Crear una nueva parte con el color del objeto
            UncParte nuevaParte = new UncParte(objeto.Color);

            // Recorrer todas las partes del objeto original
            foreach (var parteEntry in objeto.Partes)
            {
                UncParte parteOriginal = parteEntry.Value;

                // Recorrer todos los polígonos de la parte original
                foreach (var poligonoEntry in parteOriginal.Poligonos)
                {
                    UncPoligono poligonoOriginal = poligonoEntry.Value;

                    // Crear una copia del polígono
                    UncPoligono poligonoCopiado = new UncPoligono(poligonoOriginal.Color);
                    foreach (var verticeEntry in poligonoOriginal.Puntos)
                    {
                        poligonoCopiado.AñadirVertice(verticeEntry.Key, verticeEntry.Value);
                    }

                    // Añadir el polígono copiado a la nueva parte
                    string idPoligonoCopiado = "Poligono_" + Guid.NewGuid().ToString();
                    nuevaParte.AñadirPoligono(idPoligonoCopiado, poligonoCopiado);
                }
            }

            return nuevaParte;
        }
        public void ActualizarArticulaciones()
        {
            var centroHumanoide = this.CalcularCentroDeMasa();

            // Actualizar articulación de la pierna izquierda
            if (ObtenerElemento("PiernaIzquierda") is UncParte piernaIzquierda)
            {
                piernaIzquierda.EstablecerCentroDeArticulacion(new UncPunto(
                    centroHumanoide.X - 0.3,  // Ajuste en X para el centro de la cadera izquierda
                    centroHumanoide.Y - 0.35, // Ajuste en Y para la cadera
                    centroHumanoide.Z         // Mantener el mismo valor de Z
                ));
            }

            // Actualizar articulación de la pierna derecha
            if (ObtenerElemento("PiernaDerecha") is UncParte piernaDerecha)
            {
                piernaDerecha.EstablecerCentroDeArticulacion(new UncPunto(
                    centroHumanoide.X + 0.3,  // Ajuste en X para el centro de la cadera derecha
                    centroHumanoide.Y - 0.35, // Ajuste en Y para la cadera
                    centroHumanoide.Z
                ));
            }

            // Actualizar articulación del brazo izquierdo
            if (ObtenerElemento("BrazoIzquierdo") is UncParte brazoIzquierdo)
            {
                brazoIzquierdo.EstablecerCentroDeArticulacion(new UncPunto(
                    centroHumanoide.X - 0.5,  // Ajuste en X para el hombro izquierdo
                    centroHumanoide.Y + 0.5,  // Ajuste en Y para el hombro
                    centroHumanoide.Z
                ));
            }

            // Actualizar articulación del brazo derecho
            if (ObtenerElemento("BrazoDerecho") is UncParte brazoDerecho)
            {
                brazoDerecho.EstablecerCentroDeArticulacion(new UncPunto(
                    centroHumanoide.X + 0.5,  // Ajuste en X para el hombro derecho
                    centroHumanoide.Y + 0.5,  // Ajuste en Y para el hombro
                    centroHumanoide.Z
                ));
            }
        }
        public void Escalar2(double factorX, double factorY, double factorZ, UncPunto centro)
        {
            foreach (var parte in Partes.Values)
            {
                parte.Escalar2(factorX, factorY, factorZ, centro);
            }
        }

        public void Dibujar()
        {
            if (IsSelected)
            {
                GL.Color4(Color4.Yellow); // Resaltar si está seleccionado
            }
            else
            {
                GL.Color4(Color); // Dibujar con el color original
            }

            foreach (var parte in Partes.Values)
            {
                parte.Dibujar();
            }
        }


    }
}
