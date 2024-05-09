using DramaMask.Models.Network;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

namespace DramaMask.Extensions;

public static class LinqExtensions
{
    public static string AsString<S, T>(this Dictionary<S, T> dict)
        => dict == null ? null : string.Join(", ", dict.Select(p => $"{p.Key}: {p.Value}"));

    public static string AsString<S>(this NetworkList<S> list) where S : unmanaged, System.IEquatable<S>
        => string.Join(", ", (List<S>) [.. list]);

    public static T GetValueOrDefault<S, T>(this Dictionary<S, T> dict, S key, T defaultVal = default)
        => dict.ContainsKey(key) ? dict[key] : defaultVal;

    public static Data GetNetworkData<Data>(this Dictionary<ulong, Data> map, Data local, bool isLocal, ulong id)
        where Data : NetworkData
        => isLocal ? local : map.GetValueOrDefault(id);
}
