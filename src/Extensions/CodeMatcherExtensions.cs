using System.Reflection.Emit;
using HarmonyLib;

namespace DramaMask.Extensions;

public static class CodeMatcherExtensions
{
    public static bool MatchLabel(this CodeMatcher matcher, Label label)
    {
        while (matcher.Remaining > 0)
        {
            if (matcher.Instruction.labels.Contains(label)) return true;
            matcher.Advance(1);
        }
        return false;
    }
}