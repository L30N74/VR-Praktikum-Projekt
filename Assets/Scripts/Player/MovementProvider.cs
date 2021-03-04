using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;


[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerStats))]
public class MovementProvider : MonoBehaviour
{
    public XRNode inputSource;
    public LayerMask groundLayer;
    public float speed = 1;
    public float additionalHeight = 0.2f;
    public float gravity = 9.81f;

    private float fallSpeed;
    private XRRig rig;
    private Vector2 inputAxis;

    private InputDevice device;
    private CharacterController character;

    private PlayerStats playerStatsScript;

    // Start is called before the first frame update
    void Start()
    {
        rig = GetComponent<XRRig>();
        device = InputDevices.GetDeviceAtXRNode(inputSource);
        character = GetComponent<CharacterController>();
        playerStatsScript = GetComponent<PlayerStats>();
    }

    // Update is called once per frame
    void Update()
    {
        device.TryGetFeatureValue(CommonUsages.primary2DAxis, out inputAxis);
    }

    private void FixedUpdate()
    {
        CharacterControllerFollowHeadset();

        // Gravity
        if (CheckGrounded())
            fallSpeed = 0f;
        else
            fallSpeed += gravity * Time.fixedDeltaTime;

        character.Move(Vector3.down * fallSpeed * Time.fixedDeltaTime);


        if (playerStatsScript.isRestricted)
            return;

        Quaternion headYaw = Quaternion.Euler(0, rig.cameraGameObject.transform.eulerAngles.y, 0);
        Vector3 direction = headYaw * new Vector3(inputAxis.x, 0, inputAxis.y);
        character.Move(direction * speed * Time.fixedDeltaTime);
    }

    void CharacterControllerFollowHeadset()
    {
        character.height = rig.cameraInRigSpaceHeight + additionalHeight;
        Vector3 capsuleCenter = transform.InverseTransformPoint(rig.cameraGameObject.transform.position);
        character.center = new Vector3(capsuleCenter.x, character.height / 2 + character.skinWidth, capsuleCenter.z);
    }

    private bool CheckGrounded()
    {
        Vector3 rayStart = transform.TransformPoint(character.center);
        float rayLength = character.center.y + 0.05f;

        return Physics.SphereCast(rayStart, character.radius, Vector3.down, out RaycastHit hitInfo, groundLayer);
    }
}
