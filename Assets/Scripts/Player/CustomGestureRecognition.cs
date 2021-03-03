using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using AOT;
using TMPro;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class CustomGestureRecognition : MonoBehaviour
{
    public Transform leftController;
    private InputDevice leftController_device;
    public Transform rightController;
    private InputDevice rightController_device;
    // The game object associated with the currently active controller (if any):
    private GameObject active_controller = null;

    [SerializeField] private string LoadGesturesFile;
    private GestureRecognition gr;

    GCHandle me;
    Camera mainCamera;

    // GestureHandler gestureHandler;

    // Start is called before the first frame update
    void Start()
    {
        gr = new GestureRecognition();
        me = GCHandle.Alloc(this);

        leftController_device = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        rightController_device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        // Load the set of gestures.
        if (LoadGesturesFile == null) {
            LoadGesturesFile = "Sample_OneHanded_Gestures.dat";
        }

        // Find the location for the gesture database (.dat) file
#if UNITY_EDITOR
        // When running the scene inside the Unity editor,
        // we can just load the file from the Assets/ folder:
            string GesturesFilePath = "Assets/Gestures";
#endif

        gr.loadFromFile(GesturesFilePath + "/" + LoadGesturesFile);
      

        mainCamera = Camera.main;
        // gestureHandler = GameObject.FindGameObjectWithTag("LogicManager").GetComponent<GestureHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        leftController_device.TryGetFeatureValue(CommonUsages.trigger, out float trigger_left);
        rightController_device.TryGetFeatureValue(CommonUsages.trigger, out float trigger_right);

        if (active_controller == null) {
            // If the user presses either controller's trigger, we start a new gesture.
            if (trigger_right > 0.9 && !(trigger_left > 0.5)) {
                // Right controller trigger pressed.
                active_controller = rightController.gameObject;
            }
            else if (trigger_left > 0.9 && !(trigger_right > 0.5)) {
                // Left controller trigger pressed.
                active_controller = leftController.gameObject;
            }
            else {
                // If we arrive here, the user is pressing neither controller's trigger:
                // nothing to do.
                return;
            }
            // If we arrive here: either trigger was pressed, so we start the gesture.
            Vector3 hmd_p = mainCamera.transform.position;
            Quaternion hmd_q = mainCamera.transform.rotation;
            gr.startStroke(hmd_p, hmd_q, -1);
        }

        if (trigger_left > 0.85 || trigger_right > 0.85 && !(trigger_left > 0.5 && trigger_right > 0.5)) {
            // The user is still dragging with the controller: continue the gesture.
            Vector3 p = active_controller.transform.position;
            Quaternion q = active_controller.transform.rotation;
            gr.contdStrokeQ(p, q);
            return;
        }
        // else: if we arrive here, the user let go of the trigger, ending a gesture.
        active_controller = null;

        double similarity = 0; // This will receive a value of how similar the performed gesture was to previous recordings.
        Vector3 pos = Vector3.zero; // This will receive the position where the gesture was performed.
        double scale = 0; // This will receive the scale at which the gesture was performed.
        Vector3 dir0 = Vector3.zero; // This will receive the primary direction in which the gesture was performed (greatest expansion).
        Vector3 dir1 = Vector3.zero; // This will receive the secondary direction of the gesture.
        Vector3 dir2 = Vector3.zero; // This will receive the minor direction of the gesture (direction of smallest expansion).
        int gesture_id = gr.endStroke(ref similarity, ref pos, ref scale, ref dir0, ref dir1, ref dir2);

        switch(gesture_id) {
            case 0: // Stone Trap
                // TODO: Figure out which hand was used. 
                // gestureHandler.RockFall("right");
                break;
            case 1: // Waterbubble
                // gestureHandler.WaterBubble();
                break;
            case 2: // fire spell
                // gestureHandler.FireSpell();
                break;
        }
    }
}
