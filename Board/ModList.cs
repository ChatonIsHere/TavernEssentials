using System;
using System.Reflection;
using HarmonyLib;
using MelonLoader.Utils;

namespace TavernEssentials.Board
{
    // The mods this game runs, {id: version} as JSON, which a join sends the server (set_mods) as the Tavern
    // Launcher does. A server that requires mods refuses a join without it. TavernLib builds it
    // (ModParity.LocalMods); TavernEssentials doesn't reference TavernLib, so it's found by name. Null without
    // a TavernLib that has it.
    static class ModList
    {
        static MethodInfo _local;
        static bool _looked;

        public static string Read()
        {
            try
            {
                if (!_looked)
                {
                    _looked = true;
                    Type parity = AccessTools.TypeByName("TavernLib.Backend.Auth.ModParity");
                    _local = parity == null ? null : AccessTools.Method(parity, "LocalMods", new Type[] { typeof(string) });
                    if (_local == null)
                        Log.Msg("join: this TavernLib can't list your mods, so servers that require mods will refuse board joins.");
                }
                if (_local == null) return null;
                return _local.Invoke(null, new object[] { MelonEnvironment.GameRootDirectory }) as string;
            }
            catch (Exception e)
            {
                Exception why = e is TargetInvocationException && e.InnerException != null ? e.InnerException : e;
                Log.Warn("join: could not list your mods for the server (" + why.Message + "); joining without them.");
                return null;
            }
        }
    }
}
