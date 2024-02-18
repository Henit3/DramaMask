using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

namespace DramaMask.Extensions;

public static class LinqExtensions
{
    public static string AsString<S, T>(this Dictionary<S, T> dict) => dict is null ? null
        : string.Join(", ", dict.Select(p => $"[{p.Key}: {p.Value}]"));

    public static string AsString<S>(this NetworkList<S> list) where S : unmanaged, System.IEquatable<S>
    {
        List<S> output = [.. list];
        return string.Join(", ", output);
    }

    public static void AddSafe<S, T>(this Dictionary<S, T> dict, S key, T value)
    {
        if (dict is null) return;
        if (!dict.ContainsKey(key)) dict.Add(key, value);
    }
}
