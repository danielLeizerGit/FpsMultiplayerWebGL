using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using UnityEngine.UI;


public class Player : MonoBehaviourPunCallbacks,IPunObservable
{
    //get component and camera
    private PhotonView pv;
    [SerializeField] GameObject body;//player body
    public GameObject head;
    public GameObject body2;
    public BoxCollider headBox;
    public BoxCollider bodyBox;
    [SerializeField] GameObject camera;//main camera on the body
    [SerializeField] GameObject weaponCamera;
    [SerializeField] GameObject crouchCamera;
    private CharacterController controller;
    [SerializeField] GameObject groundCheck;
    [SerializeField] GameObject weapon;
    [SerializeField] GameObject shootPos;
    public Animator anim;
    [SerializeField] GameObject canvas;
    private Vector3 firstPos;
    private Vector3 firstPosPlayer;
    [SerializeField] GameObject signOfHit;
    Vector3 camLastPos;
    private float shootDis = 75f;


    //local transform and rotation
    Quaternion weaponRotation;

    //var
    public float speed = 10f;
    public float gravity = -9.81f;
    public float groundDistance = 0.4f;
    public float jumpHeight = 3f;
    public int hp = 100;
    public int currentHp;
    public int bullets = 30; // bullets before reloed
    public int ammo;
    public LayerMask groundMask;
    bool isGrounded;
    public bool canShoot = true;
    private bool canJump = true;
    Vector3 velocity;
    private float conHeight;
    private int myTeam;
    private float originalSpeed;

    //anim
    bool aim=false;
    bool crouch = false;
    bool sprint = false;
    public bool isDead = false;
    bool isReload = false;
    bool isHit = false;

    //score
    public int blueScore;
    public int redScore;

    //UI
    [SerializeField] public Text hpText;
    [SerializeField] public Text ammoText;
    [SerializeField] public Text scoreText;
    [SerializeField] public Text myTeamText;
    [SerializeField] public Image aimCross;

    //Audio
    private AudioListener audioListener;
    private AudioSource soundSource;
    [SerializeField] AudioSource soundSourceFoot;
    [SerializeField] public AudioSource soundSource2d;
    [SerializeField] public List<AudioClip> clips; // 0 is shoot,1 is reload,2 empty ammo,3 hit,4 die,5 footstep

    public void Awake()
    {
        pv = this.gameObject.GetComponent<PhotonView>();
        anim = this.body.GetComponent<Animator>();
        controller = this.body.gameObject.GetComponent<CharacterController>();
        soundSource = weapon.GetComponent<AudioSource>();
        
     
    }
    // Start is called before the first frame update
    void Start()
    {
        if (!pv.IsMine)
        {
            camera.SetActive(false);
            canvas.SetActive(false);
            soundSource2d.enabled = false;
           // audioListener.enabled = false;
            return;
        }
           
        if (pv.IsMine)
        {
            pv.RPC("RPCChangeGet", RpcTarget.MasterClient); // get team
            //if(myTeam==1)
            //{
            //    //transform.position = GameManager.gm.spawnPoints[0].position;
            //    //firstPos= GameManager.gm.spawnPoints[0].position; ;
            //    //firstPosPlayer = GameManager.gm.spawnPoints[0].position; ;
            //    //myTeamText.text = "My Team: Red";
            //    //myTeamText.color = Color.red;
            //}
            //else
            //{
            //    //transform.position = GameManager.gm.spawnPoints[2].position;
            //    //firstPos = GameManager.gm.spawnPoints[2].position; ;
            //    //firstPosPlayer = GameManager.gm.spawnPoints[2].position; ;
            //    //myTeamText.text = "My Team: Blue";
            //    //myTeamText.color = Color.blue;
            //}
            currentHp = hp;
            ammo = bullets;
            conHeight = controller.height;
            originalSpeed = speed;
            head.layer = 9;
            body2.layer = 9;

            pv.RPC("RPCScore", RpcTarget.All, PhotonView.Find(1001).GetComponent<Player>().blueScore, PhotonView.Find(1001).GetComponent<Player>().redScore);
           // GameManager.gm.FindPlayers();
        }
          

    }

