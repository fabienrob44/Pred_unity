﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class PatientController : NetworkBehaviour {

	public float h_speed = 2.0f;
	public float v_speed = 2.0f;

    private float yaw = -100.5f;
	private float pitch = 0.0f;

    public Object[] whiteNoiseArray;

    public bool MouseControl = false;
    public Color chosenColor = Color.red;
    public int chosenSound = -1;
    public float chosenVolume = 0.5f;


    [SyncVar(hook = "OnChangeFadeDir")]
    public int fadeDir = 0;


	void Awake()
	{
		DontDestroyOnLoad (this);
	}

	void Start () {
		// Loading resources in advance to prevent lag in Ganzfeld phase
		whiteNoiseArray = Resources.LoadAll ("Audio");
		if (GameObject.Find ("Audio Source") != null) {
			GameObject.Find ("Audio Source").GetComponent<AudioController> ().SetClipArray (whiteNoiseArray);
		}
		print ("Audio chargé");
	}
	
	// Update is called once per frame
	void Update () {

		if (!isLocalPlayer) 
		{
			return;
		}

		//var x = Input.GetAxis("Horizontal") * Time.deltaTime * 3.0f;
		//var z = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;
        yaw += h_speed * Input.GetAxis("Mouse X");
        pitch -= v_speed * Input.GetAxis("Mouse Y");

        if(MouseControl)
        {
            transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
            //transform.Translate(0, x, z);
        }
		
		if (Input.GetKeyDown(KeyCode.Space))
		{
			CmdChangeAgitation();
			print (GetComponent<MapBehaviour> ().agitation);
		}
	}

	[Command] // calls the function only server-side
	public void CmdChangeAgitation()
	{
		if (GetComponent<MapBehaviour> ().agitation < 0.5f) {
			//GetComponent<MapBehaviour> ().setAgitation (0.5f);
			GetComponent<MapBehaviour> ().agitation = 0.5f;
		} else if (GetComponent<MapBehaviour> ().agitation == 0.5f) {
			GetComponent<MapBehaviour> ().setAgitation (1.0f);
		} else 
		{
			GetComponent<MapBehaviour> ().setAgitation (0.0f);
		}

	}
		
	public void OnChangeFadeDir(int direction)
	{
		fadeDir = direction;
		if(Camera.main.GetComponent<Fading>() != null)
		{
			if (direction == 1)
			{
				Camera.main.GetComponent<Fading>().FadeOut();
			}
			else if (direction == -1)
			{
				Camera.main.GetComponent<Fading>().FadeIn();
			}
		}
		print("ONCHANGEFADEDIR: " + direction);
	}

	public override void OnStartLocalPlayer()
	{

        gameObject.transform.position = new Vector3(0, 0, 0);

		GetComponent<CameraFollow>().setTarget(Camera.main.transform);

		print (SceneManager.GetActiveScene ().name);

		if (SceneManager.GetActiveScene ().name == "RelaxingEnv1") {
            // setting up fading tex
            OnChangeFadeDir(-1);
            Camera.main.GetComponent<Fading>().setFadingColor(chosenColor);
            StartCoroutine(SwitchFadingTex(1));
            StartCoroutine(rotatingCam(-90.0f));

            
            print("camera trans : " + Camera.main.transform.rotation);
			GetComponent<MapBehaviour>().enabled = true;

			print ("Mapbehaviour active");
		}
        else
        {
            GetComponent<MapBehaviour>().enabled = false;
			print ("Mapbehaviour desactive");
        }

        if (SceneManager.GetActiveScene().name == "Ganzfeld")
        {
            Camera.main.GetComponent<Fading>().setFadingColor(chosenColor);
            Camera.main.GetComponent<Fading>().setCurrentFadingTex(1);
            OnChangeFadeDir(-1); // start fadeOut after loading Ganzfeld scene

            GameObject.Find("Point light").GetComponent<Light>().color = chosenColor;
        }      
	}

    //not working
    IEnumerator rotatingCam(float angle)
    {
        Camera.main.GetComponent<GvrHead>().trackRotation = false;
        Camera.main.transform.eulerAngles = new Vector3(0, angle, 0);  // <----- camera is not willing to rotate
        Camera.main.GetComponent<GvrHead>().trackRotation = true;
        yield return new WaitForSeconds(2.5f);
    }

    IEnumerator SwitchFadingTex(int index)
    {
        yield return new WaitForSeconds(10.0f);
        Camera.main.GetComponent<Fading>().setCurrentFadingTex(index);
        fadeDir = -1; // car fadeDir est reset à 1 (au lieu d'être -1) à la transition vers  RelaxationEnv1
    }
}
