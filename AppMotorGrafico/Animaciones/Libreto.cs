using AppMotorGrafico.figuras3d;

namespace AppMotorGrafico.Animaciones
{
    [Serializable]
    public class Libreto
    {
        public BicolaConClave<string, Escena> Escenas { get; private set; }

        public Libreto()
        {
            Escenas = new BicolaConClave<string, Escena>();
        }

        public void AgregarEscena(string clave, Escena escena)
        {
            Escenas.AgregarAlFinal(clave, escena);
        }

        public void Ejecutar(double tiempoActual)
        {
            foreach (var escena in Escenas.Valores)
            {
                escena.Ejecutar(tiempoActual);
            }
        }

        public bool EstaCompletado(double tiempoActual)
        {
            foreach (var escena in Escenas.Valores)
            {
                if (!escena.EstaCompletada(tiempoActual))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
