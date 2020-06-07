using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseAim : MonoBehaviour
{
    public float mouseSensitivity = 100f; //should be slower, maybe one for x and one for y
    public GameObject player;
    public Transform hands;
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
    void Update()
    {
        if (!playerScript.isDead)
        {

        }
    }
}
