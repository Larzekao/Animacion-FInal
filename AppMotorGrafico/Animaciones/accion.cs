using System.Collections.Generic;

namespace AppMotorGrafico.Animaciones
{
    [Serializable]
    public class Accion
    {
        public BicolaConClave<string, Transformacion> Transformaciones { get; private set; }
        public double TiempoInicio { get; private set; }
        public double Duracion { get; private set; }

        public Accion(double tiempoInicio, double duracion)
        {
            Transformaciones = new BicolaConClave<string, Transformacion>();
            this.TiempoInicio = tiempoInicio;
            this.Duracion = duracion;
        }

        public void AgregarTransformacion(string clave, Transformacion transformacion)
        {
            Transformaciones.AgregarAlFinal(clave, transformacion);
            transformacion.TiempoInicio = TiempoInicio;
            transformacion.Duracion = Duracion;
        }

        public void Ejecutar(double tiempoActual)
        {
            foreach (var transformacion in Transformaciones.Valores)
            {
                transformacion.EjecutarInterpolado(tiempoActual);
            }
        }

        public bool EstaCompletada(double tiempoActual)
        {
            return tiempoActual >= TiempoInicio + Duracion;
        }
    }
}
