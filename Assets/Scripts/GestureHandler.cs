using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using TMPro;

public class GestureHandler : MonoBehaviour
{
    public GameObject boulderPrefab;
    public float boulderYOffset;
    public float maxBoulderDistance;
    public float boulderMovementThreshold = 0.1f;
    public float boulderMovementForce = 100;
    private bool boulderControll = false;
    private GameObject boulder = null;
    private Vector3 controllOrigin;


    public int waterBubble_effectDuration;
    public int waterBubble_effectValue;
    public float healEffectInterval;

    private IEnumerator waterBubbleRoutine;
    private PlayerStats playerStatsScript;

    // For fire and ice spell
    public GameObject fireSpellPrefab;
    public GameObject iceSpellPrefab;
    public float throwForce;
    public Transform spawnPoint;

    private Camera mainCamera;
    private InputDevice usedcontroller_device;
    private InputDevice leftController_device;
    private InputDevice rightController_device;
    public Transform left_controller;
    public Transform right_controller;
    private Transform usedController_Transform;

    public TextMeshProUGUI text;
	
	private Rigidbody boulderRigidbody;

    public void Start()
    {
        mainCamera = Camera.main;
        playerStatsScript = GameObject.Find("Player").GetComponent<PlayerStats>();

        leftController_device = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        rightController_device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        if (!left_controller)
            left_controller = GameObject.Find("RightHand Controller").transform;
        if (!right_controller)
            right_controller = GameObject.Find("LeftHand Controller").transform;
    }

    public void Update()
    {
        if (boulder != null) {
            if (!boulderControll) {
                boulderControll = true;
                controllOrigin = usedController_Transform.position;
                text.text = "Now controlling.";
				boulderRigidbody = boulder.GetComponent<Rigidbody>();
            }
            else {
                //Let the boulder follow the user's hand-movement
                usedcontroller_device.TryGetFeatureValue(CommonUsages.grip, out float grip);
                if (grip > 0.6f) {
                    HandleBoulderMovement();
                }
                else { // Let the boulder fall upon releasing the grip-button 
                    boulderRigidbody.isKinematic = false;
                    boulderControll = false;
                    boulder = null;
					boulderRigidbody = null;
                }
            }
        }
    }

    public void ExecuteSpellByName(string spellName)
    {
        switch(spellName) 
        {
            case "fire":
                text.text = "Used fire spell";
                break;
            case "ice":
                text.text = "Used ice spell";
                break;
        }
    }


    private void HandleBoulderMovement()
    {
        // Follow head-Rotation
        float angle = 0f;
        boulder.transform.Rotate(0, angle, 0, Space.Self);

        // Get current hand position
        Vector3 newHandPosition = usedController_Transform.position;

        Vector3 vector = controllOrigin - newHandPosition;

		// Calculate differences between the hand's old position and the updated one
        float xDifference = newHandPosition.x - controllOrigin.x;
        float yDifference = newHandPosition.y - controllOrigin.y;
        float zDifference = newHandPosition.z - controllOrigin.z;
        text.text = xDifference.ToString("#.000") + "\n" + yDifference.ToString("#.000") + "\n" + zDifference.ToString("#.000");
	
		Vector3 boulderMoveDirection = Vector3.zero;


		Vector3 boulderPos = boulder.transform.position;
		// Check if the differences in position are big enough to justify movement		
		boulderMoveDirection.x = Mathf.Abs(xDifference) > boulderMovementThreshold ? boulderPos.x + xDifference : 0f;
		boulderMoveDirection.y = Mathf.Abs(yDifference) > boulderMovementThreshold ? boulderPos.y + yDifference : 0f;
		boulderMoveDirection.z = Mathf.Abs(zDifference) > boulderMovementThreshold ? boulderPos.z + zDifference : 0f;
        
		
		// Move the boulder
		if(boulderMoveDirection != Vector3.zero)
			boulder.transform.Translate(boulderMoveDirection * boulderMovementForce * Time.deltaTime, Space.Self);
		    //boulderRigidbody.MovePosition(boulderMoveDirection * boulderMovementForce * Time.fixedDeltaTime);
		
		// For testing purposes: Display the vector on the world-space text
		text.text += "\n-----------\n" + boulderMoveDirection.ToString();
    }


