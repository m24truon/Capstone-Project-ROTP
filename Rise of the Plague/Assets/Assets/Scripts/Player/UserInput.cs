using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInput : MonoBehaviour
{
    public CharacterMovement characterMove { get; protected set; }
    public WeaponHandler weaponHandler { get; protected set; }

    [System.Serializable]
    public class InputSettings
    {
        public string verticalAxis = "Vertical";
        public string horizontalAxis = "Horizontal";
        public string jumpButton = "Jump";
        public string reloadButton = "Reload";
        public string aimButton = "Fire2";
        public string fireButton = "Fire1";
        public string dropWeaponButton = "DropWeapon";
        public string switchWeaponButton = "SwitchWeapon";
    }
    [SerializeField]
    InputSettings input;

    [System.Serializable]
    public class OtherSettings
    {
        public float lookSpeed = 5.0f;
        public float lookDistance = 10.0f;
        public bool requireInputForTurn = true;
        public LayerMask aimDetectionLayers;
    }
    [SerializeField]
    public OtherSettings other;

    public bool debugAim;
    public Transform spine;
    bool aiming;

    public Camera TPSCamera;

    // Use this for initialization
    void Start()
    {
        characterMove = GetComponent<CharacterMovement>();
        weaponHandler = GetComponent<WeaponHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        CharacterLogic();
        CameraLookLogic();
        WeaponLogic();

    }

    void LateUpdate()
    {
        if(weaponHandler)
        {
            if(weaponHandler.currentWeapon)
            {
                if (aiming)
                    PositionSpine();
            }
        }

    }

    //Handles character logic
    void CharacterLogic()
    {
        if (characterMove)
            return;

        characterMove.Animate(Input.GetAxis(input.verticalAxis), Input.GetAxis(input.horizontalAxis));

        if (Input.GetButtonDown(input.jumpButton))
        {
            characterMove.Jump();
        }

    }

    //Handles camera logic
    void CameraLookLogic()
    {
        if (!TPSCamera)
            return;

        if (other.requireInputForTurn)
        {
            if (Input.GetAxis(input.horizontalAxis) != 0 || Input.GetAxis(input.verticalAxis) != 0)
            {
                CharacterLook();
            }
        }
        else
        {
            CharacterLook();
        }

    }

    //Handles all weapon logic
    void WeaponLogic()
    {
        if (!weaponHandler)
            return;

        aiming = Input.GetButton(input.aimButton) || debugAim;

        if(weaponHandler.currentWeapon)
        {
            weaponHandler.Aim(aiming);

            other.requireInputForTurn = !aiming;

            weaponHandler.FingerOnTrigger(Input.GetButton(input.fireButton));

            if (Input.GetButtonDown(input.reloadButton))
                weaponHandler.Reload();

            if (Input.GetButtonDown(input.dropWeaponButton))
                weaponHandler.DropCurWeapon();

            if (Input.GetButtonDown(input.switchWeaponButton))
                weaponHandler.SwitchWeapons();

            if (!weaponHandler.currentWeapon)
                return;

            weaponHandler.currentWeapon.shootRay = new Ray(TPSCamera.transform.position, TPSCamera.transform.forward);
        }
    }

    //Positions the spine when aiming
    void PositionSpine()
    {
        if (!spine || !weaponHandler.currentWeapon || !TPSCamera)
            return;

        Transform mainCamT = TPSCamera.transform;
        Vector3 mainCamPos = mainCamT.position;
        Vector3 dir = mainCamT.forward;
        Ray ray = new Ray(mainCamPos, dir);

        
            spine.LookAt(ray.GetPoint(50));
        

        Vector3 eulerAngleOffset = weaponHandler.currentWeapon.userSettings.spineRotation;
        spine.Rotate(eulerAngleOffset);
    }

    //Make the character look at a forward point from the camera
    void CharacterLook()
    {
        Transform mainCamT = TPSCamera.transform;
        Transform pivotT = mainCamT.parent;
        Vector3 pivotPos = pivotT.position;
        Vector3 lookTarget = pivotPos + (pivotT.forward * other.lookDistance);
        Vector3 thisPos = transform.position;
        Vector3 lookDir = lookTarget - thisPos;
        Quaternion lookRot = Quaternion.LookRotation(lookDir);
        lookRot.x = 0;
        lookRot.z = 0;

        Quaternion newRotation = Quaternion.Lerp(transform.rotation, lookRot, Time.deltaTime * other.lookSpeed);
        transform.rotation = newRotation;
    }
}
