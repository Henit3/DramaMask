using GameNetcodeStuff;
using System.Linq;

namespace DramaMask.Extensions;

public static class StartOfRoundExtensions
{
    public static PlayerControllerB GetPlayer(this StartOfRound instance, ulong id)
    {
        return instance?.allPlayerScripts.FirstOrDefault(p => p.GetId() == id);
    }
}
