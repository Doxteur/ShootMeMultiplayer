using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class MouseLook : MonoBehaviourPunCallbacks 
{
    #region Variables

    
    public Transform player;
    public Transform cam;
    public float XSensivity;
    public float YSensitivy;
    public float maxAngle;
    public GameObject cameraParent;
    private Quaternion camCenter;
    public bool cursorLockState = true;
    public Transform weapon;
    #endregion

    #region MonoBehaviour CallBacks

    void Start()
    {
        if (photonView.IsMine)
        {
            cameraParent.SetActive(true);
           

        }
        camCenter = cam.localRotation;
    } 
    
    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine) {
            setX();
            setY();
            UpdateCursorLock();
        }

    }

   
    #endregion

    #region Methods
    void setY()
    {
        float input = Input.GetAxisRaw("Mouse Y") * YSensitivy * Time.deltaTime;
        Quaternion adj = Quaternion.AngleAxis(input,-Vector3.right);
        Quaternion delta = cam.localRotation * adj;

        if(Quaternion.Angle(camCenter,delta) < maxAngle)
        {
            cam.localRotation = delta;
       

        }
        weapon.rotation = cam.rotation;
    }
    void setX()
    {

        float input = Input.GetAxisRaw("Mouse X") * XSensivity * Time.deltaTime;
        Quaternion adj = Quaternion.AngleAxis(input, Vector3.up);
        Quaternion delta = player.localRotation * adj;
        player.localRotation = delta;

        
    }
    void UpdateCursorLock()
    {
        if (cursorLockState)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                cursorLockState = false;
            }
        }else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                cursorLockState = true;
            }
        }
    }

    #endregion
}
