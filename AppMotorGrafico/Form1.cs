using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AppMotorGrafico.Pantalla;
using AppMotorGrafico.seializacion;
using AppMotorGrafico.figuras3d;
using AppMotorGrafico.Animaciones;
using AppMotorGrafico.Importacion;

namespace AppMotorGrafico
{
    public partial class Form1 : Form
    {
        private GLControl glControl1;
        private TreeView treeView1;
        private System.Windows.Forms.Timer timer;

        private Escenario escenario;
        private Camara3D camara;
        private MenuStrip menuStrip1;

        private enum ModoTransformacion { Ninguno, Trasladar, Rotar, Escalar }
        private ModoTransformacion modoActual = ModoTransformacion.Ninguno;

        private enum Eje { Ninguno, X, Y, Z }
        private Eje ejeActual = Eje.Ninguno;

        private bool mouseTransforming = false;
        private Point lastMousePos;

        // Lista para manejar múltiples selecciones
        private List<Figura3D> objetosSeleccionados = new List<Figura3D>();

        // Variables para la selección de objetos mediante rectángulo
        private bool isSelecting = false;
        private Point selectionStart;
        private Point selectionEnd;
        private Rectangle selectionRectangle;


        private Libreto libreto;
        private DateTime tiempoInicioAnimacion;
        private Dictionary<string, UncPunto> posicionesIniciales = new Dictionary<string, UncPunto>();
        private Dictionary<string, UncPunto> centrosDeArticulacionIniciales = new Dictionary<string, UncPunto>();
        private bool animacionPausada = false;
        private double tiempoPausa = 0; // Guardará el tiempo en el que se pausó la animación


        public Form1()
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;
            this.WindowState = FormWindowState.Maximized;

            InicializarMenuStrip();
            InicializarTreeView();
            InicializarGLControl();

            camara = new Camara3D();

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 16; //|
            timer.Tick += Timer_Tick;
            timer.Start();

            // Asume que tienes botones con estos nombres en tu formulario
            button1.Click += BtnTrasladar_Click;
            button2.Click += BtnRotar_Click;
            button3.Click += BtnEscalar_Click;
            button4.Click += BtnCargarOBJ_Click;
            button5.Click += BtnAnimarEscena_Click;
            button6.Click += BtnReiniciarAnimacion_Click;

            //button6.Click += BtnReiniciarAnimacion_Click;
            // Crear y ajustar el botón de animación
            Button buttonAnimar = new Button();
            buttonAnimar.Text = "Ejecutar Animación";
            buttonAnimar.Size = new Size(150, 50);
            buttonAnimar.BackColor = Color.LightBlue;
            buttonAnimar.Location = new Point(10, 30); // Posición visible por debajo del menú
            buttonAnimar.Click += BtnAnimarEscena_Click;
            this.Controls.Add(buttonAnimar);
        }

