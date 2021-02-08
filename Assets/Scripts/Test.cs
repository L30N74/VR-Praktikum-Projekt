using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class Test : MonoBehaviour
{
    Camera mainCamera;
    GestureHandler gestureHandler;

    private InputDevice leftController_device;
    private InputDevice rightController_device;
   

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;

        leftController_device = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        rightController_device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        gestureHandler = GameObject.FindGameObjectWithTag("LogicManager").GetComponent<GestureHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        leftController_device.TryGetFeatureValue(CommonUsages.gripButton, out bool left_gripped);
        rightController_device.TryGetFeatureValue(CommonUsages.gripButton, out bool right_gripped);

        if (left_gripped)
            gestureHandler.RockFall("left");
        else if(right_gripped) {
            gestureHandler.RockFall("right");
        }
    }
}
