using AppMotorGrafico.figuras3d;
using System;

namespace AppMotorGrafico.Animaciones
{
    [Serializable]
    public class Escena
    {
        public BicolaConClave<string, Accion> Acciones { get; private set; }

        public Escena()
        {
            Acciones = new BicolaConClave<string, Accion>();
        }

        public void AgregarAccion(string clave, Accion accion)
        {
            Acciones.AgregarAlFinal(clave, accion);
        }

        public void Ejecutar(double tiempoActual)
        {
            foreach (var accion in Acciones.Valores)
            {
                if (tiempoActual >= accion.TiempoInicio && !accion.EstaCompletada(tiempoActual))
                {
                    accion.Ejecutar(tiempoActual);
                }
            }
        }

        public bool EstaCompletada(double tiempoActual)
        {
            foreach (var accion in Acciones.Valores)
            {
                if (!accion.EstaCompletada(tiempoActual))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
