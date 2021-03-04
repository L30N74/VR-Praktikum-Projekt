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
    public XRNode inputSource;
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
    private InputDevice controller_device;
    private bool isMoving;

    private List<Vector3> positionList = new List<Vector3>();
    private readonly float newPositionThresholdDistance = 0.05f;


    // Start is called before the first frame update
    void Start()
    {
        controller_device = InputDevices.GetDeviceAtXRNode(inputSource);

        string[] gestureFiles = Directory.GetFiles(Application.persistentDataPath + "/Gestures/", "*.xml");

        foreach(string fileName in gestureFiles) 
        {
            trainingSet.Add(GestureIO.ReadGestureFromFile(fileName));
        }
    }

    // Update is called once per frame
    void Update()
    {
        InputHelpers.IsPressed(controller_device, inputButton, out bool isPressed, inputThreshold);

        //Start movement
        if(!isMoving && isPressed) 
        {
            StartMovement();
        }
        //Update movement
        else if (isMoving && isPressed) {
            UpdateMovement();
        }
        //End movement
        else if(isMoving && !isPressed) 
        {
            EndMovement();
        }
    }

    private void StartMovement()
    {
        debugText.text = "Started a gesture";
        isMoving = true;
        positionList.Clear();
        positionList.Add(movementSource.position);

        if(showDebugCubes)
            Destroy(Instantiate(debugInstantiatePrefab, movementSource.position, Quaternion.identity), 5f);
    }

    private void UpdateMovement()
    {
        debugText.text = "Performing a gesture";
        Vector3 lastPosition = positionList[positionList.Count - 1];

        if (showDebugCubes)
            Destroy(Instantiate(debugInstantiatePrefab, movementSource.position, Quaternion.identity), 5f);

        if (Vector3.Distance(movementSource.position, lastPosition) > newPositionThresholdDistance)
            positionList.Add(movementSource.position);
    }

    private void EndMovement()
    {
        debugText.text += "\nEnded a gesture";
        isMoving = false;

        //Create Gesture from position list
        Point[] pointArray = new Point[positionList.Count];

        for(int i = 0; i < positionList.Count; i++) 
        {
            Vector2 screenPoint = Camera.main.WorldToScreenPoint(positionList[i]);
            pointArray[i] = new Point(screenPoint.x, screenPoint.y, 0);
        }

        Gesture newGesture = new Gesture(pointArray);

        if(creationMode) 
        {
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
    }
}
