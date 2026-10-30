using Unity.VisualScripting;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class kunaiProjectileScript : MonoBehaviour
{
    [SerializeField]private Rigidbody rb;
    public bool isStuck = false;
    public float speed = 67f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Start()
    {
        fly();
    }
    void fly()
    {
        rb.useGravity = false;
        rb.linearDamping = 0f;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.linearVelocity = transform.forward * speed;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(!isStuck)
        {
            
            int layer = collision.gameObject.layer;
            string layerName = LayerMask.LayerToName(layer);
            if(layerName == "floor" || layerName == "wall" || layerName == "environment" )
            {
                //Debug.Log("stuckedd");
                stick(collision);
                
            }
        }
    }
    private void stick(Collision collision)
    {
        //Debug.Log($"stick function called inside kunai projectile script");
        isStuck = true;

        //freeze physical motion
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;


        //store world scale
        //Vector3 worldScale = transform.lossyScale;

        //parent to hit surface so it moves with moving platforms/elevators
        transform.SetParent(collision.transform.root, true);

        /*
        //get parent scale
        Vector3 parentScale = transform.parent.lossyScale;
        //force uniform scale relative to parent to prevent warping
        transform.localScale = new Vector3(
            worldScale.x / parentScale.x,
            worldScale.y / parentScale.y,
            worldScale.z / parentScale.z
            
            );
        */
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;//reduce overhead once kunai is stuck in environment
    }
}
