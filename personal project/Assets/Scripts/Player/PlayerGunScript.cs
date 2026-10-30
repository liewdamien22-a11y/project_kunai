using System.Collections;
using UnityEngine;

public class PlayerGunScript : MonoBehaviour
{
    [SerializeField] private Animator gunAnimator;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject muzzleFlashStandard;//id = 1
    [SerializeField] private GameObject muzzleFlashCharge;//id = 2
    [SerializeField] private Quaternion muzzleFlashStandardInitialRotation;
    [SerializeField] private Quaternion muzzleFlashChargeInitialRotation;
    [SerializeField] private float baseDamage = 1f;
    [SerializeField] private float chargeDamage = 3f;
    public enum gunState //dictates what the upcoming shot is
    {
        standard, charging, charged, overheated
    }
    public gunState currentGunState = gunState.standard;

    [Header("Shooting Parameters")]
    public float standardShotCooldown = 0.67f;
    public float maxRayDistance = 100f;

    private float nextFireTime = 0f;

    void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        muzzleFlashStandard.SetActive(false);
        muzzleFlashCharge.SetActive(false);

        muzzleFlashStandardInitialRotation = muzzleFlashStandard.transform.localRotation;
        muzzleFlashChargeInitialRotation = muzzleFlashCharge.transform.localRotation;
    }

    void Update()
    {
        HandleInput();
        
    }

    void HandleInput()//input polling
    {
        //standard fire or release charge (left click)
        if (Input.GetKeyDown(KeyCode.Mouse0) && Time.time >= nextFireTime)
        {
            if (currentGunState == gunState.standard)
            {
                ShootStandard();
                StartCoroutine(muzzleFlash(muzzleFlashStandard, 1));
                
            }
            else if (currentGunState == gunState.charged || currentGunState == gunState.charging)
            {
                FireChargeShot();
                StartCoroutine(muzzleFlash(muzzleFlashCharge, 2));
                
            }
        }

        //start charge sequence (right click)
        if (Input.GetKeyDown(KeyCode.Mouse1) && currentGunState == gunState.standard)
        {
            StartCharge();
            
        }
    }

    void ShootStandard()
    {
        
        nextFireTime = Time.time + standardShotCooldown;

        //reset state animation from frame 0 
        gunAnimator.Play("shootState", 0, 0f);
        

        PerformRaycast(damage: baseDamage);
    }

    void StartCharge()
    {
        
        currentGunState = gunState.charging;

        //transitions automatically to cylinderSpinState via exit time in animator
        gunAnimator.SetTrigger("charge")/*Play("hammerPullState", 0, 0f)*/;
        
    }

    void FireChargeShot()
    {
        
        currentGunState = gunState.standard;
        nextFireTime = Time.time + standardShotCooldown;

        //snaps directly into chargeShootState from wherever the cylinder spin is
        gunAnimator.Play("chargeShootState", 0, 0f);
       
        //gunAnimator.SetTrigger("fireCharge");
        PerformRaycast(damage: chargeDamage);
    }

    void PerformRaycast(float damage)
    {
        Transform cameraTransform = playerCamera.transform;
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * maxRayDistance, Color.blue, 0.5f);

        if (Physics.Raycast(ray, out hit, maxRayDistance))
        {
            if (hit.collider.CompareTag("enemy"))
            {
                enemyBase enemy_base = hit.collider.GetComponent<enemyBase>();
                if (enemy_base != null)
                {
                    enemy_base.TakeDamage(damage);
                    Debug.Log($"Hit enemy for {damage} damage!");
                }
            }
        }
    }
    IEnumerator muzzleFlash(GameObject fx, int fxID)//coroutine to handle muzzle flash fx
    {
        float randomRotation = Random.Range(67f, 360f);
        if(fxID == 1)//play standard shot fx
        {
            fx.transform.localRotation = muzzleFlashStandardInitialRotation * Quaternion.Euler(randomRotation, 0f, 0f);
        }
        else//play charged shot fx
        {
            fx.transform.localRotation = muzzleFlashChargeInitialRotation * Quaternion.Euler(randomRotation, 0f, 0f);
        }
        
        fx.SetActive(true);
        yield return new WaitForSeconds(0.067f);
        fx.SetActive(false);
    }
}