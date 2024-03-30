using System.Linq;
using System.Globalization;
using UnityEngine;

namespace DramaMask.Extensions;

public static class ColourExtensions
{
    public static string AsConfigString(this Color colour) => $"{(int)colour.r:x2}|{(int)colour.g:x2}|{(int)colour.b:x2}";
    public static Color FromConfigString(this string colourString)
    {
        var colours = colourString.Split('|')
            .Select(hex => int.Parse(hex, NumberStyles.HexNumber)/255f).ToArray();
        return new(colours[0], colours[1], colours[2], byte.MaxValue);
    }
}
