using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishMovement : MonoBehaviour
{
    public Fish fish;
    public int newScore;

    private SwipeDetect swipeManager;
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float swimInterval = 3f;
    public float rotationSpeed = 5f;
    public float swimDelay = 1f;

    private Rigidbody rb;
    public float timer;

    public Quaternion targetRotation;

    public float raycastDistance = 5f;
    public float raycastAngleOffset = 45f;
    public  bool[] rayHits = new bool[3];

    [Header("Field of View")]
    public GameObject bobber;
    public float FOV = 90;
    public bool playerInFov = false;
    public GameObject alertIcon; 

    public float timeInFOV;
    public float distance;

    [Header("Catching")]
    public float catchPeriod;
    public float catchTimer;
    public bool isCatching = false;


    void Start()
    {
        SetMaterial();
        rb = GetComponent<Rigidbody>();
        swipeManager = SwipeDetect.instance;
        SpawnMovement();
    }

    void Update()
    {
        if(bobber != null)
        {
            CheckFOV();

        }else
        {
            bobber = GameObject.Find("Bobber");
        }

        if(catchTimer < 0)
        {
            catchTimer = -1;
        }else{
            catchTimer -= Time.deltaTime;
        }

        if( timeInFOV >= 1f)
        {
            FacePlayer();
        }
        else{
            timer -= Time.deltaTime;
            
            if(timer <= 0f)
            {
                
                RaycastTurnAroundCheck();
                //RandomMovement();
            }
        }
        
        if(timeInFOV >= 5f)
        {
            if(isCatching == false)
            {
                catchTimer = catchPeriod;
                alertIcon.GetComponent<Renderer>().material.color = Color.yellow;
                isCatching = true;
            }
        }else{
            alertIcon.GetComponent<Renderer>().material.color = Color.red;
            isCatching = false;
        }

        if(catchTimer > 0)
        {
            if(!CatchingManager.instance.fishes.Contains(gameObject))
            {
                CatchingManager.instance.fishes.Add(gameObject);
            }
            //CatchFish();
        }else{
            if(CatchingManager.instance.fishes.Contains(gameObject))
            {
                TurnAround(180);
                isCatching = false;
                CatchingManager.instance.fishes.Remove(gameObject);
            }
        }

    }

    void SetMaterial()
    {
        SkinnedMeshRenderer renderer = this.gameObject.transform.Find("Fish Model Renderer").GetComponent<SkinnedMeshRenderer>();

        //https://discussions.unity.com/t/change-the-color-of-a-material-for-only-one-object/65028/5

        Material[] mats = renderer.materials;

        for (int i = 0; i < fish.colors.Length; i++)
        {
            mats[i].color = fish.colors[i];
        }

        //renderer.material.color = Color.cyan;
        renderer.materials = mats;
        //renderer.material = Instantiate(Resources.Load("Material") as Material);
        //this.gameObject.GetComponent().material = Instantiate(Resources.Load(“Material”) as Material);
        //Debug.Log(this.gameObject.transform.Find("Fish Model Renderer"));
    }

    void SpawnMovement()
    {
        timer = Random.Range(swimInterval * 0.5f, swimInterval * 1.5f);
        float randomRotationY = Random.Range(0f, 360f);
        targetRotation = Quaternion.Euler(0f, randomRotationY, 0f);
        rb.velocity = transform.forward.normalized * moveSpeed;
    }

    void RandomMovement()
    {
        //randomizes a new direction
        float randomRotationY = Random.Range(0f, 360f);
        targetRotation = Quaternion.Euler(0f, randomRotationY, 0f);

        //rotates towards the random direction
        StartCoroutine(MovementCoroutine());
    }

    void TurnAround(float newRotation)
    {
        //passes in new direction
       
        //Debug.Log(transform.eulerAngles.y + "+" +  newRotation);
        targetRotation = Quaternion.Euler(0f, transform.eulerAngles.y + newRotation, 0f);

        //rotates towards the new direction
        StartCoroutine(MovementCoroutine());
    }

    IEnumerator MovementCoroutine()
    {
        timer = Random.Range(swimInterval * 0.5f, swimInterval * 1.5f);
        
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            yield return null;
        }

        //waits a second before moving
        yield return new WaitForSeconds(swimDelay);
        

        //moves forwards in the random direction
        rb.velocity = transform.forward.normalized * moveSpeed;  
    }

    //https://discussions.unity.com/t/how-to-use-quaternion-slerp-with-transform-lookat/184419
    void FacePlayer()
    {
        Vector3 targetPostition = new Vector3( bobber.transform.position.x, this.transform.position.y, bobber.transform.position.z ) ;
        Quaternion lookOnLook = Quaternion.LookRotation(targetPostition - transform.position);

        transform.rotation = Quaternion.Slerp(transform.rotation, lookOnLook, rotationSpeed * Time.deltaTime);
    }

    /*
    IEnumerator FacePlayer()
    {
        //Vector3 targetPostition = new Vector3( target.position.x, this.transform.position.y, target.position.z ) ;

        //this.transform.LookAt(targetPostition);
        
        
        
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }*/
    

    void RaycastTurnAroundCheck()
    {
        bool hitSomething = false;
       
        Vector3[] rayDirections = new Vector3[3];
        rayDirections[0] = transform.forward;
        rayDirections[1] = Quaternion.Euler(0f, -raycastAngleOffset, 0f) * transform.forward;
        rayDirections[2] = Quaternion.Euler(0f, raycastAngleOffset, 0f) * transform.forward;

        for (int i = 0; i < 3; i++)
        {
            Debug.DrawRay(transform.position, rayDirections[i] * raycastDistance, Color.green, 2);

            RaycastHit hit;
            rayHits[i] = false;
            if (Physics.Raycast(transform.position, rayDirections[i], out hit, raycastDistance))
            {
                if(hit.transform.tag == "Obstacle") 
                {
                    hitSomething = true;
                    rayHits[i] = true;
                }

                //Debug.Log("Raycast " + (i + 1) + " hit: " + hit.collider.name);
                //break;
            }
        }

        if(rayHits[1] && rayHits[2] || rayHits[0])
        {
            TurnAround(180f);
            Debug.Log("turn around");
            //both hit
        }
        else if(rayHits[1] && !rayHits[2])
        {
            TurnAround(90f);
            Debug.Log("turn right");
            //only left raycast hit
        }
        else if(rayHits[2] && !rayHits[1])
        {
            TurnAround(-90f);
            Debug.Log("turn left");
            //only right raycast hit
        }
        else if (!hitSomething)
        {
            RandomMovement();
        }
    }

    //https://discussions.unity.com/t/check-if-player-is-in-enemys-fov/182973
    void CheckFOV()
    {
        Vector3 targetPostition = new Vector3( bobber.transform.position.x, this.transform.position.y, bobber.transform.position.z ) ;
        Vector3 targetDir = targetPostition - transform.position; 

        distance = Vector3.Distance (targetPostition, transform.position);
        float angleToPlayer = (Vector3.Angle(targetDir, transform.forward)); 

        if(angleToPlayer >= -FOV/2 && angleToPlayer <= FOV/2 && distance < 8)
        {
            playerInFov = true;
            alertIcon.SetActive(true);
            timeInFOV += Time.deltaTime;
            if(distance > 3f && timeInFOV > 1)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPostition, 2 * Time.deltaTime);

            }
            if(distance < 2f && timeInFOV > 1)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPostition, -1 * Time.deltaTime);
            }
            //Debug.Log("Player in sight!");
        }
        else{
            playerInFov = false;
            alertIcon.SetActive(false);
            timeInFOV = 0;
        }
    }

    void CatchFish()
    {
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        rb.AddForce(transform.up * 200);
    }
    
}

