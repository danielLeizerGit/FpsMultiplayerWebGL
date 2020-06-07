using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class MouseLook : MonoBehaviourPunCallbacks
{
    public float mouseSensitivity = 100f; //should be slower, maybe one for x and one for y
    public GameObject player;
    public Transform playerBody;
    public Transform weapon;
    float xRotation = 0f;
    float mouseY;
    private Player playerScript;

   
    
  
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerScript = player.GetComponent<Player>();


    }

    // Update is called once per frame
    void Update() //should be late update?
    {
     
        if(Input.GetKeyDown(KeyCode.L))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        /*float*/ mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // is 90f maybe need to be lower

        //transform.localRotation = Quaternion.Euler(xRotation, mouseY, 0f);
        if (!playerScript.isDead)
        {
           
          //  transform.localRotation = Quaternion.AngleAxis(xRotation, Vector3.right);
            playerBody.Rotate(Vector3.up * mouseX); // move the weapon to the side as well

            if (Input.GetKey(KeyCode.Mouse1))
            {
                //  Debug.Log(weapon.eulerAngles.x);
                float x = weapon.eulerAngles.x - mouseY;
                //if (weapon.eulerAngles.x - mouseY > 45f && weapon.localRotation.eulerAngles.x - mouseY < 315f)
                if (x > 55f && x < 325f)
                {
                    mouseY = 0f;
                }

             
             //   transform.localRotation = Quaternion.AngleAxis(mouseY * 0.25f, Vector3.up); // half speed
                weapon.Rotate(Vector3.right * -mouseY*0.25f );// half speed
            }







        }


    }

    private void LateUpdate()
    {
        transform.localRotation = Quaternion.AngleAxis(xRotation, Vector3.right);
        if (Input.GetKey(KeyCode.Mouse1))
            transform.localRotation = Quaternion.AngleAxis(mouseY * 0.25f, Vector3.up);
    }
    public IEnumerator ShakeCamera(float shakeDuration, float shakeAmount = 0.2f, float decreaseFactor = 0.3f)
    {
        Vector3 originalRot = transform.eulerAngles;
        float currentShakeDuration = shakeDuration;
        while (currentShakeDuration > 0)
        {
            transform.eulerAngles += Random.insideUnitSphere * shakeAmount;
            currentShakeDuration -= Time.deltaTime * decreaseFactor;
            yield return null;
        }
        
        player.GetComponent<Player>().canShoot = true;
    }




}
