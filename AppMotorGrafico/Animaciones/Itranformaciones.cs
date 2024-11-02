using AppMotorGrafico.figuras3d;
using AppMotorGrafico.Pantalla;
using System;
using System.Text.Json.Serialization;

namespace AppMotorGrafico.Animaciones
{
    public abstract class Transformacion
    {
        public double Duracion { get; set; }
        public double TiempoInicio { get; set; }

        public abstract void EjecutarInterpolado(double tiempoActual);
    }
    public class Traslacion : Transformacion
    {
        private Figura3D objeto;
        private double deltaX, deltaY, deltaZ;
        private double posXInicial, posYInicial, posZInicial;
        private double posXAnterior, posYAnterior, posZAnterior;

        // Posiciones relativas de los centros de articulación de los brazos y piernas
        private UncPunto brazoIzquierdoPosRelativo;
        private UncPunto brazoDerechoPosRelativo;
        private UncPunto piernaIzquierdaPosRelativo;
        private UncPunto piernaDerechaPosRelativo;

        public Traslacion(Figura3D objeto, double deltaX, double deltaY, double deltaZ, double duracion,
                          UncPunto brazoIzqPosRelativo, UncPunto brazoDerPosRelativo,
                          UncPunto piernaIzqPosRelativo, UncPunto piernaDerPosRelativo)
        {
            this.objeto = objeto;
            this.deltaX = deltaX;
            this.deltaY = deltaY;
            this.deltaZ = deltaZ;
            this.Duracion = duracion;
            CalcularPosicionInicial();
            posXAnterior = posXInicial;
            posYAnterior = posYInicial;
            posZAnterior = posZInicial;

            // Guardar las posiciones relativas iniciales de los brazos y piernas
            this.brazoIzquierdoPosRelativo = brazoIzqPosRelativo;
            this.brazoDerechoPosRelativo = brazoDerPosRelativo;
            this.piernaIzquierdaPosRelativo = piernaIzqPosRelativo;
            this.piernaDerechaPosRelativo = piernaDerPosRelativo;
        }

        private void CalcularPosicionInicial()
        {
            var centro = objeto.CalcularCentroDeMasa();
            this.posXInicial = centro.X;
            this.posYInicial = centro.Y;
            this.posZInicial = centro.Z;
        }

        public override void EjecutarInterpolado(double tiempoActual)
        {
            if (tiempoActual < TiempoInicio) return;

            double progreso = Math.Min(1.0, (tiempoActual - TiempoInicio) / Duracion);

            double desplazamientoX = deltaX * progreso;
            double desplazamientoY = deltaY * progreso;
            double desplazamientoZ = deltaZ * progreso;

            double dx = desplazamientoX - (posXAnterior - posXInicial);
            double dy = desplazamientoY - (posYAnterior - posYInicial);
            double dz = desplazamientoZ - (posZAnterior - posZInicial);

            // Actualizar la posición anterior
            posXAnterior = posXInicial + desplazamientoX;
            posYAnterior = posYInicial + desplazamientoY;
            posZAnterior = posZInicial + desplazamientoZ;

            objeto.Trasladar(dx, dy, dz);

            // Recalcular los centros de articulación en función de la posición actual del torso
            if (objeto is UncObjeto uncObjeto)
            {
                var torsoCentro = uncObjeto.CalcularCentroDeMasa();

                UncParte brazoIzquierdo = uncObjeto.ObtenerElemento("BrazoIzquierdo") as UncParte;
                UncParte brazoDerecho = uncObjeto.ObtenerElemento("BrazoDerecho") as UncParte;
                UncParte piernaIzquierda = uncObjeto.ObtenerElemento("PiernaIzquierda") as UncParte;
                UncParte piernaDerecha = uncObjeto.ObtenerElemento("PiernaDerecha") as UncParte;

                if (brazoIzquierdo != null)
                {
                    var nuevoCentroIzq = new UncPunto(
                        torsoCentro.X + brazoIzquierdoPosRelativo.X,
                        torsoCentro.Y + brazoIzquierdoPosRelativo.Y,
                        torsoCentro.Z + brazoIzquierdoPosRelativo.Z
                    );
                    brazoIzquierdo.EstablecerCentroDeArticulacion(nuevoCentroIzq);
                }

                if (brazoDerecho != null)
                {
                    var nuevoCentroDer = new UncPunto(
                        torsoCentro.X + brazoDerechoPosRelativo.X,
                        torsoCentro.Y + brazoDerechoPosRelativo.Y,
                        torsoCentro.Z + brazoDerechoPosRelativo.Z
                    );
                    brazoDerecho.EstablecerCentroDeArticulacion(nuevoCentroDer);
                }

                if (piernaIzquierda != null)
                {
                    var nuevoCentroPiernaIzq = new UncPunto(
                        torsoCentro.X + piernaIzquierdaPosRelativo.X,
                        torsoCentro.Y + piernaIzquierdaPosRelativo.Y,
                        torsoCentro.Z + piernaIzquierdaPosRelativo.Z
                    );
                    piernaIzquierda.EstablecerCentroDeArticulacion(nuevoCentroPiernaIzq);
                }

                if (piernaDerecha != null)
                {
                    var nuevoCentroPiernaDer = new UncPunto(
                        torsoCentro.X + piernaDerechaPosRelativo.X,
                        torsoCentro.Y + piernaDerechaPosRelativo.Y,
                        torsoCentro.Z + piernaDerechaPosRelativo.Z
                    );
                    piernaDerecha.EstablecerCentroDeArticulacion(nuevoCentroPiernaDer);
                }
            }
        }
    }

