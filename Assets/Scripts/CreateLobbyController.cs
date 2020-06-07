using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CreateLobbyController : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject startBtn;
    [SerializeField] private GameObject startZBtn;
    [SerializeField] private CreateRoomController cr;
    public int roomSize;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        startBtn.SetActive(true);
        startZBtn.SetActive(true);
    }
    public void startBtnPressed()
    {
        startBtn.SetActive(false);
        startZBtn.SetActive(false);
        cr.multiplayerSceneIndex =cr.NormalScene;
        PhotonNetwork.JoinRandomRoom();
        Debug.Log("press on start");
    }
    public void StartZBtn()
    {
        startBtn.SetActive(false);
        startZBtn.SetActive(false);
        cr.multiplayerSceneIndex = cr.zScene;
        PhotonNetwork.JoinRandomRoom();
        Debug.Log("press on start");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("failed");
        CreateRoom();
    }
    
    void CreateRoom()
    {
        Debug.Log("Create room");
        int randomRoomNumber = Random.Range(0, 10000);
        RoomOptions roomOptions = new RoomOptions() { IsVisible = true,IsOpen=true,MaxPlayers=(byte)roomSize };
        PhotonNetwork.CreateRoom("Room " + randomRoomNumber, roomOptions);
        Debug.Log("Room: " + randomRoomNumber);
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("failed to create room");
        CreateRoom();
    }
}
