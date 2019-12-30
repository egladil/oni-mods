using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Egladil
{
    public class YamlColor
    {
        public byte r { get; set; } = 0;
        public byte g { get; set; } = 0;
        public byte b { get; set; } = 0;
        public byte a { get; set; } = 255;

        public static implicit operator UnityEngine.Color32(YamlColor color) => new UnityEngine.Color32(color.r, color.g, color.b, color.a);
    }
}
