using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace CrizGames.Vour
{
    public class TeleportPoint : MonoBehaviour, IInteractable
    {
        public Location targetLocation;

        public bool resetPlayerRotation = false;
        
        private SpriteRenderer sprite;

        protected Color hoverColor = new Color(0.6f, 0.6f, 0.6f);
        private Color startColor;

        protected Location parentLocation;

        public enum TeleportType
        {
            SwitchLocation,
            UpdatePosition
        }
        public TeleportType teleportType;

        public virtual void Awake()
        {
            sprite = GetComponentInChildren<SpriteRenderer>();
            parentLocation = GetComponentInParent<Location>();

            startColor = sprite.color;
        }

        public void Interact()
        {
            Teleport();
        }

        public void Teleport()
        {
            StartCoroutine(TeleportIE());
        }

        private IEnumerator TeleportIE()
        {
            LocationManager manager = LocationManager.GetManager();
            
            yield return manager.PlayBlinkAnim(1f);
            
            if (resetPlayerRotation)
                PlayerBase.Instance.ResetRotation();
            
            if(teleportType == TeleportType.SwitchLocation)
            {
                // Switch location view
                manager.SwitchCurrentLocationView(targetLocation);

                // Update location
                targetLocation.SetData();
                
                // Unload & preload videos
                parentLocation.UnloadLinkedVideos(targetLocation);
                targetLocation.PreloadLinkedVideos();

                // Wait until target location is ready
                LocationView l = LocationManager.GetManager().GetLocationView(targetLocation.locationType, targetLocation.displayType);
                if(!l.IsReady && VourSettings.Instance.blinkWaitUntilLocationIsReady) yield return new WaitUntil(() => l.IsReady);
                else yield return new WaitForSeconds(0.2f);

                // Set locations
                parentLocation.SetActive(false);
                targetLocation.SetActive(true);
            }
            else
            {
                var player = PlayerBase.Instance;
                var playerT = player.transform;
                var cam = player.cam.transform;
                
                Vector3 camOffset = playerT.position - cam.position;
                playerT.position = transform.position + new Vector3(camOffset.x, 0, camOffset.z);
                
                yield return new WaitForSeconds(0.2f);
            }

            yield return manager.PlayBlinkAnim(0f);
        }

        public void OnPointerHoverEnter()
        {
            if (sprite != null)
                sprite.color = hoverColor;
        }

        public void OnPointerHoverExit()
        {
            if (sprite != null)
                sprite.color = startColor;
        }
    }
}