    /// <summary>
    /// Instantiate a giant boulder at a target location
    /// Target location is either directly where the player is looking at (plus some y-offset) or
    /// an arbitrary point in the distance in the direction the player is looking (plus some y-offset)
    ///
    /// The boulder is controlled by the player's hand movement in update
    /// </summary>
    /// <param name="initializedHand">String representing hand with which to controll the boulder</param>
    public void RockFall(string handUsed) {
        if (boulder) return;

        switch (handUsed) {
            case "left":
                usedcontroller_device = leftController_device;
                usedController_Transform = left_controller;
                break;
            case "right":
            default:
                usedcontroller_device = rightController_device;
                usedController_Transform = right_controller;
                break;
        }


        // See what the player is looking at
        if(Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out RaycastHit hit, maxBoulderDistance)) {
            Vector3 spawnPosition = new Vector3(hit.point.x, hit.point.y + boulderYOffset, hit.point.z);

            // Instantiate the rock
            boulder = Instantiate(boulderPrefab, spawnPosition, mainCamera.transform.rotation);
        }
        else {
            // Raycast didnt hit anything.
            // The player is either not looking at something or maxdistance is reached
            // Spawn boulder at max distance
            Vector3 spawnPosition = mainCamera.transform.position + mainCamera.transform.forward * maxBoulderDistance;
            spawnPosition.y += boulderYOffset;

            boulder = Instantiate(boulderPrefab, spawnPosition, Quaternion.identity);
        }
    }

    /// <summary>
    /// Creates a healing-effect for the player that, 
    /// for a set time, periodically heals him
    /// </summary>
    public void WaterBubble() {
        //Stop the effect in case it's already active so it doesn't stack
        if (waterBubbleRoutine != null) {
            StopCoroutine(waterBubbleRoutine);
        }

        waterBubbleRoutine = HealBubbleRoutine();
        StartCoroutine(waterBubbleRoutine);
    }

    /// <summary>
    /// The Coroutine that controlls healing the player
    /// </summary>
    /// <returns>Time to wait for the next healing-effect to take place</returns>
    private IEnumerator HealBubbleRoutine()
    {
        float startTime = Time.time;

        while(true) {
            playerStatsScript.AlterHealth(waterBubble_effectValue, DamageType.Healing);

            // Break after the effect's duration has elapsed
            if (Time.time - startTime > waterBubble_effectDuration) {
                waterBubbleRoutine = null;
                break;
            }

            yield return new WaitForSeconds(healEffectInterval);
        }
    }

    public void FireSpell(string handUsed) {

        switch (handUsed) {
            case "left":
                usedcontroller_device = leftController_device;
                usedController_Transform = left_controller;
                break;
            case "right":
            default:
                usedcontroller_device = rightController_device;
                usedController_Transform = right_controller;
                break;
        }

        GameObject spellGo = Instantiate(fireSpellPrefab, spawnPoint.position, spawnPoint.rotation);
        spellGo.GetComponent<Rigidbody>().AddForce(spawnPoint.forward * throwForce, ForceMode.Impulse);
    }

    public void IceSpell(string handUsed) {

        switch (handUsed) {
            case "left":
                usedcontroller_device = leftController_device;
                usedController_Transform = left_controller;
                break;
            case "right":
            default:
                usedcontroller_device = rightController_device;
                usedController_Transform = right_controller;
                break;
        }

        GameObject spellGo = Instantiate(iceSpellPrefab, spawnPoint.position, spawnPoint.rotation);
        spellGo.GetComponent<Rigidbody>().AddForce(spawnPoint.forward * throwForce, ForceMode.Impulse);
    }
}
