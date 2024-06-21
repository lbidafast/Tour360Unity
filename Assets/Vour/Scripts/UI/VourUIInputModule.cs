using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif
#if VOUR_XRI
using UnityEngine.XR.Interaction.Toolkit.UI;
#endif

namespace CrizGames.Vour
{
#if VOUR_XRI
    public class VourUIInputModule : XRUIInputModule
    {
        protected override void OnEnable()
        {
            //AssignDefaultActions();
            
            base.OnEnable();
        }
        
        // bool InputActionReferencesAreSet()
        // {
        //     return (pointAction != null &&
        //             leftClickAction != null &&
        //             rightClickAction != null &&
        //             middleClickAction != null &&
        //             navigateAction != null &&
        //             submitAction != null &&
        //             cancelAction != null &&
        //             scrollWheelAction != null);
        // }
        //
        // private void AssignDefaultActions()
        // {
        //     if(InputActionReferencesAreSet())
        //         return;
        //     
        //     var defaultActions = new XRIDefaultInputActions();
        //     pointAction = InputActionReference.Create(defaultActions.XRIUI.Point);
        //     leftClickAction = InputActionReference.Create(defaultActions.XRIUI.Click);
        //     rightClickAction = InputActionReference.Create(defaultActions.XRIUI.RightClick);
        //     middleClickAction = InputActionReference.Create(defaultActions.XRIUI.MiddleClick);
        //     navigateAction = InputActionReference.Create(defaultActions.XRIUI.Navigate);
        //     submitAction = InputActionReference.Create(defaultActions.XRIUI.Submit);
        //     cancelAction = InputActionReference.Create(defaultActions.XRIUI.Cancel);
        //     scrollWheelAction = InputActionReference.Create(defaultActions.XRIUI.ScrollWheel);
        // }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            //AssignDefaultActions();
            
            base.OnValidate();
        }
#endif
    }
#elif ENABLE_INPUT_SYSTEM
    public class VourUIInputModule : InputSystemUIInputModule {}
#else
    public class VourUIInputModule : MonoBehaviour {}
#endif
}