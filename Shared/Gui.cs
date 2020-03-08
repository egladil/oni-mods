using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Egladil
{
    public class Gui
    {
        private static readonly DefaultControls.Resources resources;

        public static GameObject Create(string name, GameObject prefab = null, GameObject parent = null)
        {
            Log.Info($"Gui.Create {name}");

            var gameObject = prefab != null ? GameObject.Instantiate(prefab) : new GameObject(name);

            if (parent != null)
            {
                gameObject.transform.SetParent(parent.transform);
            }

            var rect = gameObject.AddOrGet<RectTransform>();
            rect.localScale = Vector3.one;

            gameObject.AddOrGet<CanvasRenderer>();
            gameObject.layer = LayerMask.NameToLayer("UI");

            return gameObject;
        }

        public static T Create<T>(GameObject prefab = null, GameObject parent = null) where T : Component
        {
            var gameObject = Create(typeof(T).Name, prefab, parent);
            return gameObject.AddOrGet<T>();
        }

        public static GameObject CreatePanel(GameObject parent = null)
        {
            var panel = DefaultControls.CreatePanel(resources);

            if (parent != null)
            {
                panel.transform.SetParent(parent.transform);
            }

            panel.layer = LayerMask.NameToLayer("UI");
            panel.GetComponent<Image>().color = new Color(0, 0, 0, 0);

            return panel;
        }
    }
}
