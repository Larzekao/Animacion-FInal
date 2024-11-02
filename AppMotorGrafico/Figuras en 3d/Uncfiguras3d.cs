namespace AppMotorGrafico.figuras3d
{
    public interface Figura3D
    {
        void Trasladar(double tx, double ty, double tz);
        void Escalar(double factor);
        void Escalar(double factor, UncPunto centro);
       

        void Rotar(double anguloX, double anguloY, double anguloZ, UncPunto centro);
       
        bool IsSelected { get; set; }
        UncPunto CalcularCentroDeMasa();

        UncPunto Centro { get; set; }
        Figura3D ObtenerElemento(string id);
        void Dibujar();
    }
    

}
