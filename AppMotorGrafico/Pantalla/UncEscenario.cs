using System;
using System.Collections.Generic;
using AppMotorGrafico.figuras3d;
using AppMotorGrafico.Importacion;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace AppMotorGrafico.Pantalla
{
    public class Escenario
    {
        private Dictionary<string, Figura3D> figuras;
        private Dictionary<int, object> idToObject; // Mapeo de ID a objeto
        public Color4 FondoColor { get; set; }
        private PlanoCartesiano plano = new PlanoCartesiano(0.1, 0.02);

        // Eventos para notificar cambios en el escenario
        public event Action<string, Figura3D> FiguraAgregada;
        public event Action<string> FiguraEliminada;

        public Escenario(Color4 fondoColor)
        {
            figuras = new Dictionary<string, Figura3D>();
            idToObject = new Dictionary<int, object>();
            FondoColor = fondoColor;
        }
        public Dictionary<string, Figura3D> ObtenerDiccionarioFiguras()
        {
            return figuras;
        }



        // Método para agregar una figura al escenario
        public bool AgregarFigura(string id, Figura3D figura)
        {
            if (figura == null)
                return false;

            if (figuras.ContainsKey(id))
            {
                Console.WriteLine($"Una figura con el ID '{id}' ya existe. No se añadió.");
                return false; // Evitar duplicados
            }

            figuras[id] = figura;
            FiguraAgregada?.Invoke(id, figura); // Notificar que una figura ha sido agregada
            return true;
        }

        // Método para eliminar una figura
        public bool EliminarFigura(string id)
        {
            if (figuras.Remove(id))
            {
                FiguraEliminada?.Invoke(id); // Notificar que una figura ha sido eliminada
                return true;
            }

            Console.WriteLine($"No se encontró la figura con el ID '{id}' para eliminar.");
            return false;
        }

        // Método para obtener una figura por su id
        public Figura3D ObtenerFigura(string id)
        {
            if (figuras.TryGetValue(id, out Figura3D figura))
            {
                return figura;
            }

            Console.WriteLine($"No se encontró la figura con el ID '{id}'.");
            return null;
        }

        // Método para calcular el centro de masa de todas las figuras
        public UncPunto CalcularCentroDeMasa()
        {
            if (figuras.Count == 0)
                return new UncPunto();

            var centros = new List<UncPunto>();
            foreach (var figura in figuras.Values)
            {
                if (figura is UncObjeto objeto)
                {
                    centros.Add(objeto.CalcularCentroDeMasa());
                }
            }

            if (centros.Count == 0)
                return new UncPunto();

            double xProm = centros.Average(p => p.X);
            double yProm = centros.Average(p => p.Y);
            double zProm = centros.Average(p => p.Z);

            return new UncPunto(xProm, yProm, zProm);
        }

        // Método para listar todas las figuras
        public List<string> ListarFiguras()
        {
            return new List<string>(figuras.Keys);
        }

        // Método para dibujar una figura específica
        public void DibujarFigura(string id)
        {
            if (figuras.TryGetValue(id, out Figura3D figura))
            {
                figura.Dibujar();
            }
            else
            {
                Console.WriteLine($"No se encontró la figura con el ID '{id}'.");
            }
        }

        // Método para dibujar todas las figuras del escenario
        public void Dibujar()
        {
            GL.ClearColor(FondoColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Dibujar el plano cartesiano
            plano.Dibujar();

            // Dibujar todas las figuras
            foreach (var figura in figuras.Values)
            {
                figura.Dibujar();
            }
        }

        // Método para trasladar todas las figuras
        public void TrasladarTodas(double tx, double ty, double tz)
        {
            foreach (var figura in figuras.Values)
            {
                figura.Trasladar(tx, ty, tz);
            }
        }

        // Método para escalar todas las figuras
        public void EscalarTodas(double factor)
        {
            foreach (var figura in figuras.Values)
            {
                figura.Escalar(factor);
            }
        }

        // Método para rotar todas las figuras
        public void RotarTodas(double anguloX, double anguloY, double anguloZ, UncPunto centro)
        {
            foreach (var figura in figuras.Values)
            {
                figura.Rotar(anguloX, anguloY, anguloZ, centro);
            }
        }
    }
}
