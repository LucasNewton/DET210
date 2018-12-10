using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipRes : MonoBehaviour {

	private PlayerObject player;

	private void OnTriggerEnter(Collider other) {
		if (other.gameObject.tag == "Player") {
			player.respawnPlayer();
		}
	}

	// Use this for initialization
	void Start () {
		player = (PlayerObject) GameObject.FindGameObjectWithTag("Player").GetComponent(typeof(PlayerObject));
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
