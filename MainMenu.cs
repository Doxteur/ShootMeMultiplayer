using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public Launcher launcher;

    public void JoinMatch() {

        launcher.Join();
    }

    public void CreateMatch() {
        launcher.Create();
    }

    public void Quitting() {
        Application.Quit();
    }


}
