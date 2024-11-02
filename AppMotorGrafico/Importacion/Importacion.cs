using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using AppMotorGrafico.figuras3d;
using OpenTK.Graphics;

namespace AppMotorGrafico.Importacion
{
    public class OBJImporter
    {
        public async Task<UncObjeto> ImportarAsync(string rutaArchivo, Color4 color)
        {
            return await Task.Run(() =>
            {
                try
                {
                    return Importar(rutaArchivo, color);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al importar el archivo OBJ: {ex.Message}");
                    return null;
                }
            });
        }

        public UncObjeto Importar(string rutaArchivo, Color4 color)
        {
            if (!File.Exists(rutaArchivo))
            {
                Console.WriteLine("El archivo especificado no existe.");
                return null;
            }

            var objeto = new UncObjeto(color);
            var vertices = new List<UncPunto>();
            UncParte parteActual = null;
            var nombreParte = "Parte_0";
            var poligonos = new Dictionary<string, UncPoligono>();
            int indicePoligono = 0;

            using (var reader = new StreamReader(rutaArchivo))
            {
                string linea;

                while ((linea = reader.ReadLine()) != null)
                {
                    linea = linea.Trim();
                    if (linea.StartsWith("#") || string.IsNullOrEmpty(linea))
                    {
                        continue; // Ignorar comentarios y líneas vacías
                    }
                    else if (linea.StartsWith("o ") || linea.StartsWith("g "))
                    {
                        // Nueva parte cuando se detecta un objeto o grupo
                        if (parteActual != null && poligonos.Count > 0)
                        {
                            foreach (var kvp in poligonos)
                            {
                                parteActual.AñadirPoligono(kvp.Key, kvp.Value);
                            }
                            objeto.AñadirParte(nombreParte, parteActual);
                        }

                        // Resetear para la nueva parte
                        nombreParte = linea.Substring(2).Trim();
                        parteActual = new UncParte(color);
                        poligonos.Clear();
                        indicePoligono = 0;
                    }
                    else if (linea.StartsWith("v "))
                    {
                        // Definición de un vértice
                        var partes = linea.Substring(2).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (partes.Length >= 3)
                        {
                            double x = double.Parse(partes[0], CultureInfo.InvariantCulture);
                            double y = double.Parse(partes[1], CultureInfo.InvariantCulture);
                            double z = double.Parse(partes[2], CultureInfo.InvariantCulture);
                            vertices.Add(new UncPunto(x, y, z));
                        }
                    }
                    else if (linea.StartsWith("f "))
                    {
                        // Definición de una cara (polígono)
                        var partes = linea.Substring(2).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        var poligono = new UncPoligono(color);

                        for (int i = 0; i < partes.Length; i++)
                        {
                            var indiceVertice = partes[i].Split('/')[0];
                            int indice = int.Parse(indiceVertice, CultureInfo.InvariantCulture);
                            var vertice = vertices[indice - 1];
                            poligono.AñadirVertice($"v{i}", new UncPunto(vertice.X, vertice.Y, vertice.Z));
                        }

                        poligonos.Add($"Poligono_{indicePoligono}", poligono);
                        indicePoligono++;
                    }
                }

                // Añadir la última parte si existe
                if (parteActual != null && poligonos.Count > 0)
                {
                    foreach (var kvp in poligonos)
                    {
                        parteActual.AñadirPoligono(kvp.Key, kvp.Value);
                    }
                    objeto.AñadirParte(nombreParte, parteActual);
                }
            }

            // Normalizar el objeto para ajustar el tamaño si es necesario
            objeto.Normalizar(2.0);

            return objeto;
        }
    }
}
