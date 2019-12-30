using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Egladil
{
    public class Mod
    {
        public static KMod.Mod KleiMod { get; private set; }

        public static string Id { get => KleiMod.label.id; }
        public static string AssemblyName { get => Assembly.GetExecutingAssembly().GetName().Name; }
        public static Version AssemblyVersion { get => Assembly.GetExecutingAssembly().GetName().Version; }

        public static void OnLoad(string path)
        {
            foreach(var mod in Global.Instance.modManager.mods)
            {
                if (mod.label.install_path == path)
                {
                    KleiMod = mod;
                }
            }

            if (KleiMod == null)
            {
                Log.Error($"Could not find mod info for {path}");
                return;
            }

            Log.Info($"Loaded {Id} ({AssemblyName}: {AssemblyVersion}");
        }
    }
}
