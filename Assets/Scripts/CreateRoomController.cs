using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CreateRoomController : MonoBehaviourPunCallbacks
{
    [SerializeField] public int multiplayerSceneIndex;
    [SerializeField] public int NormalScene;
    [SerializeField] public int zScene;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }
    public override void OnDisable()
    { 
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("join room");
        StartGame();
    }
    public void StartGame()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Start Game");
            PhotonNetwork.LoadLevel(multiplayerSceneIndex);
        }
    }
}
