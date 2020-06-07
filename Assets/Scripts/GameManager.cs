using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.IO;


public class GameManager : MonoBehaviour
{
    public static GameManager gm;
    [SerializeField]
    GameObject playerPrefabs;
    [SerializeField]
    public List<Transform> spawnPoints;
    private GameObject player;
    private PhotonView pv;
    public int nextPlayerTeam;
    public List<Player> playersScript;
    public List<GameObject> players;


    //global
    int countPlayers;

    private void Awake()
    {
        pv = this.gameObject.GetComponent<PhotonView>();
    }

    private void OnEnable()
    {
        if (GameManager.gm == null)
            GameManager.gm = this; 
    }
    // Start is called before the first frame update
    void Start()
    {
        Camera.main.gameObject.SetActive(false);
        countPlayers = PhotonNetwork.CountOfPlayersInRooms;
        CreatePlayer();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
            FindPlayers();
    }
    void CreatePlayer()
    {
        int randomSpawn = Random.Range(0, 4);
        Debug.Log("Creating player");
        //GameObject player=   PhotonNetwork.Instantiate(Path.Combine("Prefabs","Cube"), spawnPoints[randomSpawn].position, Quaternion.identity);
         player=  PhotonNetwork.Instantiate(Path.Combine("Prefabs","Player"), spawnPoints[randomSpawn].position, Quaternion.identity);
       // StartCoroutine("Find");





    }
    private void LateUpdate()
    {
        FindPlayers();
    }

    public void UpdateTeam()
    {
        if (nextPlayerTeam == 1)
            nextPlayerTeam = 2;
        else
            nextPlayerTeam = 1;
    }

    public void FindPlayers()
    {
        playersScript = new List<Player>();
        GameObject[] Red = GameObject.FindGameObjectsWithTag("RedTeam");
        GameObject[] Blue = GameObject.FindGameObjectsWithTag("BlueTeam");
        foreach (GameObject g in Red)
        {
            Debug.Log("red");
            players.Add(g);
            playersScript.Add(g.GetComponent<Player>());
        }
           
        foreach (GameObject g in Blue)
        {
            Debug.Log("blue");
            players.Add(g);
            playersScript.Add(g.GetComponent<Player>());
        }
          
        Debug.Log("find");
    }
    
    //IEnumerator Find()
    //{
    //    yield return new WaitForSeconds(10f);
    //    FindPlayers();
    //}



}
