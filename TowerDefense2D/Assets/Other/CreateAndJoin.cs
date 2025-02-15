using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.SceneManagement;

public class CreateAndJoin : MonoBehaviourPunCallbacks
{
    public TMP_InputField input_Create;
    public TMP_InputField input_Join;
    public GameObject CustomRoomView;
    public Toggle CustomRoomToggle;

    void Update(){
        if(CustomRoomToggle.isOn){
            CustomRoomView.SetActive(true);
        }else{
            CustomRoomView.SetActive(false);
        }
    }
    public void DeckOpen(){
        SceneManager.LoadScene("Deck");
    }
    public void CreateRoom()
    {
        if(!String.IsNullOrEmpty(input_Create.text)){
            PhotonNetwork.CreateRoom(input_Create.text);
        }
    }

    public void JoinRandomOrCreateRoom()
    {
        PhotonNetwork.JoinRandomOrCreateRoom();
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(input_Join.text);
    }
    public void JoinRoomList(string RoomName){
        PhotonNetwork.JoinRoom(RoomName);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("SampleScene");
    }
}
