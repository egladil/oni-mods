using Harmony;
using Klei;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Egladil
{
    public class Yaml
    {
        public interface Collection<T>
        {
            T[] Entries { get; }
        }

        public static List<T> CollectFromYAML<T>(string path)
        {
            List<T> list = new List<T>();
            var files = ListPool<FileHandle, Yaml>.Allocate();
            FileSystem.GetFiles(FileSystem.Normalize(path), "*.yaml", files);
            FileSystem.GetFiles(FileSystem.Normalize(path), "*.yml", files);
            var errors = ListPool<YamlIO.Error, Yaml>.Allocate();

            Log.Info($"Loading {files.Count} files from {path}");

            foreach (FileHandle file in files)
            {
                Log.Info($"Loading {file.full_path} files");

                T obj = YamlIO.LoadFile<T>(file.full_path, delegate (YamlIO.Error error, bool force_log_as_warning)
                {
                    errors.Add(error);
                });
                if (obj != null)
                {
                    list.Add(obj);
                }
            }
            files.Recycle();
            if (Global.Instance != null && Global.Instance.modManager != null)
            {
                Global.Instance.modManager.HandleErrors(errors);
            }
            errors.Recycle();
            return list;
        }

        public static List<T> CollectFromYAML<T, C>(string path) where C : Collection<T>
        {
            var list = new List<T>();
            var collections = CollectFromYAML<C>(path);
            foreach (C collection in collections)
            {
                list.AddRange(collection.Entries);
            }
            return list;
        }
    }
}
