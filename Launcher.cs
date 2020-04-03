using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class ProfileData
{
    public string username;
    public int level;
    public int xp;
    public ProfileData()
    {
        this.username = "Default Username";
        this.level = 0;
        this.xp = 0;
    }

    public ProfileData(string u, int l,int x)
    {
        this.username = u;
        this.level = l;
        this.xp = x;
    }
  
}
public class Launcher : MonoBehaviourPunCallbacks
{
    public InputField usernameField;
    public static ProfileData myProfile = new ProfileData();

    public void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        Connect();
    }
   
    public override void OnJoinedRoom()
    {
        StartGame();
        base.OnJoinedRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Create();
        base.OnJoinRandomFailed(returnCode, message);
    }
    public void Connect()
    {
        PhotonNetwork.GameVersion = "0.0.0";
        PhotonNetwork.ConnectUsingSettings();

    }

    public void Join()
    {
        PhotonNetwork.JoinRandomRoom();
    }
    public void Create()
    {
        PhotonNetwork.CreateRoom("");
    }
    public void StartGame()
    {

        if (string.IsNullOrEmpty(usernameField.text)) {

            myProfile.username = "RANDOM_USER_" + Random.Range(100, 1000); 
        }
        else
        {
            myProfile.username = usernameField.text;

        }

        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            PhotonNetwork.LoadLevel(1);
        }
    }




}
