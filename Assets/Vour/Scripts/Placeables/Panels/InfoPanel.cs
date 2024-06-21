using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace CrizGames.Vour
{
    public class InfoPanel : Panel
    {
        public bool customPanel = false;
        public GameObject customPanelObject;

        public string title = "Awesome Title";

        public Sprite image;
        public InfoPoint.InfoPanelImageType panelType;

        [TextArea(5, 10)]
        public string text = "Interesting text.";

        public override Transform panelParent => transform;

        /// <summary>
        /// Start
        /// </summary>
        protected override void Start()
        {
            if (customPanel && customPanelObject == null)
                Debug.LogError($"Info Point \"{gameObject.name}\" has no Custom Panel Object set!");

            base.Start();
        }

        /// <summary>
        /// InitPanel
        /// </summary>
        public override void InitPanel()
        {
            if (customPanel)
                return;

            name = $"Info Panel ({title})";
            panel.GetChild(1).GetComponent<TextMeshProUGUI>().text = title;
            panel.GetChild(2).GetComponent<TextMeshProUGUI>().text = text;
            if (panel.childCount > 3)
                panel.GetChild(3).GetComponent<UnityEngine.UI.Image>().sprite = image;
        }

        /// <summary>
        /// FindPanel
        /// </summary>
        /// <returns>Transform of panel</returns>
        protected override Transform GetPanel()
        {
            if (transform.childCount > 0)
                return transform.GetChild(0);
            return null;
        }
    }
}
