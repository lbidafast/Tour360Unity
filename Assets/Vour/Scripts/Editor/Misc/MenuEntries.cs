using UnityEditor;
using UnityEngine;

namespace CrizGames.Vour.Editor
{
    public static class MenuEntries
    {

        [MenuItem("Vour/Setup")]
        public static void ShowSetupWindow()
        {
            SetupWindow.ShowWindow();
        }
        
        [MenuItem("Vour/Add Location Manager", false, 10), MenuItem("GameObject/Vour/Location Manager")]
        public static void AddLocationManager()
        {
            var managerInScene = LocationManager.GetManager();
            if (managerInScene != null)
            {
                EditorUtility.DisplayDialog("Info", "A Location Manager object is already in the scene!", "Okay");
                Selection.activeObject = managerInScene.gameObject;
                return;
            }

            GameObject manager = (GameObject) PrefabUtility.InstantiatePrefab(VourSettings.Instance.locationManagerPrefab);
            manager.transform.SetAsFirstSibling();
            Selection.activeGameObject = manager;
        }

        private static void AddLocation(Location.LocationType type)
        {
            var manager = LocationManager.GetManager();

            // If it doesn't exist yet, add one
            if (manager == null)
                AddLocationManager();

            // Add location to scene
            GameObject location = (GameObject) PrefabUtility.InstantiatePrefab(VourSettings.Instance.locationPrefab);
            location.transform.SetAsLastSibling();
            Selection.activeGameObject = location;

            Location o = location.GetComponent<Location>();
            o.locationType = type;
        }

        private static void AddObject(GameObject obj)
        {
            Selection.activeGameObject = (GameObject) PrefabUtility.InstantiatePrefab(obj, Selection.activeTransform);
        }

        [MenuItem("Vour/Add Player", false, 10)]
        [MenuItem("GameObject/Vour/Player")]
        public static void AddPlayer()
        {
            Player playerInScene = Object.FindObjectOfType<Player>();
            if (playerInScene != null)
            {
                EditorUtility.DisplayDialog("Info", "A Player object is already in the scene!", "Okay");
                Selection.activeObject = playerInScene.gameObject;
                return;
            }

            GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(VourSettings.Instance.playerPrefab);
            Selection.activeGameObject = player;
        }

        #region Locations

        // EMPTY
        [MenuItem("Vour/Add Location/Empty", false, 1)]
        [MenuItem("GameObject/Vour/Location/Empty Location", false, 10)]
        public static void AddLocationEmpty() => AddLocation(Location.LocationType.Empty);

        // IMAGE
        [MenuItem("Vour/Add Location/Image", false, 1)]
        [MenuItem("GameObject/Vour/Location/Image Location", false, 10)]
        public static void AddLocationImage() => AddLocation(Location.LocationType.Image);

        // // IMAGE 3D
        // [MenuItem("Vour/Add Location/Image 3D", false, 1)]
        // [MenuItem("GameObject/Vour/Location/Image 3D Location", false, 10)]
        // public static void AddLocationImage3D() => AddLocation(Location.LocationType.Image3D);
        //
        // // IMAGE 180
        // [MenuItem("Vour/Add Location/Image 180", false, 1)]
        // [MenuItem("GameObject/Vour/Location/Image 180 Location", false, 10)]
        // public static void AddLocationImage180() => AddLocation(Location.LocationType.Image180);
        //
        // // IMAGE 3D 180
        // [MenuItem("Vour/Add Location/Image 3D 180", false, 1)]
        // [MenuItem("GameObject/Vour/Location/Image 3D 180 Location", false, 10)]
        // public static void AddLocationImage3D180() => AddLocation(Location.LocationType.Image3D180);
        //
        // // IMAGE 360
        // [MenuItem("Vour/Add Location/Image 360", false, 1)]
        // [MenuItem("GameObject/Vour/Location/Image 360 Location", false, 10)]
        // public static void AddLocationImage360() => AddLocation(Location.LocationType.Image360);
        //
        // // IMAGE 3D 360
        // [MenuItem("Vour/Add Location/Image 3D 360", false, 1)]
        // [MenuItem("GameObject/Vour/Location/Image 3D 360 Location", false, 10)]
        // public static void AddLocationImage3D360() => AddLocation(Location.LocationType.Image3D360);

