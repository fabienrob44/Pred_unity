﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.UI;

public class NetworkManagerCustom : NetworkManager {

	public int chosenCharacter = 0;


	//subclass for sending network messages
	public class NetworkMessage : MessageBase {
		public int chosenClass;
	}

	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader) {

		// Read client message and receive index
		var stream = extraMessageReader.ReadMessage<IntegerMessage> ();
		chosenCharacter = stream.value;

		//Select the prefab from the spawnable objects list
		var playerPrefab = spawnPrefabs[chosenCharacter];       

		// Create player object with prefab
		var player = Instantiate(playerPrefab, playerPrefab.transform.position, Quaternion.identity) as GameObject;        

		// Add player object for connection
		NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
	}

	public override void OnClientConnect(NetworkConnection conn) {
		// Create message to set the player
		IntegerMessage msg = new IntegerMessage(chosenCharacter);      

		// Call Add player and pass the message
		ClientScene.AddPlayer(conn, 0, msg);
	}


	public override void OnClientSceneChanged(NetworkConnection conn) {
		//base.OnClientSceneChanged(conn);
	}

	// Therapist's button
	public void StartupHost()
	{
		SetPort ();
		NetworkManager.singleton.StartHost ();
		chosenCharacter = 0;
	}

	// Patient's button
	public void JoinGame()
	{
		SetIPAddress ();
		SetPort ();
		NetworkManager.singleton.StartClient ();
		chosenCharacter = 1;
	}

	void SetIPAddress()
	{
		string ipAdress = GameObject.Find ("InputFieldIPAddress").transform.FindChild ("Text").GetComponent<Text> ().text;
		NetworkManager.singleton.networkAddress = ipAdress;
	}

	void SetPort()
	{
		NetworkManager.singleton.networkPort = 7777;
	}

	void OnLevelWasLoaded(int level)
	{
		if (level == 0) {
			SetupMenuSceneButtons ();
		} 
		else 
		{
			SetupOtherSceneButtons ();
		}
	}

	// To recreate button listeners when scene is changed
	void SetupMenuSceneButtons() 
	{
		GameObject.Find ("ButtonStartHost").GetComponent<Button>().onClick.RemoveAllListeners ();
		GameObject.Find ("ButtonStartHost").GetComponent<Button>().onClick.AddListener (StartupHost);

		GameObject.Find ("ButtonJoinGame").GetComponent<Button>().onClick.RemoveAllListeners ();
		GameObject.Find ("ButtonJoinGame").GetComponent<Button>().onClick.AddListener (JoinGame);
	}

	void SetupOtherSceneButtons()
	{	
		/*GameObject.Find ("ButtonDisconnect").GetComponent<Button>().onClick.RemoveAllListeners ();
		GameObject.Find ("ButtonDisconnect").GetComponent<Button>().onClick.AddListener (NetworkManager.singleton.StopHost);*/
	}





}
