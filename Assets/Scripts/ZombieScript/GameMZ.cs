using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class GameMZ : MonoBehaviour
{
    public static GameMZ gm;
    public List<Transform> spawnPoints;
    public PhotonView pv;
    private GameObject player;
    // Start is called before the first frame update

    private void Awake()
    {
        pv = this.gameObject.GetComponent<PhotonView>();
    }

    private void OnEnable()
    {
        if (GameMZ.gm == null)
            GameMZ.gm = this;
    }
    void Start()
    {
        Camera.main.gameObject.SetActive(false);
        CreatePlayer();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void CreatePlayer()
    {
        player = PhotonNetwork.Instantiate(Path.Combine("Prefabs", "Player2"), Vector3.zero, Quaternion.identity); //need postion need player 2 for zmap
    }
}