        private async void BtnAnimarEscena_Click(object sender, EventArgs e)
        {
            // Obtener el humanoide y el objetoT1 del escenario
            if (escenario.ObtenerFigura("Humanoide") is UncObjeto humanoideObjeto &&
                escenario.ObtenerFigura("objetoT1") is UncObjeto objetoT1)
            {
                // Obtener la parte rectangular horizontal del objetoT1 y la pelota
                var rectanguloHorizontal = objetoT1.ObtenerElemento("rectanguloHorizontal") as Figura3D;
                var pelota = escenario.ObtenerFigura("pelota") as Figura3D;

                if (rectanguloHorizontal != null && pelota != null)
                {
                    // Obtener las piernas y brazos
                    var piernaIzquierda = humanoideObjeto.ObtenerElemento("PiernaIzquierda") as UncParte;
                    var piernaDerecha = humanoideObjeto.ObtenerElemento("PiernaDerecha") as UncParte;
                    var brazoIzquierdo = humanoideObjeto.ObtenerElemento("BrazoIzquierdo") as UncParte;
                    var brazoDerecho = humanoideObjeto.ObtenerElemento("BrazoDerecho") as UncParte;

                    if (piernaIzquierda != null && piernaDerecha != null && brazoIzquierdo != null && brazoDerecho != null)
                    {
                        // Calcular las posiciones relativas de los centros de articulación de los brazos y piernas respecto al torso
                        var torsoCentroInicial = humanoideObjeto.CalcularCentroDeMasa();
                        var brazoIzquierdoPosRelativo = new UncPunto(
                            brazoIzquierdo.CentroDeArticulacion.X - torsoCentroInicial.X,
                            brazoIzquierdo.CentroDeArticulacion.Y - torsoCentroInicial.Y,
                            brazoIzquierdo.CentroDeArticulacion.Z - torsoCentroInicial.Z
                        );

                        var brazoDerechoPosRelativo = new UncPunto(
                            brazoDerecho.CentroDeArticulacion.X - torsoCentroInicial.X,
                            brazoDerecho.CentroDeArticulacion.Y - torsoCentroInicial.Y,
                            brazoDerecho.CentroDeArticulacion.Z - torsoCentroInicial.Z
                        );

                        var piernaIzquierdaPosRelativo = new UncPunto(
                            piernaIzquierda.CentroDeArticulacion.X - torsoCentroInicial.X,
                            piernaIzquierda.CentroDeArticulacion.Y - torsoCentroInicial.Y,
                            piernaIzquierda.CentroDeArticulacion.Z - torsoCentroInicial.Z
                        );

                        var piernaDerechaPosRelativo = new UncPunto(
                            piernaDerecha.CentroDeArticulacion.X - torsoCentroInicial.X,
                            piernaDerecha.CentroDeArticulacion.Y - torsoCentroInicial.Y,
                            piernaDerecha.CentroDeArticulacion.Z - torsoCentroInicial.Z
                        );

                        // Crear el libreto y las escenas
                        libreto = new Libreto();

                        // Obtener la posición inicial del humanoide y de la parte rectangular horizontal de objetoT1
                        UncPunto posicionInicial = humanoideObjeto.CalcularCentroDeMasa();
                        UncPunto posicionDestino = rectanguloHorizontal.CalcularCentroDeMasa();

                        // Ajustar la posición destino para que esté más a la izquierda y sobre la superficie
                        double offsetX = -0.6;  // Ajuste hacia la izquierda
                        double offsetY = 1.5;   // Ajuste en altura si es necesario para colocarlo sobre la T
                        posicionDestino.X += offsetX;
                        posicionDestino.Y += offsetY;

                        // Calcular el desplazamiento necesario en X, Y y Z
                        double deltaXTotal = posicionDestino.X - posicionInicial.X;
                        double deltaYTotal = posicionDestino.Y - posicionInicial.Y;
                        double deltaZTotal = posicionDestino.Z - posicionInicial.Z;

                        // Definir la cantidad de pasos para una animación más fluida
                        int numPasos = 20;
                        double pasoDeltaX = deltaXTotal / numPasos;
                        double pasoDeltaY = deltaYTotal / numPasos;
                        double pasoDeltaZ = deltaZTotal / numPasos;

                        // Configuración del tiempo de oscilación y duración de cada paso
                        double duracionBalanceo = 0.4;
                        double duracionPaso = 0.8;

                        // Crear las escenas de caminata repetidas
                        for (int i = 0; i < numPasos; i++)
                        {
                            // Escena de caminata con oscilación de brazos y piernas y traslación parcial hacia el objeto
                            Escena escenaCaminata = new Escena();
                            Accion accionCaminata = new Accion(i * duracionPaso, duracionPaso);

                            // Oscilación de brazos y piernas
                            accionCaminata.AgregarTransformacion("MovimientoCaminata" + i, new MovimientoCaminata(
                                piernaIzquierda,
                                piernaDerecha,
                                brazoIzquierdo,
                                brazoDerecho,
                                2,  // Ángulo de balanceo para las piernas
                                1.8,  // Ángulo de balanceo para los brazos
                                () => piernaIzquierda.CentroDeArticulacion,
                                () => piernaDerecha.CentroDeArticulacion,
                                () => brazoIzquierdo.CentroDeArticulacion,
                                () => brazoDerecho.CentroDeArticulacion,
                                duracionBalanceo
                            ));

                            // Traslación parcial en cada paso
                            accionCaminata.AgregarTransformacion("TraslacionPaso" + i, new Traslacion(
                                humanoideObjeto,
                                pasoDeltaX,
                                pasoDeltaY,
                                pasoDeltaZ,
                                duracionPaso,
                                brazoIzquierdoPosRelativo,
                                brazoDerechoPosRelativo,
                                piernaIzquierdaPosRelativo,
                                piernaDerechaPosRelativo
                            ));

                            escenaCaminata.AgregarAccion("CaminataPaso" + i, accionCaminata);

                            // Agregar la escena de cada paso al libreto
                            libreto.AgregarEscena("CaminataPaso" + i, escenaCaminata);
                        }

                        // Escena final: Pateo simulado con rotación de la pierna derecha
                        Escena escenaPateo = new Escena();
                        Accion accionPateo = new Accion(numPasos * duracionPaso, 0.5); // Duración de 0.5 segundos para el pateo

                        // Rotación de la pierna derecha para simular el pateo
                        accionPateo.AgregarTransformacion("PateoPiernaDerecha", new Rotacion(
                            piernaDerecha,
                            0, 0, 30,  // Rotación de 30 grados en el eje Z
                            () => piernaDerecha.CentroDeArticulacion,  // Centro de rotación
                            0.5   // Duración de la rotación
                        ));

                        // Agregar la acción de pateo a la escena de pateo
                        escenaPateo.AgregarAccion("Pateo", accionPateo);

                        // Escena de movimiento parabólico de la pelota
                        Escena escenaMovimientoPelota = new Escena();
                        Accion accionMovimientoPelota = new Accion(numPasos * duracionPaso + 0.5, 1.5); // Duración de 1.5 segundos para el movimiento parabólico

                        // Aumentar la velocidad inicial en Y para lanzar la pelota hacia arriba
                        accionMovimientoPelota.AgregarTransformacion("MovimientoParabolicoPelota", new MovimientoParabolico(
                            pelota,
                            1.0, // Velocidad inicial en X
                            6.0, // Velocidad inicial en Y (aumentada para un lanzamiento hacia arriba)
                            9.8, // Gravedad
                            1.7  // Duración
                        ));

                        // Agregar la acción de movimiento parabólico a la escena de la pelota
                        escenaMovimientoPelota.AgregarAccion("MovimientoPelota", accionMovimientoPelota);

                        // Escena para regresar la pierna derecha a su posición original
                        Escena escenaRegresoPierna = new Escena();
                        Accion accionRegresoPierna = new Accion(numPasos * duracionPaso + 2.0, 0.5); // Duración de 0.5 segundos para regresar la pierna

                        // Rotación inversa para regresar la pierna derecha
                        accionRegresoPierna.AgregarTransformacion("RegresoPiernaDerecha", new Rotacion(
                            piernaDerecha,
                            0, 0, -30,  // Rotación inversa de -30 grados en el eje Z
                            () => piernaDerecha.CentroDeArticulacion,  // Centro de rotación
                            0.5   // Duración de la rotación de regreso
                        ));

                        // Agregar la acción de regreso de la pierna derecha a la escena de regreso
                        escenaRegresoPierna.AgregarAccion("RegresoPierna", accionRegresoPierna);

                        // Agregar las escenas al libreto
                        libreto.AgregarEscena("CaminataCompleta", escenaPateo);
                        libreto.AgregarEscena("MovimientoPelota", escenaMovimientoPelota);
                        libreto.AgregarEscena("RegresoPiernaDerecha", escenaRegresoPierna);

                        // Iniciar la animación
                        tiempoInicioAnimacion = DateTime.Now;
                        await EjecutarEscenaAsincrona(libreto, "CaminataYSimulacionPateoConRegreso");
                    }
                    else
                    {
                        MessageBox.Show("No se encontraron todas las partes necesarias (piernas y brazos) en el modelo del humanoide.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("No se encontró la parte 'RectanguloHorizontal' o la 'pelota' en el escenario.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("El modelo de humanoide o el objetoT1 no fueron encontrados en el escenario.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }






        // Evento Click para cargar archivo .obj
        private async void BtnCargarOBJ_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Archivos OBJ (*.obj)|*.obj";
                openFileDialog.Title = "Seleccionar archivo OBJ";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string rutaArchivo = openFileDialog.FileName;
                    string idObjeto = "Objeto_" + DateTime.Now.Ticks; // ID único para el objeto
                    Color4 colorObjeto = Color4.LightGreen; // Puedes personalizar el color

                    var importer = new OBJImporter();
                    var objeto = await importer.ImportarAsync(rutaArchivo, colorObjeto);

                    if (objeto != null)
                    {
                        // Añadir el objeto importado al escenario
                        escenario.AgregarFigura(idObjeto, objeto);

                        // Ajustar la cámara para que el objeto sea visible
                        //  escenario.AjustarCamara(camara);

                        // Actualizar el TreeView
                        ActualizarTreeView();

                        // Recalcular matrices de la cámara y redibujar
                        camara.IniciarMatrices(glControl1.Width, glControl1.Height);
                        glControl1.Invalidate();

                        MessageBox.Show("Archivo OBJ cargado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Error al cargar el archivo OBJ.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        private UncParte CrearRectanguloParte(double ancho, double alto, double profundidad)
        {
            UncParte rectanguloParte = new UncParte();
            rectanguloParte.Color = Color4.Blue;

            // Crear los vértices del rectángulo (paralelogramo en 3D)
            rectanguloParte.AñadirPoligono("Frontal", CrearRectangulo(
                new UncPunto(0, 0, 0),
                new UncPunto(0, alto, 0),
                new UncPunto(ancho, alto, 0),
                new UncPunto(ancho, 0, 0)
            ));

            rectanguloParte.AñadirPoligono("Trasera", CrearRectangulo(
                new UncPunto(0, 0, -profundidad),
                new UncPunto(0, alto, -profundidad),
                new UncPunto(ancho, alto, -profundidad),
                new UncPunto(ancho, 0, -profundidad)
            ));

            rectanguloParte.AñadirPoligono("LateralIzquierdo", CrearRectangulo(
                new UncPunto(0, 0, 0),
                new UncPunto(0, alto, 0),
                new UncPunto(0, alto, -profundidad),
                new UncPunto(0, 0, -profundidad)
            ));

            rectanguloParte.AñadirPoligono("LateralDerecho", CrearRectangulo(
                new UncPunto(ancho, 0, 0),
                new UncPunto(ancho, alto, 0),
                new UncPunto(ancho, alto, -profundidad),
                new UncPunto(ancho, 0, -profundidad)
            ));

            rectanguloParte.AñadirPoligono("Superior", CrearRectangulo(
                new UncPunto(0, alto, 0),
                new UncPunto(ancho, alto, 0),
                new UncPunto(ancho, alto, -profundidad),
                new UncPunto(0, alto, -profundidad)
            ));

            rectanguloParte.AñadirPoligono("Inferior", CrearRectangulo(
                new UncPunto(0, 0, 0),
                new UncPunto(ancho, 0, 0),
                new UncPunto(ancho, 0, -profundidad),
                new UncPunto(0, 0, -profundidad)
            ));

            return rectanguloParte;
        }

        // Método auxiliar para crear un polígono rectangular
        private UncPoligono CrearRectangulo(UncPunto p1, UncPunto p2, UncPunto p3, UncPunto p4)
        {
            UncPoligono rectangulo = new UncPoligono(Color4.Blue);
            rectangulo.AñadirVertice("v1", p1);
            rectangulo.AñadirVertice("v2", p2);
            rectangulo.AñadirVertice("v3", p3);
            rectangulo.AñadirVertice("v4", p4);
            return rectangulo;
        }

        private void InicializarMenuStrip()
        {
            menuStrip1 = new MenuStrip();
            var archivo = new ToolStripMenuItem("Archivo");
            var nuevo = new ToolStripMenuItem("Nuevo");
            var abrir = new ToolStripMenuItem("Abrir");
            var guardar = new ToolStripMenuItem("Guardar");
            var salir = new ToolStripMenuItem("Salir");
            salir.Click += (s, e) => this.Close();

            archivo.DropDownItems.Add(nuevo);
            archivo.DropDownItems.Add(abrir);
            archivo.DropDownItems.Add(guardar);
            archivo.DropDownItems.Add(new ToolStripSeparator());
            archivo.DropDownItems.Add(salir);

            var opciones = new ToolStripMenuItem("Opciones");
            var ayuda = new ToolStripMenuItem("Ayuda");

            menuStrip1.Items.Add(archivo);
            menuStrip1.Items.Add(opciones);
            menuStrip1.Items.Add(ayuda);
            this.MainMenuStrip = menuStrip1;
            this.Controls.Add(menuStrip1);
        }

        private void InicializarTreeView()
        {
            treeView1 = new TreeView();
            treeView1.Width = 200;
            treeView1.Height = (this.ClientSize.Height / 2) - menuStrip1.Height - 20;
            treeView1.Location = new Point(this.ClientSize.Width - treeView1.Width - 10, menuStrip1.Height + 10);
            treeView1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            treeView1.AfterSelect += TreeView1_AfterSelect;
            this.Controls.Add(treeView1);
        }

        private void InicializarGLControl()
        {
            glControl1 = new GLControl(new GraphicsMode(32, 24, 0, 4));
            glControl1.BackColor = Color.Black;
            glControl1.Location = new Point(0, menuStrip1.Height + 10);
            glControl1.Size = new Size(this.ClientSize.Width - treeView1.Width - 30, this.ClientSize.Height - menuStrip1.Height - 20);
            glControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            glControl1.Load += glControl1_Load;
            glControl1.Paint += glControl1_Paint;
            glControl1.Resize += glControl1_Resize;
            glControl1.MouseDown += GlControl1_MouseDown;
            glControl1.MouseMove += GlControl1_MouseMove;
            glControl1.MouseUp += GlControl1_MouseUp;
            glControl1.MouseWheel += GlControl1_MouseWheel;
            this.Controls.Add(glControl1);
        }

        private void glControl1_Load(object sender, EventArgs e)
        {
            GL.ClearColor(Color4.Black);
            GL.Enable(EnableCap.DepthTest);
            camara.IniciarMatrices(glControl1.Width, glControl1.Height);
            InicializarEscena();
        }





        private async Task EjecutarEscenaAsincrona(Libreto libreto, string nombreEscena)
        {
            tiempoInicioAnimacion = DateTime.Now;

            while (!libreto.EstaCompletado(GetTiempoActual()))
            {
                if (animacionPausada)
                {
                    await Task.Delay(100); // Pausa la animación y espera a que se reanude
                    continue; // Salta a la siguiente iteración sin actualizar la animación
                }

                double tiempoActual = GetTiempoActual();
                libreto.Ejecutar(tiempoActual);
                glControl1.Invalidate();
                await Task.Delay(16); // Refresca cada 16 ms (aprox. 60 fps)
            }
        }

        private double GetTiempoActual()
        {
            if (animacionPausada)
            {
                return tiempoPausa; // Devuelve el tiempo en que se pausó
            }
            return (DateTime.Now - tiempoInicioAnimacion).TotalSeconds;
        }






        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            camara.ConfigurarMatrices();
            escenario.Dibujar();

            //  rectángulo de selección si est selección
            if (isSelecting)
            {
                DrawSelectionRectangle();
            }

            glControl1.SwapBuffers();
        }

        private void glControl1_Resize(object sender, EventArgs e)
        {
            if (glControl1.ClientSize.Height == 0)
                glControl1.ClientSize = new Size(glControl1.ClientSize.Width, 1);

            GL.Viewport(0, 0, glControl1.Width, glControl1.Height);
            camara.IniciarMatrices(glControl1.Width, glControl1.Height);

            treeView1.Height = (this.ClientSize.Height / 2) - menuStrip1.Height - 20;
            treeView1.Location = new Point(this.ClientSize.Width - treeView1.Width - 10, menuStrip1.Height + 10);

            glControl1.Size = new Size(this.ClientSize.Width - treeView1.Width - 30, this.ClientSize.Height - menuStrip1.Height - 20);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            glControl1.Invalidate();
        }
        private void InicializarEscena()
        {
            escenario = new Escenario(Color4.Black);
            Serializador ser = new Serializador();

            // Cargar y configurar el objeto "T"
            Figura3D objetoT1 = ser.Deserializar("ObjetoT");
            objetoT1.Trasladar(3, 4, 0);
            objetoT1.Escalar(1.5, objetoT1.CalcularCentroDeMasa());
            escenario.AgregarFigura("objetoT1", objetoT1);
            posicionesIniciales["objetoT1"] = objetoT1.CalcularCentroDeMasa();

            // Cargar y configurar el humanoide
            Figura3D humanoideFigura = ser.Deserializar("Humanoide");
            humanoideFigura.Trasladar(-5, -1.25, 0);
            humanoideFigura.Rotar(0, 90, 0, humanoideFigura.CalcularCentroDeMasa());
            posicionesIniciales["Humanoide"] = humanoideFigura.CalcularCentroDeMasa();

            if (humanoideFigura is UncObjeto humanoideObjeto)
            {
                // Configurar articulaciones y guardar sus posiciones iniciales

                // Pierna Izquierda
                UncParte piernaIzquierda = humanoideObjeto.ObtenerElemento("PiernaIzquierda") as UncParte;
                if (piernaIzquierda != null)
                {
                    UncPunto caderaIzquierda = new UncPunto(
                        humanoideObjeto.CalcularCentroDeMasa().X,
                        humanoideObjeto.CalcularCentroDeMasa().Y - 0.35,
                        piernaIzquierda.CalcularCentroDeMasa().Z
                    );
                    piernaIzquierda.EstablecerCentroDeArticulacion(caderaIzquierda);
                    centrosDeArticulacionIniciales["PiernaIzquierda"] = caderaIzquierda;
                }

                // Pierna Derecha
                UncParte piernaDerecha = humanoideObjeto.ObtenerElemento("PiernaDerecha") as UncParte;
                if (piernaDerecha != null)
                {
                    UncPunto caderaDerecha = new UncPunto(
                        humanoideObjeto.CalcularCentroDeMasa().X,
                        humanoideObjeto.CalcularCentroDeMasa().Y - 0.35,
                        piernaDerecha.CalcularCentroDeMasa().Z
                    );
                    piernaDerecha.EstablecerCentroDeArticulacion(caderaDerecha);
                    centrosDeArticulacionIniciales["PiernaDerecha"] = caderaDerecha;
                }

                // Brazo Izquierdo
                UncParte brazoIzquierdo = humanoideObjeto.ObtenerElemento("BrazoIzquierdo") as UncParte;
                if (brazoIzquierdo != null)
                {
                    UncPunto hombroIzquierdo = new UncPunto(
                        humanoideObjeto.CalcularCentroDeMasa().X,
                        humanoideObjeto.CalcularCentroDeMasa().Y + 0.35,
                        brazoIzquierdo.CalcularCentroDeMasa().Z
                    );
                    brazoIzquierdo.EstablecerCentroDeArticulacion(hombroIzquierdo);
                    centrosDeArticulacionIniciales["BrazoIzquierdo"] = hombroIzquierdo;
                }

                // Brazo Derecho
                UncParte brazoDerecho = humanoideObjeto.ObtenerElemento("BrazoDerecho") as UncParte;
                if (brazoDerecho != null)
                {
                    UncPunto hombroDerecho = new UncPunto(
                        humanoideObjeto.CalcularCentroDeMasa().X,
                        humanoideObjeto.CalcularCentroDeMasa().Y + 0.35,
                        brazoDerecho.CalcularCentroDeMasa().Z
                    );
                    brazoDerecho.EstablecerCentroDeArticulacion(hombroDerecho);
                    centrosDeArticulacionIniciales["BrazoDerecho"] = hombroDerecho;
                }

                escenario.AgregarFigura("Humanoide", humanoideObjeto);
            }

            // Cargar y configurar la esfera (pelota)
            Figura3D esfera = ser.Deserializar("pelota");
            esfera.Escalar(0.4);
            esfera.Trasladar(1.5, 2.95, 0);
            escenario.AgregarFigura("pelota", esfera);
            posicionesIniciales["pelota"] = esfera.CalcularCentroDeMasa();

            // Actualizar el TreeView
            ActualizarTreeView();
        }

        private void ResetearEscena()
        {
            // Restablecer las posiciones de los objetos al inicio
            foreach (var kvp in posicionesIniciales)
            {
                var nombreFigura = kvp.Key;
                var posicionInicial = kvp.Value;
                var figura = escenario.ObtenerFigura(nombreFigura);
                if (figura != null)
                {
                    var posicionActual = figura.CalcularCentroDeMasa();
                    var dx = posicionInicial.X - posicionActual.X;
                    var dy = posicionInicial.Y - posicionActual.Y;
                    var dz = posicionInicial.Z - posicionActual.Z;

                    figura.Trasladar(dx, dy, dz);
                }
            }

            // Restablecer los centros de articulación a sus valores iniciales
            if (escenario.ObtenerFigura("Humanoide") is UncObjeto humanoideObjeto)
            {
                foreach (var kvp in centrosDeArticulacionIniciales)
                {
                    var nombreParte = kvp.Key;
                    var centroInicial = kvp.Value;
                    var parte = humanoideObjeto.ObtenerElemento(nombreParte) as UncParte;
                    if (parte != null)
                    {
                        parte.EstablecerCentroDeArticulacion(centroInicial);
                    }
                }
            }

            // Reiniciar el tiempo de inicio de la animación
            tiempoInicioAnimacion = DateTime.Now;

            // Si es necesario, reinicia los estados internos de las transformaciones
        }
        private void BtnPausarAnimacion_Click(object sender, EventArgs e)
        {
            if (animacionPausada)
            {
                // Reanudar la animación
                animacionPausada = false;
                tiempoInicioAnimacion = DateTime.Now - TimeSpan.FromSeconds(tiempoPausa); // Ajusta el tiempo para continuar
                ((Button)sender).Text = "Pausar Animación"; // Cambia el texto del botón
            }
            else
            {
                // Pausar la animación
                animacionPausada = true;
                tiempoPausa = (DateTime.Now - tiempoInicioAnimacion).TotalSeconds; // Guarda el tiempo actual
                ((Button)sender).Text = "Reanudar Animación"; // Cambia el texto del botón
            }
        }

        private void BtnReiniciarAnimacion_Click(object sender, EventArgs e)
        {
            ResetearEscena();
            // Volver a ejecutar la animación
            BtnAnimarEscena_Click(null, null);
        }



        private void ActualizarTreeView()
        {
            treeView1.BeginUpdate();
            treeView1.Nodes.Clear();

            foreach (var figuraEntry in escenario.ListarFiguras())
            {
                var objeto = escenario.ObtenerFigura(figuraEntry);
                TreeNode nodoObjeto = new TreeNode(figuraEntry) { Tag = objeto };

                if (objeto is UncObjeto uncObjeto)
                {
                    foreach (var parteEntry in uncObjeto.Partes)
                    {
                        var parte = parteEntry.Value;
                        TreeNode nodoParte = new TreeNode(parteEntry.Key) { Tag = parte };

                        foreach (var poligonoEntry in parte.Poligonos)
                        {
                            var poligono = poligonoEntry.Value;
                            TreeNode nodoPoligono = new TreeNode(poligonoEntry.Key) { Tag = poligono };

                            foreach (var puntoEntry in poligono.Puntos)
                            {
                                var punto = puntoEntry.Value;
                                TreeNode nodoPunto = new TreeNode(puntoEntry.Key) { Tag = punto };
                                nodoPoligono.Nodes.Add(nodoPunto);
                            }

                            nodoParte.Nodes.Add(nodoPoligono);
                        }

                        nodoObjeto.Nodes.Add(nodoParte);
                    }
                }

                treeView1.Nodes.Add(nodoObjeto);
            }

            treeView1.EndUpdate();
            treeView1.ExpandAll();
        }

        private void TreeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var seleccionado = e.Node.Tag;

            // Deseleccionamos todo antes de aplicar la selección
            DeseleccionarTodos();

            // Limpiar la lista de seleccionados
            objetosSeleccionados.Clear();

            // Marcamos lo que seleccionamos como "objetoSeleccionado"
            if (seleccionado is Figura3D figura)
            {
                figura.IsSelected = true;
                objetosSeleccionados.Add(figura);

            }
            else if (seleccionado is UncParte parte)
            {
                parte.IsSelected = true;
                objetosSeleccionados.Add(parte);

            }
            else if (seleccionado is UncPoligono poligono)
            {
                poligono.IsSelected = true;
                objetosSeleccionados.Add(poligono);

            }
            else if (seleccionado is UncPunto punto)
            {
                // No asignamos puntos como objetos seleccionados directamente
                Console.WriteLine($"Vértice seleccionado: {e.Node.Text}");
            }

            glControl1.Invalidate(); // Para refrescar la pantalla después de la selección
        }

        private void DeseleccionarTodos()
        {
            foreach (var figuraEntry in escenario.ListarFiguras())
            {
                var objeto = escenario.ObtenerFigura(figuraEntry);
                objeto.IsSelected = false;

                if (objeto is UncObjeto uncObjeto)
                {
                    foreach (var parte in uncObjeto.Partes.Values)
                    {
                        parte.IsSelected = false;
                        foreach (var poligono in parte.Poligonos.Values)
                        {
                            poligono.IsSelected = false;
                        }
                    }
                }
            }
        }





        private void BtnTrasladar_Click(object sender, EventArgs e)
        {
            modoActual = ModoTransformacion.Trasladar;
        }

        private void BtnRotar_Click(object sender, EventArgs e)
        {
            modoActual = ModoTransformacion.Rotar;
        }

        private void BtnEscalar_Click(object sender, EventArgs e)
        {
            modoActual = ModoTransformacion.Escalar;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.X:
                    ejeActual = Eje.X;
                    break;
                case Keys.Y:
                    ejeActual = Eje.Y;
                    break;
                case Keys.Z:
                    ejeActual = Eje.Z;
                    break;
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.X || e.KeyCode == Keys.Y || e.KeyCode == Keys.Z)
                ejeActual = Eje.Ninguno;
        }

        private void GlControl1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && Control.ModifierKeys.HasFlag(Keys.Shift))
            {
                isSelecting = true;
                selectionStart = e.Location;
                selectionEnd = e.Location;
            }
            else if (e.Button == MouseButtons.Right && modoActual != ModoTransformacion.Ninguno && ejeActual != Eje.Ninguno)
            {
                if (objetosSeleccionados.Count > 0)
                {
                    mouseTransforming = true;
                    lastMousePos = e.Location;
                }
            }
            else
            {
                camara.OnMouseDown(e);
            }
        }

        private void GlControl1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isSelecting)
            {
                selectionEnd = e.Location;
                selectionRectangle = GetRectangleFromPoints(selectionStart, selectionEnd);
                glControl1.Invalidate();
            }
            else if (mouseTransforming && objetosSeleccionados.Count > 0)
            {
                int deltaX = e.X - lastMousePos.X;
                int deltaY = e.Y - lastMousePos.Y;
                lastMousePos = e.Location;

                switch (modoActual)
                {
                    case ModoTransformacion.Trasladar:
                        AplicarTraslacion(deltaX, deltaY);
                        break;
                    case ModoTransformacion.Rotar:
                        AplicarRotacion(deltaX, deltaY);
                        break;
                    case ModoTransformacion.Escalar:
                        AplicarEscalado(deltaX, deltaY);
                        break;
                }

                glControl1.Invalidate();
            }
            else
            {
                camara.OnMouseMove(e);
                glControl1.Invalidate();
            }
        }

        private void GlControl1_MouseUp(object sender, MouseEventArgs e)
        {
            if (isSelecting)
            {
                isSelecting = false;
                PerformSelection();
                glControl1.Invalidate();
            }
            else if (mouseTransforming)
            {
                mouseTransforming = false;
            }
            else
            {
                camara.OnMouseUp(e);
            }
        }

        private void GlControl1_MouseWheel(object sender, MouseEventArgs e)
        {
            camara.OnMouseWheel(e);
            glControl1.Invalidate();
        }

        private Rectangle GetRectangleFromPoints(Point p1, Point p2)
        {
            return new Rectangle(
                Math.Min(p1.X, p2.X),
                Math.Min(p1.Y, p2.Y),
                Math.Abs(p1.X - p2.X),
                Math.Abs(p1.Y - p2.Y));
        }

        private void DrawSelectionRectangle()
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Ortho(0, glControl1.Width, glControl1.Height, 0, -1, 1);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();

            GL.Disable(EnableCap.DepthTest);
            GL.Color4(1.0f, 1.0f, 1.0f, 0.3f); // Blanco semitransparente
            GL.Begin(PrimitiveType.Quads);
            GL.Vertex2(selectionRectangle.Left, selectionRectangle.Top);
            GL.Vertex2(selectionRectangle.Right, selectionRectangle.Top);
            GL.Vertex2(selectionRectangle.Right, selectionRectangle.Bottom);
            GL.Vertex2(selectionRectangle.Left, selectionRectangle.Bottom);
            GL.End();
            GL.Enable(EnableCap.DepthTest);

            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Modelview);
        }

        private void PerformSelection()
        {
            objetosSeleccionados.Clear(); // Limpiar la lista antes de la selección

            foreach (var figuraEntry in escenario.ListarFiguras())
            {
                var objeto = escenario.ObtenerFigura(figuraEntry);

                // Revisar si el objeto completo está dentro de la selección
                if (IsObjectInSelection(objeto))
                {
                    objeto.IsSelected = true;
                    objetosSeleccionados.Add(objeto);
                }
                else
                {
                    objeto.IsSelected = false;
                }

                // Si es un objeto compuesto, revisar sus partes y polígonos
                if (objeto is UncObjeto objetoCompuesto)
                {
                    foreach (var parte in objetoCompuesto.Partes.Values)
                    {
                        if (IsObjectInSelection(parte))
                        {
                            parte.IsSelected = true;
                            objetosSeleccionados.Add(parte);
                        }
                        else
                        {
                            parte.IsSelected = false;
                        }

                        foreach (var poligono in parte.Poligonos.Values)
                        {
                            if (IsObjectInSelection(poligono))
                            {
                                poligono.IsSelected = true;
                                objetosSeleccionados.Add(poligono);
                            }
                            else
                            {
                                poligono.IsSelected = false;
                            }
                        }
                    }
                }
            }

            // Actualizar el TreeView para reflejar la selección
            ActualizarTreeView();
        }

        private bool IsObjectInSelection(Figura3D objeto)
        {
            // Obtener el centro de masa del objeto o parte/polígono
            UncPunto centro = objeto.CalcularCentroDeMasa();

            // Convertir las coordenadas del mundo a coordenadas de clip space
            Vector4 worldPosition = new Vector4((float)centro.X, (float)centro.Y, (float)centro.Z, 1.0f);
            Vector4 clipSpacePos = Vector4.Transform(worldPosition, camara.GetModelViewProjectionMatrix());


            if (clipSpacePos.W == 0)
                return false;

            Vector3 ndcSpacePos = new Vector3(
                clipSpacePos.X / clipSpacePos.W,
                clipSpacePos.Y / clipSpacePos.W,
                clipSpacePos.Z / clipSpacePos.W);


            Point screenPos = new Point(
                (int)(((ndcSpacePos.X + 1.0f) / 2.0f) * glControl1.Width),
                (int)(((1.0f - ndcSpacePos.Y) / 2.0f) * glControl1.Height));

            // Verificar si las coordenadas del objeto están dentro del rectángulo de selección
            return selectionRectangle.Contains(screenPos);
        }


        private void AplicarTraslacion(int deltaX, int deltaY)
        {
            double factor = 0.01;
            double dx = 0, dy = 0, dz = 0;

            switch (ejeActual)
            {
                case Eje.X:
                    dx = deltaX * factor;
                    break;
                case Eje.Y:
                    dy = -deltaY * factor;
                    break;
                case Eje.Z:
                    dz = deltaX * factor;
                    break;
            }

            foreach (var objeto in objetosSeleccionados)
            {
                if (objeto is UncPoligono poligono)  // Si es una cara (polígono)
                {
                    poligono.Trasladar(dx, dy, dz); // Solo trasladamos la cara
                }
                else if (objeto is UncParte parte)  // Si es una parte
                {
                    parte.Trasladar(dx, dy, dz); // Trasladamos toda la parte
                }
                else  // Si es un objeto completo
                {
                    objeto.Trasladar(dx, dy, dz);
                }
            }
        }
        private void AplicarRotacion(int deltaX, int deltaY)
        {
            // Factor de sensibilidad para controlar la velocidad de rotación
            double factor = 0.5;
            double angleX = 0, angleY = 0, angleZ = 0;

            // Asignar el ángulo de rotación dependiendo del eje actual
            switch (ejeActual)
            {
                case Eje.X:
                    angleX = deltaY * factor;
                    break;
                case Eje.Y:
                    angleY = deltaX * factor;
                    break;
                case Eje.Z:
                    angleZ = deltaX * factor;
                    break;
            }



            UncPunto centroGlobal = CalcularCentroDeSeleccion();

            // Aplicar la rotación a cada objeto seleccionado
            foreach (var objeto in objetosSeleccionados)
            {
                objeto.Rotar(angleX, angleY, angleZ, centroGlobal);
            }

            // Redibujar la escena
            glControl1.Invalidate();

        }

        // Método para calcular el centro de masa o centro de los objetos seleccionados
        private UncPunto CalcularCentroDeSeleccion()
        {
            if (objetosSeleccionados.Count == 0)
            {
                return new UncPunto(0, 0, 0);
            }


            double xProm = objetosSeleccionados.Average(obj => obj.CalcularCentroDeMasa().X);
            double yProm = objetosSeleccionados.Average(obj => obj.CalcularCentroDeMasa().Y);
            double zProm = objetosSeleccionados.Average(obj => obj.CalcularCentroDeMasa().Z);

            return new UncPunto(xProm, yProm, zProm);
        }



        private void AplicarEscalado(int deltaX, int deltaY)
        {
            // Definir el factor de escalado basándonos en los movimientos del ratón
            double factor = 1.0 + deltaY * 0.01;


            if (objetosSeleccionados.Count > 0)
            {

                UncPunto centroGlobal = CalcularCentroDeSeleccion();


                foreach (var objeto in objetosSeleccionados)
                {
                    // Paso 1: Trasladar el objeto al origen (respecto al centro de masa)
                    objeto.Trasladar(-centroGlobal.X, -centroGlobal.Y, -centroGlobal.Z);


                    objeto.Escalar(factor, new UncPunto(0, 0, 0));


                    objeto.Trasladar(centroGlobal.X, centroGlobal.Y, centroGlobal.Z);
                }


                glControl1.Invalidate();
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            BtnPausarAnimacion_Click(sender, e);
        }
    }


}
