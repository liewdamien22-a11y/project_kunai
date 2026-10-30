using System.Collections;
using UnityEngine;

public class PlayerKunaiScript : MonoBehaviour // playerside kunai script
{
    public enum kunaiState { throwing, thrown, stored }; // state machine so both throwing and teleporting are mapped to the same key

    public kunaiState state = kunaiState.stored;

    [SerializeField] private GameObject kunaiProjectilePrefab;

    [SerializeField] private Transform projectileOrigin;

    public KeyCode throwKunaiKey = KeyCode.F; // first press throws kunai, second press teleports to kunai

    [SerializeField] private PlayerMovement PlayerMovement;

    public GameObject kunaiTransform; // for teleporting


    [Header("Animation Stuff")]
    [SerializeField] private MeshRenderer kunaiHandRenderer; // Hand-held visual kunai mesh
    [SerializeField] private Animator armsAnimator;
    public float animationWait = 0.12f;
    public float teleportdelay = 0.12f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {


        PlayerMovement = GetComponentInParent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        keyPressed();
    }

    void keyPressed()
    {
        if (Input.GetKeyDown(throwKunaiKey))
        {
            if (state == kunaiState.stored)
            {
                //StartCoroutine(throwKunai());
                armsAnimator.Play("throwState");
            }
            else if (state == kunaiState.thrown)
            {
                //StartCoroutine(teleportToKunai());
                teleportToKunai();
            }
        }
    }


    private void throwKunai()//called at end of animation
    {
        //Debug.Log("throw function called");
        state = kunaiState.throwing; // Block double-inputs while animating\

        /*
        //play the Blender throw animation
        if (armsAnimator != null)
        {
            armsAnimator.SetTrigger("throw");
        }

        
        //wait until Frame 4 (the exact release moment in your 4-frame animation)
        yield return new WaitForSeconds(animationWait);
        */

        // hide kunai mesh
        if (kunaiHandRenderer != null)
        {
            kunaiHandRenderer.enabled = false;
        }

        //instantiate the physics projectile at the hand release position
        Vector3 vectorForInstantiate = projectileOrigin.position + projectileOrigin.forward * 0.2f;
        kunaiTransform = Instantiate(kunaiProjectilePrefab, vectorForInstantiate, projectileOrigin.rotation);
        state = kunaiState.thrown;
    }

    private void teleportToKunai()
    {
        if (kunaiTransform == null)
        {
            state = kunaiState.stored;
            if (kunaiHandRenderer != null)
            {
                kunaiHandRenderer.enabled = true;
            }
            //yield break;
        }

        Vector3 teleportPos = kunaiTransform.transform.position + Vector3.up * (PlayerMovement.controller.height / 2f);
        if (PlayerMovement != null && PlayerMovement.controller != null)
        {
            // disable player controller before teleporting to prevent any funny business
            PlayerMovement.controller.enabled = false;
            PlayerMovement.transform.position = teleportPos;
            PlayerMovement.controller.enabled = true;
            state = kunaiState.stored;
        }

        //yield return new WaitForSeconds(teleportdelay);

        if (kunaiHandRenderer != null)
        {
            kunaiHandRenderer.enabled = true;
        }

        Destroy(kunaiTransform);
        kunaiTransform = null;
        armsAnimator.Play("catchState");
    }
}