    public class Rotacion : Transformacion
    {
        private Figura3D objeto;
        private double anguloX, anguloY, anguloZ;
        private Func<UncPunto> obtenerCentro;
        private double anguloXInicial, anguloYInicial, anguloZInicial;
        private double anguloXAnterior, anguloYAnterior, anguloZAnterior;

        public Rotacion(Figura3D objeto, double anguloX, double anguloY, double anguloZ, Func<UncPunto> obtenerCentro, double duracion)
        {
            this.objeto = objeto;
            this.anguloX = anguloX;
            this.anguloY = anguloY;
            this.anguloZ = anguloZ;
            this.obtenerCentro = obtenerCentro;
            this.Duracion = duracion;
            anguloXInicial = 0;
            anguloYInicial = 0;
            anguloZInicial = 0;
            anguloXAnterior = anguloXInicial;
            anguloYAnterior = anguloYInicial;
            anguloZAnterior = anguloZInicial;
        }

        public override void EjecutarInterpolado(double tiempoActual)
        {
            if (tiempoActual < TiempoInicio) return;

            double progreso = Math.Min(1.0, (tiempoActual - TiempoInicio) / Duracion);

            double rotacionX = anguloX * progreso;
            double rotacionY = anguloY * progreso;
            double rotacionZ = anguloZ * progreso;

            double deltaX = rotacionX - anguloXAnterior;
            double deltaY = rotacionY - anguloYAnterior;
            double deltaZ = rotacionZ - anguloZAnterior;

            anguloXAnterior = rotacionX;
            anguloYAnterior = rotacionY;
            anguloZAnterior = rotacionZ;

            UncPunto centroActual = obtenerCentro(); // Obtener el centro dinámicamente

            objeto.Rotar(deltaX, deltaY, deltaZ, centroActual);
        }
    }



    public class MovimientoParabolico : Transformacion
    {
        private Figura3D objeto;
        private double velocidadInicialX;
        private double velocidadInicialY;
        private double gravedad;
        private double tiempoAnterior;

        public MovimientoParabolico(Figura3D objeto, double velocidadInicialX, double velocidadInicialY, double gravedad, double duracion)
        {
            this.objeto = objeto;
            this.velocidadInicialX = velocidadInicialX;
            this.velocidadInicialY = velocidadInicialY;
            this.gravedad = gravedad;
            this.Duracion = duracion;
            this.tiempoAnterior = 0.0;
        }

        public override void EjecutarInterpolado(double tiempoActual)
        {
            if (tiempoActual < TiempoInicio) return;

            double t = tiempoActual - TiempoInicio;

            if (t > Duracion)
                t = Duracion;

            // Calcular la posición actual en X y Y
            double x = velocidadInicialX * t;
            double y = velocidadInicialY * t - 0.5 * gravedad * t * t;

            // Calcular la posición anterior en X y Y
            double xAnterior = velocidadInicialX * tiempoAnterior;
            double yAnterior = velocidadInicialY * tiempoAnterior - 0.5 * gravedad * tiempoAnterior * tiempoAnterior;

            // Ca
            double deltaX = x - xAnterior;
            double deltaY = y - yAnterior;

            // Aplicar los desplazamientos al objeto
            objeto.Trasladar(deltaX, deltaY, 0.0);

            tiempoAnterior = t;


        }
    }






