using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]

public class Weapon : MonoBehaviour
{
    Collider col;
    Rigidbody rigidBody;
    Animator animator;

    public enum WeaponType
    {
        Pistol,Rifle
    }
    public WeaponType weaponType;

    [System.Serializable]
    public class UserSettings
    {
        public Transform leftHandIKTarget;
        public Vector3 spineRotation;
    }
    [SerializeField]
    public UserSettings userSettings;

    [System.Serializable]
    public class WeaponSettings
    {
        [Header("-Bullet Options-")]
        public Transform bulletSpawn;
        public float damage = 5.0f;
        public float bulletSpread = 5.0f;
        public float fireRate = 0.2f;
        public LayerMask bulletLayers;
        public float range = 200.0f;

        [Header("-Effects-")]
        public GameObject muzzleFlash;
        public GameObject decal;
        public GameObject shell;
        public GameObject clip;

        [Header("-Other-")]
        public float reloadDuration = 2.0f;
        public Transform shellEjectSpot;
        public float shellEjectSpeed = 7.5f;
        public GameObject clipGO;

        [Header("-Positioning-")]
        public Vector3 equipPosition;
        public Vector3 equipRotation;
        public Vector3 unequipPosition;
        public Vector3 unequipRotation;

        [Header("-Animation-")]
        public bool useAnimation;
        public int fireAnimationLayer = 0;
        public string fireAnimationName = "Fire";
    }
    [SerializeField]
    public WeaponSettings weaponSettings;

    [System.Serializable]
    public class Ammunition
    {
        public int carryingAmmo;
        public int clipAmmo;
        public int maxClipAmmo;
    }

    [SerializeField]
    public Ammunition ammo;

    WeaponHandler owner;
    bool equipped;
    bool pullingTrigger;
    bool resetttingCartridge;

    // Use this for initialization
    void Start()
    {
        col = GetComponent<Collider>();
        rigidBody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (owner)
        {
            DisableEnableComponents(false);

            if(equipped)
            {
                if(owner.userSettings.rightHand)
                {
                    Equip();

                    if(pullingTrigger)
                    {
                        Fire();
                    }
                }
            }
            else
            {
                Unequip(weaponType);
            }
        }
        else
        {
            DisableEnableComponents(true);

            transform.SetParent(null);
        }
    }

    //This fires the weapon
    void Fire()
    {
        if (ammo.clipAmmo <= 0 || resetttingCartridge || !weaponSettings.bulletSpawn)
            return;

        RaycastHit hit;
        Transform bSpawn = weaponSettings.bulletSpawn;
        Vector3 bSpawnPoint = bSpawn.position;
        Vector3 dir = bSpawn.forward;

        dir += (Vector3)Random.insideUnitCircle * weaponSettings.bulletSpread;

        if(Physics.Raycast(bSpawnPoint, dir, out hit, weaponSettings.range, weaponSettings.bulletLayers))
        {
            #region decal
            if(hit.collider.gameObject.isStatic)
            {
                if(weaponSettings.decal)
                {
                    Vector3 hitPoint = hit.point;
                    Quaternion lookRotation = Quaternion.LookRotation(hit.normal);
                    GameObject decal = Instantiate(weaponSettings.decal, hitPoint, lookRotation) as GameObject;
                    Transform decalT = decal.transform;
                    Transform hitT = hit.transform;
                    decalT.SetParent(hitT);
                    Destroy(decal, Random.Range(30.0f, 45.0f));
                }
            }
            #endregion
        }

        #region muzzle flash
        if(weaponSettings.muzzleFlash)
        {
            Vector3 bulletSpawnPos = weaponSettings.bulletSpawn.position;
            GameObject muzzleFlash = Instantiate(weaponSettings.muzzleFlash, bulletSpawnPos, Quaternion.identity) as GameObject;
            Transform muzzleT = muzzleFlash.transform;
            muzzleT.SetParent(weaponSettings.bulletSpawn);
            Destroy(muzzleFlash, 1.0f);
        }
        #endregion

        #region shell
        if(weaponSettings.shell)
        {
            if(weaponSettings.shellEjectSpot)
            {
                Vector3 shellEjectPos = weaponSettings.shellEjectSpot.position;
                Quaternion shellEjectRot = weaponSettings.shellEjectSpot.rotation;
                GameObject shell = Instantiate(weaponSettings.shell, shellEjectPos, shellEjectRot) as GameObject;

                if(shell.GetComponent<Rigidbody>())
                {
                    Rigidbody rigidB = shell.GetComponent<Rigidbody>();
                    rigidB.AddForce(weaponSettings.shellEjectSpot.forward * weaponSettings.shellEjectSpeed, ForceMode.Impulse);
                }
                Destroy(shell, Random.Range(30.0f, 45.0f));
            }
        }
        #endregion

        if (weaponSettings.useAnimation)
            animator.Play(weaponSettings.fireAnimationName, weaponSettings.fireAnimationLayer);

        ammo.clipAmmo--;
        resetttingCartridge = true;
        StartCoroutine(LoadNextBullet());
    }

    //Loads the next bullet into chamber
    IEnumerator LoadNextBullet()
    {
        yield return new WaitForSeconds(weaponSettings.fireRate);
        resetttingCartridge = false;
    }

    //Disables or enables collider and rigidbody
    void DisableEnableComponents(bool enabled)
    {
        if(!enabled)
        {
            rigidBody.isKinematic = true;
            col.enabled = false;
        }
        else
        {
            rigidBody.isKinematic = false;
            col.enabled = true;
        }
    }

    //Equips this weapon to the hand
    void Equip()
    {
        if (!owner)
            return;
        else if (!owner.userSettings.rightHand)
            return;

        transform.SetParent(owner.userSettings.rightHand);
        transform.localPosition = weaponSettings.equipPosition;
        Quaternion equipRot = Quaternion.Euler(weaponSettings.equipRotation);
        transform.localRotation = equipRot;
    }

    //Unequips the weapon and places it to the desired location
    void Unequip(WeaponType wpType)
    {
        if (!owner)
            return;

        switch(wpType)
        {
            case WeaponType.Pistol:
                transform.SetParent(owner.userSettings.pistolUnequipSpot);
                break;
            case WeaponType.Rifle:
                transform.SetParent(owner.userSettings.rifleUnequipSpot);
                break;
        }
        transform.localPosition = weaponSettings.unequipPosition;
        Quaternion unEquipRot = Quaternion.Euler(weaponSettings.unequipRotation);
        transform.localRotation = unEquipRot;
    }

    //Loads the clip and calculates the ammo
    public void LoadClip()
    {
        int ammoNeeded = ammo.maxClipAmmo - ammo.clipAmmo;

        if(ammoNeeded >= ammo.carryingAmmo)
        {
            ammo.clipAmmo = ammo.carryingAmmo;
            ammo.carryingAmmo = 0;
        }
        else
        {
            ammo.carryingAmmo -= ammoNeeded;
            ammo.clipAmmo = ammo.maxClipAmmo;
        }
    }

    //Sets the weapons equip state
    public void SetEquipped(bool equip)
    {
        equipped = equip;
    }

    //Pulls the trigger
    public void PullTrigger(bool isPulling)
    {
        pullingTrigger = isPulling;
    }

    //Sets the owner of this weapon
    public void SetOwner(WeaponHandler wp)
    {
        owner = wp;
    }

}
