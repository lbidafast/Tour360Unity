using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace CrizGames.Vour
{
    /// <summary>
    /// InfoPoint
    /// </summary>
    public class InfoPoint : PopupPoint
    {
        public enum InfoPanelImageType
        {
            LeftImage,
            RightImage
        }

        public bool CustomPanel = false;
        public GameObject CustomPanelObject;

        public string Title = "Awesome Title";

        public Sprite Image;
        public InfoPanelImageType PanelType;

        [TextArea(5, 10)]
        public string Text = "Interesting text.";

        public override Transform panelParent => transform.FindChildByTag("PopupPanel");

        protected override void Start()
        {
            if (CustomPanel)
            {
                if (CustomPanelObject == null)
                    Debug.LogError($"Info Point \"{gameObject.name}\" has no Custom Panel Object set!");
                
                for (int i = 0; i < panelParent.childCount; i++)
                    panelParent.GetChild(i).gameObject.SetActive(false);
            }

            panel.localScale = new Vector3(0, 0, 1);
            panel.gameObject.SetActive(false);

            base.Start();

            RotateTowardsPlayer();
        }

        public override void InitPanel()
        {
            if (CustomPanel)
                return;

            name = $"Info Point ({Title})";
            panel.GetChild(1).GetComponent<TextMeshProUGUI>().text = Title;
            panel.GetChild(2).GetComponent<TextMeshProUGUI>().text = Text;
            if (panel.childCount > 3)
                panel.GetChild(3).GetComponent<UnityEngine.UI.Image>().sprite = Image;
        }

        protected override Transform GetPanel()
        {
            if (CustomPanel)
                return CustomPanelObject.transform;
            
            return panelParent.GetChild(0);
        }
        
        public override void RotateTowardsPlayer()
        {
            Vector3 playerPos = Vector3.zero;

            if (Application.isPlaying && PlayerBase.Instance != null)
                playerPos = PlayerBase.Instance.cam.transform.position;
            
            transform.LookAt(playerPos);
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y + 180, 0);

            if (!CustomPanel)
            {
                panelParent.LookAt(playerPos);
                panelParent.eulerAngles = new Vector3(0, panelParent.eulerAngles.y + 180, 0);
            }
            else if (CustomPanelObject != null)
            {
                CustomPanelObject.transform.LookAt(playerPos);
                CustomPanelObject.transform.eulerAngles = new Vector3(0, CustomPanelObject.transform.eulerAngles.y + 180, 0);
            }
        }
    }
}