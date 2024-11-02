using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenTK.Graphics;

namespace AppMotorGrafico.seializacion
{
    public class Color4Serializable
    {
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }
        public float A { get; set; }

        public Color4Serializable() { }

        public Color4Serializable(Color4 color)
        {
            R = color.R;
            G = color.G;
            B = color.B;
            A = color.A;
        }

        public Color4 ToColor4()
        {
            return new Color4(R, G, B, A);
        }
    }

    public class Color4Converter : JsonConverter<Color4>
    {
        public override Color4 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var colorSerializable = JsonSerializer.Deserialize<Color4Serializable>(ref reader, options);
            return colorSerializable?.ToColor4() ?? Color4.Black;
        }

        public override void Write(Utf8JsonWriter writer, Color4 value, JsonSerializerOptions options)
        {
            var colorSerializable = new Color4Serializable(value);
            JsonSerializer.Serialize(writer, colorSerializable, options);
        }
    }
}
