using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorMaterialGenerator : MonoBehaviour {

	// Use this for initialization
	public void Start ()
	{
		GameObject [] gameObjects = GameObject.FindGameObjectsWithTag ("Floor");

		//	Fetch the Renderer from the GameObject
        Renderer rend = GetComponent <Renderer> ();

		for (int i = 0; i < gameObjects.Length; i++)
		{
			//	Fetch the Renderer from the GameObject
			rend = gameObjects [i].GetComponent <Renderer> ();

			//	Set the main Color of the Material to something Random
        	rend.material.SetColor ("_Color", new Color(
				Random.Range (0f, 1f), 
				Random.Range (0f, 1f), 
				Random.Range (0f, 1f)
			));
		}
	}
}
