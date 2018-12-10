using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeFlip : MonoBehaviour {
	public enum AXIS { x, y, z };
	public enum DIR { Right, Left };
	public DIR Direction;
	public AXIS axis;
	public GameObject rotatingCube;
	public float speed = 2;
	private Vector3 [] path;
	public Vector3 node1, node2, node3, node4;
	private int currentNodeIndex, nextNodeIndex;
	public Vector3 axisOfRot;
	private int rotationDirection;
	public GameObject player;
	private PlayerObject playerScript;
	private PlayerObject.directions currentGravity;
	private Transform parentTransform;
	public GameObject parent;
	public Camera playerCam;
	public GameObject triggerBox;
	public int pause;
	private bool paused;
	private Vector3 nextNodePos;
	private bool timerReached;
	private float timer = 0;



	float dist(float a1, float a2, float b1, float b2) {
		return Mathf.Abs(Mathf.Sqrt(Mathf.Pow(a2-a1,2)+Mathf.Pow(b2-b1,2)));
	}

	IEnumerator waiter(int time) {
		paused = false;
		yield return new WaitForSeconds(time);
	}

	// Use this for initialization
	void Start () {

		currentNodeIndex = 0;
		nextNodeIndex = currentNodeIndex + 1;

		rotationDirection = (Direction == DIR.Right) ? -1 : 1;
		axisOfRot *= rotationDirection;
		playerScript = player.GetComponent<PlayerObject>();
		currentGravity = playerScript.getGravityDirection();
		parentTransform = GetComponentInParent<Transform>();
		playerCam = player.GetComponentInChildren<Camera>();
		//triggerBox = rotatingCube.GetComponentInChildren<GameObject>();
		paused = false;
		timerReached = false;

		



		
		
		path = new Vector3[4];
		path[0] = node1;
		path[1] = node2;
		path[2] = node3;
		path[3] = node4;

		transform.localPosition = path[0];
	}
	
	// Update is called once per frame
	void Update () {

		if (paused) {
			if (!timerReached)
				timer += Time.deltaTime;

			if (!timerReached && timer > pause) {
				paused = false;
				timerReached = true;
				timer = 0;
			}

			return;
		}


		nextNodeIndex = (currentNodeIndex + 1) % 4; 
		Vector3 nextNodePos = path[nextNodeIndex];
	
		//Debug.Log(paused);
		
		
	
		
		switch(axis) {
			/* case AXIS.x:
			
				if (Mathf.Abs(dist(transform.localPosition.z, rotatingCube.transform.localPosition.z, transform.localPosition.y, rotatingCube.transform.localPosition.y) - dist(rotatingCube.transform.localPosition.z, nextNodePos.z, rotatingCube.transform.localPosition.y, nextNodePos.y)) > 0.01) {
					rotatingCube.transform.RotateAround(transform.localPosition, axisOfRot, 3f *speed);
				}
				else {
					currentNodeIndex++;
					currentNodeIndex = currentNodeIndex % 4;
					transform.localPosition = path[currentNodeIndex]; 
				}
				break;
			case AXIS.y:
				if (Mathf.Abs(dist(transform.localPosition.z, rotatingCube.transform.localPosition.z, transform.localPosition.x, rotatingCube.transform.localPosition.x) - dist(rotatingCube.transform.localPosition.z, nextNodePos.z, rotatingCube.transform.localPosition.x, nextNodePos.x)) > 0.01) {
					rotatingCube.transform.RotateAround(transform.position, axisOfRot, 3f *speed);
				}
				else {
					currentNodeIndex++;
					currentNodeIndex = currentNodeIndex % 4;
					transform.localPosition = path[currentNodeIndex];
				}
				break;*/
			case AXIS.z:

				if ((Mathf.Abs(dist(transform.localPosition.x, rotatingCube.transform.localPosition.x, transform.localPosition.y, rotatingCube.transform.localPosition.y) - dist(rotatingCube.transform.localPosition.x, nextNodePos.x, rotatingCube.transform.localPosition.y, nextNodePos.y)) > 0.01) ) {
					rotatingCube.transform.RotateAround(transform.position, axisOfRot, 3f * speed);
				}
				else {
					if (pause != 0) {
						paused = true;
						timerReached = false;
					}

					currentNodeIndex++;
					currentNodeIndex = currentNodeIndex % 4;
					transform.localPosition = path[currentNodeIndex];
					//Debug.Log("Pause"); 
				}
				break;
		}
	}
}