    public class Escalado : Transformacion
    {
        private Figura3D objeto;
        private double factorInicial, factorFinal;
        private UncPunto centro;
        private double factorAnterior;
        private double tiempoOffset;

        public Escalado(Figura3D objeto, double factorFinal, UncPunto centro, double duracion, double tiempoOffset = 0.0)
        {
            this.objeto = objeto;
            this.factorFinal = factorFinal;
            this.centro = centro;
            this.Duracion = duracion;
            this.factorInicial = 1.0;
            this.factorAnterior = factorInicial;
            this.tiempoOffset = tiempoOffset;
        }

        public override void EjecutarInterpolado(double tiempoActual)
        {
            if (tiempoActual < TiempoInicio + tiempoOffset) return;

            double tiempoRelativo = tiempoActual - TiempoInicio - tiempoOffset;
            double progreso = Math.Min(1.0, tiempoRelativo / Duracion);

            double factorActual = factorInicial + (factorFinal - factorInicial) * progreso;
            double factorEscala = factorActual / factorAnterior;

            objeto.Escalar(factorEscala, centro);

            factorAnterior = factorActual;
        }
    }



    public class MovimientoCaminata : Transformacion
    {
        private Figura3D piernaIzquierda;
        private Figura3D piernaDerecha;
        private Figura3D brazoIzquierdo;
        private Figura3D brazoDerecho;
        private double anguloMaximoPiernas;
        private double anguloMaximoBrazos;
        private Func<UncPunto> obtenerCentroPiernaIzquierda;
        private Func<UncPunto> obtenerCentroPiernaDerecha;
        private Func<UncPunto> obtenerCentroBrazoIzquierdo;
        private Func<UncPunto> obtenerCentroBrazoDerecho;

        public MovimientoCaminata(
            Figura3D piernaIzquierda,
            Figura3D piernaDerecha,
            Figura3D brazoIzquierdo,
            Figura3D brazoDerecho,
            double anguloMaximoPiernas,
            double anguloMaximoBrazos,
            Func<UncPunto> obtenerCentroPiernaIzquierda,
            Func<UncPunto> obtenerCentroPiernaDerecha,
            Func<UncPunto> obtenerCentroBrazoIzquierdo,
            Func<UncPunto> obtenerCentroBrazoDerecho,
            double duracion)
        {
            this.piernaIzquierda = piernaIzquierda;
            this.piernaDerecha = piernaDerecha;
            this.brazoIzquierdo = brazoIzquierdo;
            this.brazoDerecho = brazoDerecho;
            this.anguloMaximoPiernas = anguloMaximoPiernas;
            this.anguloMaximoBrazos = anguloMaximoBrazos;
            this.obtenerCentroPiernaIzquierda = obtenerCentroPiernaIzquierda;
            this.obtenerCentroPiernaDerecha = obtenerCentroPiernaDerecha;
            this.obtenerCentroBrazoIzquierdo = obtenerCentroBrazoIzquierdo;
            this.obtenerCentroBrazoDerecho = obtenerCentroBrazoDerecho;
            this.Duracion = duracion;
        }

        public override void EjecutarInterpolado(double tiempoActual)
        {
            if (tiempoActual < TiempoInicio) return;

            double progreso = (tiempoActual - TiempoInicio) / Duracion;
            double anguloActualPiernaIzquierda = anguloMaximoPiernas * Math.Sin(progreso * Math.PI * 2);
            double anguloActualPiernaDerecha = anguloMaximoPiernas * Math.Sin((progreso + 0.5) * Math.PI * 2);
            double anguloActualBrazoIzquierdo = anguloMaximoBrazos * Math.Sin((progreso + 0.5) * Math.PI * 2);
            double anguloActualBrazoDerecho = anguloMaximoBrazos * Math.Sin(progreso * Math.PI * 2);

            // Aplicar la rotación a las piernas
            piernaIzquierda.Rotar(0, 0, anguloActualPiernaIzquierda, obtenerCentroPiernaIzquierda());
            piernaDerecha.Rotar(0, 0, anguloActualPiernaDerecha, obtenerCentroPiernaDerecha());

            // Aplicar la rotación a los brazos
            brazoIzquierdo.Rotar(0, 0, anguloActualBrazoIzquierdo, obtenerCentroBrazoIzquierdo());
            brazoDerecho.Rotar(0, 0, anguloActualBrazoDerecho, obtenerCentroBrazoDerecho());
        }
    }




}


