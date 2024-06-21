using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using static CrizGames.Vour.Location;

namespace CrizGames.Vour
{
    public class LocationManager : MonoBehaviour
    {
        private static LocationManager instance;
        
        [SerializeField] private Material blinkMat;
        [SerializeField] private Transform blinkMesh;
        [SerializeField] private Image blinkPanel;
        private readonly int _alpha = Shader.PropertyToID("_Alpha");
        [Space] 
        [SerializeField] private GameObject videoDesktopUI;
        [SerializeField] private GameObject videoVRUI;
        [Space] 
        [SerializeField] private LocationView emptyLoc;
        [SerializeField] private LocationView imageLoc;
        [SerializeField] private LocationView image360Loc;
        [SerializeField] private LocationView videoLoc;
        [SerializeField] private LocationView video360Loc;
        [SerializeField] private LocationView sceneLoc;
        [Space]  
        public Location startLocation;

        private LocationView _currentLocationView;

        private Location[] _locationsCache;
        private Location[] LocationsCache => _locationsCache ??= FindObjectsOfType<Location>(true);

        private void Start()
        {
            if (startLocation == null)
            {
                Debug.LogError("Start location is not assigned.");
                return;
            }

            instance = this;

            SetBlinkState(1f);

            DeactivateLocationViews();
            DeactivateLocations();

            bool hasVideoLocation = false;

            // Init all locations
            foreach (Location l in LocationsCache)
            {
                l.Init();

                if (l.locationType.IsVideo())
                    hasVideoLocation = true;
            }

            // Add VideoUI
            if (hasVideoLocation)
            {
                GameObject videoUIObj = Instantiate(Player.GetPlayerPlatform().IsAnyVR() ? videoVRUI : videoDesktopUI);
                videoUIObj.SetActive(false);
                videoUIObj.transform.root.SetAsLastSibling();
            }

            // Activate StartLocation
            startLocation.SetData();
            SetLocationViewActive(startLocation, true);
            startLocation.SetActive(true);
            _currentLocationView = GetLocationView(startLocation.locationType, startLocation.displayType);
            
            if (hasVideoLocation)
                startLocation.PreloadLinkedVideos();

            PlayBlinkAnim(0f);
        }

        public void DeactivateLocationViews()
        {
            foreach (var l in new [] {emptyLoc, imageLoc, image360Loc, videoLoc, video360Loc, sceneLoc})
                l.gameObject.SetActive(false);
        }

        public void DeactivateLocations()
        {
            foreach (var l in LocationsCache)
                l.SetActive(false);
        }

        public LocationView GetLocationView(Location l) => GetLocationView(l.locationType, l.displayType);

        public LocationView GetLocationView(LocationType loc, DisplayType display)
        {
            switch (loc)
            {
                case LocationType.Empty:
                    return emptyLoc;
                
                case LocationType.Image:
                    switch (display)
                    {
                        case DisplayType._2D:
                        case DisplayType._3D:
                            return imageLoc;
                        case DisplayType._180:
                        case DisplayType._180_3D:
                        case DisplayType._360:
                        case DisplayType._3603D:
                            return image360Loc;
                    }
                    break;
                
                case LocationType.Video:
                    switch (display)
                    {
                        case DisplayType._2D:
                        case DisplayType._3D:
                            return videoLoc;
                        case DisplayType._180:
                        case DisplayType._180_3D:
                        case DisplayType._360:
                        case DisplayType._3603D:
                            return video360Loc;
                    }
                    break;
                
                case LocationType.Scene:
                    return sceneLoc;
            }
            return emptyLoc;
        }
        
        public void SwitchCurrentLocationView(Location newLoc)
        {
            var newView = GetLocationView(newLoc);
            if (_currentLocationView != null)
                SetLocationViewActive(_currentLocationView, false);
            SetLocationViewActive(newView, true);
            _currentLocationView = newView;
        }

        public void SetDataToLocationView(Location l)
        {
            GetLocationView(l.locationType, l.displayType).SetData(l);
        }
        
        public void SetLocationViewActive(Location l, bool active) => SetLocationViewActive(GetLocationView(l.locationType, l.displayType), active);

        public void SetLocationViewActive(LocationView v, bool active)
        {
            v.gameObject.SetActive(active);
        }

        public static LocationManager GetManager()
        {
            if (instance != null)
                return instance;

            GameObject managerGO = GameObject.FindGameObjectWithTag("LocationManager");
            if (!managerGO)
            {
#if UNITY_EDITOR
                if (Application.isPlaying || GameObject.FindGameObjectWithTag("Location") != null)
#endif
                    Debug.LogError("Could not find a Location Manager in scene. You have to add one to your scene!");
                return null;
            }

            return managerGO.GetComponent<LocationManager>();
        }

        public Coroutine PlayBlinkAnim(float targetAlpha)
        {
            StopCoroutine(nameof(DoPlayBlinkAnim));
            return StartCoroutine(DoPlayBlinkAnim(targetAlpha));
        }

        private IEnumerator DoPlayBlinkAnim(float targetAlpha)
        {
            if (PlayerBase.Instance != null)
                blinkMesh.position = PlayerBase.Instance.transform.position;

            float startAlpha = blinkMat.GetFloat(_alpha);

            const float animDuration = 0.1f;
            float time = 0f;
            Color color = new Color(0, 0, 0, startAlpha);
            while (time < animDuration)
            {
                float a = color.a = Mathf.Lerp(startAlpha, targetAlpha, time / animDuration);
                blinkMat.SetFloat(_alpha, a);
                blinkPanel.color = color;

                time += Time.deltaTime;
                yield return null;
            }

            SetBlinkState(targetAlpha);
        }

        private void SetBlinkState(float a)
        {
            blinkMat.SetFloat(_alpha, a);
            blinkPanel.color = new Color(0, 0, 0, a);
        }

        private void OnApplicationQuit()
        {
            blinkMat.SetFloat(_alpha, 0);
        }
    }
}