    // Update is called once per frame
    void Update()
    {
        if (!pv.IsMine)
            return;


        if (pv.IsMine)
        {
            //cheat
            if(Input.GetKeyDown(KeyCode.Delete)) //kill the other team
            {
                Debug.Log("hack kill");

                if(gameObject.CompareTag("RedTeam"))
                {
                    foreach(GameObject eneny in GameManager.gm.players)
                    {
                        if (eneny.CompareTag("BlueTeam"))
                            eneny.GetComponent<Player>().pv.RPC("RPCDead", RpcTarget.AllBuffered);
                    }
                }
                else
                {
                    foreach (GameObject eneny in GameManager.gm.players)
                    {
                        if (eneny.CompareTag("RedTeam"))
                            eneny.GetComponent<Player>().pv.RPC("RPCDead", RpcTarget.AllBuffered);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.End)) //heal myself
            {
                currentHp = hp;
                hpText.text = "Hp: " + currentHp;
            }
            //end cheat
            if (Input.GetKeyDown(KeyCode.T))
            {
                weaponCamera.transform.localPosition = new Vector3(-weaponCamera.transform.localPosition.x, weaponCamera.transform.localPosition.y, weaponCamera.transform.localPosition.z);
            }
            if (Input.GetKeyDown(KeyCode.Y))
            {
                foreach (Player ps in GameManager.gm.playersScript)
                   ps.pv.RPC("RPCScore2", RpcTarget.AllBuffered, 5, 5);
            }
            if (Input.GetKeyDown(KeyCode.U))
            {
                blueScore=0;
                redScore=0;
                foreach(Player ps in GameManager.gm.playersScript)
                ps.pv.RPC("RPCScore", RpcTarget.AllBufferedViaServer, blueScore, redScore);
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                Hit(this.gameObject, true);
                anim.SetTrigger("Hit");
            }
            //kill
            if(Input.GetKeyDown(KeyCode.Q))
            {
                pv.RPC("RPCDead", RpcTarget.AllBuffered);
                anim.SetTrigger("Dead");
            }

            if(!isDead)
            {
                Movement();
                //shoot
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    if (canShoot && ammo > 0)
                        Shoot();
                    else if (ammo == 0 && !soundSource2d.isPlaying)
                    {
                         soundSource2d.PlayOneShot(clips[2]);
                    }
                      
                }

                //aim
                if (Input.GetKeyDown(KeyCode.Mouse1))
                {
                    aim = true;
                    weaponRotation = weapon.transform.localRotation;
                    anim.SetBool("AimBool", aim);
                    weaponCamera.SetActive(true);
                    camera.SetActive(false);
                    aimCross.enabled = false;

                }

                if (Input.GetKeyUp(KeyCode.Mouse1))
                {
                    aim = false;
                    anim.SetBool("AimBool", aim);
                    weapon.transform.localRotation = weaponRotation;
                    weaponCamera.SetActive(false);
                    camera.SetActive(true);
                    aimCross.enabled = true;

                }
                //reload
                if (Input.GetKeyDown(KeyCode.R))
                {
                    anim.SetTrigger("Reload");
                    //Reload();

                }
                if(Input.GetKeyDown(KeyCode.LeftShift))
                {
                    if(!crouch)
                    {
                        speed = speed * 2;
                        sprint = true;
                    }
                }
                if(Input.GetKeyUp(KeyCode.LeftShift))
                {
                    if (!crouch)
                    {
                        speed = speed /2;
                        sprint = false;
                    }
                }
                //crouch
                if (Input.GetKeyDown(KeyCode.C))
                {
                    if(!aim)
                    {
                        camera.SetActive(false);
                        crouchCamera.SetActive(true);
                    }
                   
                    crouch = true;
                    canJump = false;
                    if(sprint)
                    speed = speed / 4;
                    else
                    speed = speed / 2;
                    conHeight = conHeight / 2;
                    headBox.center = new Vector3(headBox.center.x, headBox.center.y, headBox.center.z + 0.3f);
                    bodyBox.center = new Vector3(bodyBox.center.x, bodyBox.center.y, bodyBox.center.z + 0.1f);

                    //camera.transform.Translate(Vector3.down);
                    //camera.transform.Translate(body.transform.forward);
                    //camLastPos = body.transform.forward;
                    anim.SetBool("CrouchBool", crouch);
                }
                if(Input.GetKey(KeyCode.C))
                {
                    if(aim)
                    {
                        camera.SetActive(false);
                        crouchCamera.SetActive(false);
                        weaponCamera.SetActive(true);
                    }
                    else
                    {
                        camera.SetActive(false);
                        crouchCamera.SetActive(true);
                        weaponCamera.SetActive(false);
                    }
                }
                if (Input.GetKeyUp(KeyCode.C))
                {
                    if (!aim)
                    {
                        crouchCamera.SetActive(false);
                        camera.SetActive(true);    
                    }
                    else
                    {
                        crouchCamera.SetActive(false);
                        camera.SetActive(false);
                    }
                 
                    crouch = false;
                    canJump = true;
                    if (sprint)
                        speed = speed * 4;
                    else
                        speed = speed * 2;
                    conHeight = conHeight * 2;
                    headBox.center = new Vector3(headBox.center.x, headBox.center.y, headBox.center.z - 0.3f);
                    bodyBox.center = new Vector3(bodyBox.center.x, bodyBox.center.y, bodyBox.center.z - 0.1f);

                    //camera.transform.Translate(Vector3.up);
                    //camera.transform.Translate(-camLastPos);
                    anim.SetBool("CrouchBool", crouch);
                }
            }
            

        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(groundCheck.transform.position, groundDistance);
    }

    public void Movement()
    {
        isGrounded = Physics.CheckSphere(groundCheck.transform.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = body.transform.right * x + body.transform.forward * z;
        anim.SetFloat("VecX", x,0.25f,Time.deltaTime);
        anim.SetFloat("VecZ", z, 0.25f, Time.deltaTime);
        controller.Move(move * speed * Time.deltaTime);

        //jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && canJump)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;

        //sound
        if(x!=0||z!=0 &&isGrounded)
        {
            pv.RPC("RPC_SoundFoot", RpcTarget.AllBuffered);
        }

        controller.Move(velocity * Time.deltaTime);
    }

    public void Shoot() // will be changed later for event anim
    {
        canShoot = false;
        ammo--;
        ammoText.text = "Bullets: " + ammo+"/30";
        pv.RPC("RPC_SoundShoot", RpcTarget.AllBuffered);

        Transform camShootPos;
        if (aim)
            camShootPos= weaponCamera.transform;
        else if(crouch)
            camShootPos= crouchCamera.transform;
        else
            camShootPos = camera.transform;

        // Shoot raycast
        RaycastHit objectHit;

        //  Debug.DrawRay(shootPos.transform.position, shootPos.transform.forward *50f, Color.green,10);
        //if (Physics.Raycast(shootPos.transform.position, shootPos.transform.forward, out objectHit, 50))//50 is the distance of shooting

        Debug.DrawRay(camShootPos.position, camShootPos.forward * shootDis, Color.green, 10);
        if (Physics.Raycast(camShootPos.position, camShootPos.forward, out objectHit, shootDis))//50 is the distance of shooting
            {
                if (objectHit.collider.gameObject.tag.Equals("Head"))
                {
                   if(objectHit.collider.gameObject.transform.root.tag.Equals("RedTeam") && this.transform.tag.Equals("BlueTeam"))
                    {
                    Hit(objectHit.collider.gameObject.transform.root.gameObject, true);
                   // objectHit.collider.gameObject.transform.root.gameObject.GetComponent<Player>().currentHp -= 90;
                    Debug.Log("hit");
                    }

                    else if (objectHit.collider.gameObject.transform.root.tag.Equals("BlueTeam") && this.transform.tag.Equals("RedTeam"))
                    {
                    Hit(objectHit.collider.gameObject.transform.root.gameObject, true);
                    //  objectHit.collider.gameObject.transform.root.gameObject.GetComponent<Player>().currentHp -= 90;
                    Debug.Log("hit");
                    }
                }
                else if(objectHit.collider.gameObject.tag == "Body")
                {
                    if (objectHit.collider.gameObject.transform.root.tag.Equals("RedTeam") && this.transform.tag.Equals("BlueTeam"))
                    {
                    Hit(objectHit.collider.gameObject.transform.root.gameObject, false);
                    //  objectHit.collider.gameObject.transform.root.gameObject.GetComponent<Player>().currentHp -= 30;
                    Debug.Log("hit");
                    }

                    else if (objectHit.collider.gameObject.transform.root.tag.Equals("BlueTeam") && this.transform.tag.Equals("RedTeam"))
                    {
                    Hit(objectHit.collider.gameObject.transform.root.gameObject, false);
                   // objectHit.collider.gameObject.transform.root.gameObject.GetComponent<Player>().currentHp -= 30;
                    Debug.Log("hit");
                    }
            }

              Debug.Log("Raycast hitted to: " + objectHit.collider.gameObject.tag);

           GameObject sign= GameObject.Instantiate(signOfHit, objectHit.point,Quaternion.identity);
            Destroy(sign, 2f);
            
            }

            if(aim)
            StartCoroutine(weaponCamera.GetComponent<MouseLook>().ShakeCamera(0.2f));
            else if(crouch)
            StartCoroutine(crouchCamera.GetComponent<MouseLook>().ShakeCamera(0.2f));
            else
            StartCoroutine(camera.GetComponent<MouseLook>().ShakeCamera(0.2f));


    }
    public void Hit(GameObject hit,bool head)
    {
        Player enemy=hit.GetComponent<Player>();
        if (head)
        {
        
            if(enemy.currentHp-90 <= 0)
            {
              if(!enemy.isDead)
                enemy.pv.RPC("RPCDead", RpcTarget.AllBuffered);
               // anim.SetTrigger("Dead");
            }
            else
            {
                enemy.pv.RPC("RPC_Hit", RpcTarget.AllBuffered, 90);
              //  anim.SetTrigger("Hit");


            }

        }
        else
        {
            // enemy.currentHp -= 30;
            if (!enemy.isDead)
                if (enemy.currentHp-30 <= 0)
            {
                enemy.pv.RPC("RPCDead", RpcTarget.AllBuffered);
            }
            else
            {
                enemy.pv.RPC("RPC_Hit", RpcTarget.AllBuffered, 30);
               // anim.SetTrigger("Hit");
            }
        }
    }
    public void Reload()
    {
        ammo = bullets;
        ammoText.text = "Bullets: " + ammo + "/30"; ;
    }

    [PunRPC]
    public void RPCDead()
    {
        
        if (pv.IsMine && !isDead)
        {
            isDead = true;
            //anim.SetBool("DeadBool", true);
            anim.SetTrigger("Dead");
            pv.RPC("RPC_Sound2D", RpcTarget.All, 4);
            hpText.text = "Health: " + 0;
            UpdateScore();
            StartCoroutine("reviveCooldown");
            Debug.Log("dead rpc");
        }
       

    }
    [PunRPC]
    public void RPCRevive()
    { 
            Reload();
            currentHp = hp;
            hpText.text = "Hp: " + currentHp;
            transform.position = firstPosPlayer;
            body.transform.position = firstPos;
            speed = originalSpeed;
            isDead = false;
            isHit = false;
            crouch = false;
            aim = false;
            anim.SetBool("DeadBool", false);
      
        //animator set back to idle/run
    } 

    IEnumerator reviveCooldown()
    {
        yield return new WaitForSeconds(5f);
        pv.RPC("RPCRevive", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void RPCChangeGet()
    {
        myTeam = GameManager.gm.nextPlayerTeam;
        if(myTeam==1)
        {
            head.GetComponent<SkinnedMeshRenderer>().material.color = Color.red;
            this.tag = "RedTeam";
            //
            transform.position = GameManager.gm.spawnPoints[0].position;
            firstPos = GameManager.gm.spawnPoints[0].position; ;
            firstPosPlayer = GameManager.gm.spawnPoints[0].position; ;
            myTeamText.text = "My Team: Red";
            myTeamText.color = Color.red;
        }
        else
        {
            head.GetComponent<SkinnedMeshRenderer>().material.color = Color.blue;
            this.tag = "BlueTeam";
            //
            transform.position = GameManager.gm.spawnPoints[2].position;
            firstPos = GameManager.gm.spawnPoints[2].position; ;
            firstPosPlayer = GameManager.gm.spawnPoints[2].position; ;
            myTeamText.text = "My Team: Blue";
            myTeamText.color = Color.blue;
        }
        
        GameManager.gm.UpdateTeam();
        pv.RPC("RPCChangerSent", RpcTarget.OthersBuffered,myTeam);
    }
    [PunRPC]
    void RPCChangerSent(int whichTeam)
    {
        myTeam = whichTeam;
        if (myTeam == 1)
        {
            head.GetComponent<SkinnedMeshRenderer>().material.color = Color.red;
            this.tag = "RedTeam";
            //
            transform.position = GameManager.gm.spawnPoints[0].position;
            firstPos = GameManager.gm.spawnPoints[0].position; ;
            firstPosPlayer = GameManager.gm.spawnPoints[0].position; ;
            myTeamText.text = "My Team: Red";
            myTeamText.color = Color.red;

        }
        else
        {
            head.GetComponent<SkinnedMeshRenderer>().material.color = Color.blue;
            this.tag = "BlueTeam";
            //
            transform.position = GameManager.gm.spawnPoints[2].position;
            firstPos = GameManager.gm.spawnPoints[2].position; ;
            firstPosPlayer = GameManager.gm.spawnPoints[2].position; ;
            myTeamText.text = "My Team: Blue";
            myTeamText.color = Color.blue;
        }

    }
    void UpdateScore()
    {
        Debug.Log("update score rpc");
        
            if (myTeam == 1)
                blueScore++;
            else
                redScore++;
        foreach (Player ps in GameManager.gm.playersScript)
            ps.pv.RPC("RPCScore", RpcTarget.AllBuffered, blueScore, redScore);
        
     
    }
    void UpdateScore2()
    {
        Debug.Log("update score rpc2");

        if (myTeam == 1)
            blueScore++;
        else
            redScore++;
      


    }

    [PunRPC]
    void RPCScore(int blueScoreSent,int redScoreSent)
    {

        blueScore = blueScoreSent;
        redScore = redScoreSent;

        scoreText.text ="Blue team: " + blueScore + "\n" + "Red Team: " + redScore;
       
    }

    [PunRPC]
    void RPCScore2(int blueScoreSent, int redScoreSent)
    {
      
            blueScore = blueScoreSent;
            redScore = redScoreSent;

            scoreText.text = "Blue team: " + blueScore + "\n" + "Red Team: " + redScore;
            Debug.Log("score rpc2");
            Debug.Log("pv: " + pv.ViewID + " blue: " + blueScore + " red: " + redScore);
        
    }



    [PunRPC]
    void RPC_Hit(int damage)
    {
        // anim.SetBool("HitBool", true);
        anim.SetTrigger("Hit");
        currentHp -= damage;
        hpText.text = "Health: " + currentHp;
        pv.RPC("RPC_Sound2D", RpcTarget.All,3);
        Debug.Log("hit rpc");
    }

    [PunRPC]
    void RPC_SoundShoot()
    {
        soundSource.PlayOneShot(clips[0]);
    }
    [PunRPC]
    void RPC_SoundFoot()
    {
        if (!soundSourceFoot.isPlaying)
            soundSourceFoot.PlayOneShot(clips[5]);
    }

    [PunRPC]
    void RPC_Sound2D(int clipNum)
    {
        if (!soundSource2d.isPlaying && pv.IsMine)
            soundSource2d.PlayOneShot(clips[clipNum]);
    }

   
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(currentHp);
           
        }
        else
        {
            this.currentHp = (int)stream.ReceiveNext();
            
        }
    }
}
