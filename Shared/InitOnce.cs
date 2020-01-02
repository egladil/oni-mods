using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Egladil
{
    public class EgladilInitOnceHelper : MonoBehaviour
    {

        private Version bestVersion = null;
        private System.Action initCallback;
        private List<Assembly> assemblies = new List<Assembly>();

        public void Register(Assembly assembly, Version version, System.Action initCallback)
        {
            lock (this)
            {
                assemblies.Add(assembly);

                if (bestVersion == null || version > bestVersion)
                {
                    this.bestVersion = version;
                    this.initCallback = initCallback;
                }
            }
        }

        public void ExecuteInitCallback()
        {
            if (initCallback == null) return;

            lock (this)
            {
                if (initCallback != null)
                {
                    var callback = initCallback;
                    initCallback = null;
                    callback();
                }
            }
        }

        public List<Assembly> GetAssemblies()
        {
            lock (this)
            {
                return new List<Assembly>(assemblies);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class PatchOnceAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false)]
    public class UseLatestAttribute : Attribute
    {
        public readonly string Key;

        public UseLatestAttribute(string key)
        {
            Key = key;
        }
    }

    public class InitOnce
    {
        public static readonly Version Version = new Version(1, 0, 1, 0);

        public static bool Patch { get; private set; }

        public static HarmonyInstance HarmonyInstance { get; private set; }

        private static MonoBehaviour Helper;
        private static bool IsHelperOwner;
        private static bool InitHasRun;

        public static void PrePatch(HarmonyInstance harmonyInstance)
        {
            HarmonyInstance = harmonyInstance;

            SetupHelper();

            Traverse.Create(Helper)
                .Method(nameof(EgladilInitOnceHelper.Register), Assembly.GetExecutingAssembly(), Version, (System.Action)InitCallback)
                .GetValue();
        }

        private static void SetupHelper()
        {
            if (Helper != null)
            {
                return;
            }

            var global = Global.Instance.gameObject;
            if ( global == null)
            {
                Log.Error("InitOnce loaded before global gameobject");
            }

            var helper = (MonoBehaviour) global.GetComponent(typeof(EgladilInitOnceHelper).Name);
            if (helper != null)
            {
                Helper = helper;
                return;
            }

            lock (global)
            {
                helper = (MonoBehaviour) global.GetComponent(typeof(EgladilInitOnceHelper).Name);
                if (helper == null)
                {
                    helper = global.AddComponent<EgladilInitOnceHelper>();
                    IsHelperOwner = true;

                    Log.Info($"Added InitOnce helper {helper.GetType().Name} from {Assembly.GetExecutingAssembly().GetName().Name}");
                }
            }

            Helper = helper;
        }

        private static void InitCallback()
        {
            Assembly self = Assembly.GetExecutingAssembly();
            List<Assembly> assemblies = Traverse.Create(Helper)
                    .Method(nameof(EgladilInitOnceHelper.GetAssemblies))
                    .GetValue<List<Assembly>>();

            Log.Info($"Using InitOnce version {Version} from {self.GetName().Name}");

            ApplyPatches(self, assemblies);
            RedirectToLatest(self, assemblies);

            InitComplete(self, assemblies);
        }

        private static void ApplyPatches(Assembly self, List<Assembly> assemblies)
        {
            Patch = true;

            int count = 0;
            
            foreach (var type in self.GetTypes())
            {
                if (type.GetCustomAttributes(typeof(PatchOnceAttribute), false).Length == 0)
                {
                    continue;
                }

                var harmonyMethods = type.GetHarmonyMethods();
                if (harmonyMethods == null || harmonyMethods.Count == 0)
                {
                    continue;
                }

                Log.Spam($"Applying patches in {type.FullName}");

                new PatchProcessor(HarmonyInstance, type, HarmonyMethod.Merge(harmonyMethods)).Patch();
                count++;
            }

            Log.Info($"Applied {count} patches");
        }

        private static void RedirectToLatest(Assembly self, List<Assembly> assemblies)
        {
            Dictionary<string, FieldInfo> targets = new Dictionary<string, FieldInfo>();
            int targetCount = 0;

            foreach (var type in self.GetTypes())
            {
                foreach (var field in type.GetFields())
                {
                    if (!field.IsStatic)
                    {
                        continue;
                    }

                    var attributes = field.GetCustomAttributes(typeof(UseLatestAttribute), false).Cast<UseLatestAttribute>();
                    bool added = false;
                    foreach (var attribute in attributes)
                    {
                        if (targets.ContainsKey(attribute.Key))
                        {
                            Log.Error($"Duplicate redirection key {attribute.Key} for {type.FullName}.{field.Name} in {self.GetName().Name}");
                            continue;
                        }

                        targets.Add(attribute.Key, field);
                        added = true;
                    }

                    if (added)
                    {
                        targetCount++;
                    }
                }
            }

            Log.Info($"Found {targetCount} redirection targets in {self.GetName().Name}");

            foreach (Assembly assembly in assemblies)
            {
                if (assembly == self) continue;

                Log.Spam($"Reirecting in {assembly.GetName().Name}");

                int count = 0;

                foreach (var type in assembly.GetTypes())
                {
                    foreach (var field in type.GetFields())
                    {
                        if (!field.IsStatic)
                        {
                            continue;
                        }

                        var keys = (from attr in field.GetCustomAttributes(false)
                                         where attr.GetType().FullName == typeof(UseLatestAttribute).FullName
                                         select (string) attr.GetType().GetField("Key").GetValue(attr))
                                         .ToList();
                        if (keys.Count == 0)
                        {
                            continue;
                        }

                        FieldInfo target = null;
                        foreach (var key in keys)
                        {
                            if (targets.TryGetValue(key, out target))
                            {
                                break;
                            }
                        }

                        if (target == null)
                        {
                            Log.Error($"Could not redirect {type.FullName}.{field.Name} in {assembly.GetName().Name}");
                            continue;
                        }

                        Log.Spam($"Redirecting {assembly.GetName().Name}:{type.FullName}.{field.Name} to {self.GetName().Name}:{target.DeclaringType.FullName}.{target.Name}");

                        field.SetValue(null, target.GetValue(null));

                        count++;
                    }
                }

                Log.Info($"Redirected {count} fields in {assembly.GetName().Name}");
            }
        }

        private static void InitComplete(Assembly self, List<Assembly> assemblies)
        {
            var parameters = new Type[] { };
            var arguments = new object[] { };

            foreach (Assembly assembly in assemblies)
            {
                int count = 0;

                foreach (var type in assembly.GetTypes())
                {
                    var onInitOnceComplete = type.GetMethod("OnInitOnceComplete", parameters);
                    if (onInitOnceComplete != null)
                    {
                        Log.Spam($"Calling {type.FullName}.{onInitOnceComplete.Name} in {assembly.GetName().Name}");
                        onInitOnceComplete.Invoke(null, arguments);
                        count++;
                    }
                }

                Log.Info($"Called {count} instances of OnInitOnceComplete in {assembly.GetName().Name}");
            }
        }

        [HarmonyPatch(typeof(GlobalResources), nameof(GlobalResources.Instance), new Type[] { })]
        public static class GlobalResources_Instance_Patch
        {
            public static bool Prepare() => IsHelperOwner || Log.EnableSpamLog;

            public static bool Prefix()
            {
                if (InitHasRun)
                {
                    return true;
                }
                InitHasRun = true;

                if (IsHelperOwner)
                {
                    Traverse.Create(Helper)
                        .Method(nameof(EgladilInitOnceHelper.ExecuteInitCallback))
                        .GetValue();
                }

                if (!Patch)
                {
                    Log.Spam($"Ignored InitOnce version {Version} from {Assembly.GetExecutingAssembly().GetName().Name}");
                }

                return true;
            }
        }
    }
}
