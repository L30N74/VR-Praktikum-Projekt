using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using PDollarGestureRecognizer;
using TMPro;
using System.IO;
using UnityEngine.Events;


public class GestureRecognizer : MonoBehaviour
{
    public InputHelpers.Button inputButton;
    public float inputThreshold = 0.9f;

    public Transform movementSource;    // The crystal

    public GameObject debugInstantiatePrefab;

    public TextMeshProUGUI debugText;

    public bool creationMode = true;
    public bool showDebugCubes = false;
    public string newGestureName;

    
    [System.Serializable]
    public class UnityStringEvent : UnityEvent<string> { }
    [Space(10f)]
    public float recognitionThreshold = 0.9f;
    public UnityStringEvent onRecognized;

    private List<Gesture> trainingSet = new List<Gesture>();
    private InputDevice[] controller_devices = new InputDevice[2];
    private bool isMoving;

    private List<List<Vector3>> positionList = new List<List<Vector3>>();
    private readonly float newPositionThresholdDistance = 0.05f;

    //Time offset for multi-stroke gestures
    public float maxStrokeDrawTimeOffset = 1.5f;
    private float timeSinceLastGesture = 0f;

    private int strokeIndex = -1;
    private bool gestureRecorded = false; 


    // Start is called before the first frame update
    void Start()
    {
        controller_devices[0] = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        controller_devices[1] = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        string[] gestureFiles = Directory.GetFiles(Application.persistentDataPath + "/Gestures/", "*.xml");

        foreach(string fileName in gestureFiles) 
        {
            trainingSet.Add(GestureIO.ReadGestureFromFile(fileName));
        }
    }

    // Update is called once per frame
    void Update()
    {
        InputHelpers.IsPressed(controller_devices[0], inputButton, out bool isPressed_left, inputThreshold);
        InputHelpers.IsPressed(controller_devices[1], inputButton, out bool isPressed_right, inputThreshold);

        bool isPressed = isPressed_left || isPressed_right;

        //Start movement
        if (!isMoving && isPressed) {
            StartMovement();
        }
        //Update movement
        else if (isMoving && isPressed) {
            UpdateMovement();
        }
        //End movement
        else if (isMoving && !isPressed) {
            EndMovement();
        }
        else if (!isMoving && !isPressed && !gestureRecorded) {
            timeSinceLastGesture += Time.deltaTime;

            //Determine if gesture is really over of if user can still add new strokes
            if (timeSinceLastGesture > maxStrokeDrawTimeOffset) {
                DetermineGesture();
            }
        }
    }

    private void StartMovement()
    {
        debugText.text = "Started a gesture";

        timeSinceLastGesture = 0f;
        positionList.Add(new List<Vector3>());
        strokeIndex++;
        positionList[strokeIndex].Add(movementSource.position);

        if(showDebugCubes)
            Destroy(Instantiate(debugInstantiatePrefab, movementSource.position, Quaternion.identity), 5f);

        isMoving = true;
        gestureRecorded = false;
    }

    private void UpdateMovement()
    {
        debugText.text = "Performing a gesture\nStroke: " + strokeIndex;
        Vector3 lastPosition = positionList[strokeIndex][positionList[strokeIndex].Count - 1];

        if (showDebugCubes)
            Destroy(Instantiate(debugInstantiatePrefab, movementSource.position, Quaternion.identity), 5f);

        if (Vector3.Distance(movementSource.position, lastPosition) > newPositionThresholdDistance)
            positionList[strokeIndex].Add(movementSource.position);
    }

    private void EndMovement()
    {
        debugText.text += "\nEnded a gesture";
        isMoving = false;
    }

    private void DetermineGesture()
    {
        //Create Gesture from position list
        int pointSum = 0;
        foreach (List<Vector3> list in positionList)
            pointSum += list.Count;

        Point[] pointArray = new Point[pointSum];

        int counter = 0;
        for (int i = 0; i < positionList.Count; i++) {
            for (int j = 0; j < positionList[i].Count; j++) {
                Vector2 screenPoint = Camera.main.WorldToScreenPoint(positionList[i][j]);
                pointArray[counter] = new Point(screenPoint.x, screenPoint.y, i);
                counter++;
            }
        }

        Gesture newGesture = new Gesture(pointArray);

        if (creationMode) {
            newGesture.Name = newGestureName;
            trainingSet.Add(newGesture);

            string fileName = Application.persistentDataPath + "/Gestures/" + newGestureName + ".xml";
            GestureIO.WriteGesture(pointArray, newGestureName, fileName);
            debugText.text += "\nNew Gesture recorded: " + newGestureName;
        }
        else //Recognize gesture
        {
            Result result = PointCloudRecognizer.Classify(newGesture, trainingSet.ToArray());
            if (debugText)
                debugText.text += "\nGesture Name: " + result.GestureClass + "\nPrecision Score: " + result.Score;

            if (result.Score > recognitionThreshold)
                onRecognized.Invoke(result.GestureClass);
        }

        gestureRecorded = true;
        strokeIndex = -1;

        positionList.Clear();
    }
}