        // VIDEO
        [MenuItem("Vour/Add Location/Video", false, 1)]
        [MenuItem("GameObject/Vour/Location/Video Location", false, 10)]
        public static void AddLocationVideo() => AddLocation(Location.LocationType.Video);

        // // VIDEO 3D
        // [MenuItem("Vour/Add Location/Video 3D", false, 1)]
        // [MenuItem("GameObject/Vour/Location/Video 3D Location", false, 10)]
        // public static void AddLocationVideo3D() => AddLocation(Location.LocationType.Video3D);
        //
        // // VIDEO 180
        // [MenuItem("Vour/Add Location/Video 180", false, 1)]
        // [MenuItem("GameObject/Vour/Location/Video 180 Location", false, 10)]
        // public static void AddLocationVideo180() => AddLocation(Location.LocationType.Video180);
        //
        // // VIDEO 3D 180
        // [MenuItem("Vour/Add Location/Video 3D 180", false, 1)]
        // [MenuItem("GameObject/Vour/Location/Video 3D 180 Location", false, 10)]
        // public static void AddLocationVideo3D180() => AddLocation(Location.LocationType.Video3D180);
        //
        // // VIDEO 360
        // [MenuItem("Vour/Add Location/Video 360", false, 1)]
        // [MenuItem("GameObject/Vour/Location/Video 360 Location", false, 10)]
        // public static void AddLocationVideo360() => AddLocation(Location.LocationType.Video360);
        //
        // // VIDEO 3D 360
        // [MenuItem("Vour/Add Location/Video 3D 360", false, 1)]
        // [MenuItem("GameObject/Vour/Location/Video 3D 360 Location", false, 10)]
        // public static void AddLocationVideo3D360() => AddLocation(Location.LocationType.Video3D360);

        // SCENE
        [MenuItem("Vour/Add Location/Scene", false, 1)]
        [MenuItem("GameObject/Vour/Location/Scene Location", false, 10)]
        public static void AddLocationScene() => AddLocation(Location.LocationType.Scene);

        #endregion

        #region Placeables

        [MenuItem("Vour/Add Teleport Point", false, 1)]
        [MenuItem("GameObject/Vour/Teleport Point", false, 10)]
        public static void AddTeleportPoint() => AddObject(VourSettings.Instance.defaultTeleportPoint);

        [MenuItem("Vour/Add Info Point", false, 1)]
        [MenuItem("GameObject/Vour/Info Point", false, 10)]
        public static void AddInfoPoint() => AddObject(VourSettings.Instance.defaultInfoPoint);

        [MenuItem("Vour/Add Info Panel", false, 1)]
        [MenuItem("GameObject/Vour/Info Panel", false, 10)]
        public static void AddInfoPanel() => AddObject(VourSettings.Instance.defaultInfoPanel);

        [MenuItem("Vour/Add Video Point", false, 1)]
        [MenuItem("GameObject/Vour/Video Point", false, 10)]
        public static void AddVideoPoint() => AddObject(VourSettings.Instance.defaultVideoPoint);

        [MenuItem("Vour/Add Video Panel", false, 1)]
        [MenuItem("GameObject/Vour/Video Panel", false, 10)]
        public static void AddVideoPanel() => AddObject(VourSettings.Instance.defaultVideoPanel);

        #endregion

        [MenuItem("Vour/Center Editor Camera &c", false, 3000)]
        public static void CenterEditorCam()
        {
            SceneView view = SceneView.lastActiveSceneView;
            if (view != null)
            {
                Camera target = view.camera;
                target.transform.position = Vector3.zero;
                target.transform.rotation = Quaternion.identity;
                view.AlignViewToObject(target.transform);
            }
        }

        [MenuItem("Vour/Open Online Documentation", false, 4000)]
        public static void OpenOnlineDocs()
        {
            Application.OpenURL("https://crizgames.gitbook.io/vour/");
        }
    }
}