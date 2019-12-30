using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Egladil
{
    public class Log
    {
        public static bool EnableSpamLog { get; set; } = false;

        private static string AddPrefix(string str)
        {
            if (Mod.KleiMod != null)
            {
                return $"[{Mod.Id}] {str}";
            }

            return $"[{Mod.AssemblyName}] {str}";
        }

        public static void Spam(string str)
        {
            if (EnableSpamLog)
            {
                Debug.Log(AddPrefix(str));
            }
        }

        public static void Info(string str)
        {
            Debug.Log(AddPrefix(str));
        }

        public static void Warn(string str)
        {
            Debug.LogWarning(AddPrefix(str));
        }

        public static void Error(string str)
        {
            Debug.LogError(AddPrefix(str));
        }
    }
}
