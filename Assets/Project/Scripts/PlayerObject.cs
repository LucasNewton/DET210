using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerObject : MonoBehaviour {
	//	Enums
	public enum directions {
		left, right, up, down, forward, backward
	}

	public enum axis {
		x, y, z
	}

	private struct inputKeys {
		public bool left, right, forward, backward, space, escape, lShift;
	}
	
	public Vector3 respawnPosition = Vector3.zero;

	//	SerializeFields
	[SerializeField]					private directions gravityDirection;
	[SerializeField] [Range (1, 500)]   private int mouseSensitivity;
	[SerializeField] 					private bool lockedCursor;
	[SerializeField] [Range (5, 20)]	private float walkSpeed;
	[SerializeField] [Range (0, 5)]		private float playerRotateSpeed;
	[SerializeField] [Range (1, 100)]	private float jumpHeight;
	[SerializeField] [Range (1, 6)]		private float horizontalJumpingBoost;
	[SerializeField] [Range (50, 300)]	private float dashSpeed;
	[SerializeField] [Range (1, 20)]	private int dashingCoolDown;
	[SerializeField] [Range (1, 20)]	private int dashingDuration;
	[SerializeField] [Range (1, 20)]	private float gravityStrengthGUI;
	[SerializeField] [Range (3, 15)]	private int deathFallDistance;

	//	other object attributes
	private Camera playerCamera;
	private CharacterController controller;
	private GameObject player;
	private GameObject worldAxis;
	private inputKeys playerKeysInput;
	private Vector3 playerVelocity;
	//	Width, height, depth
	private directions gravityFrom;
	private axis axisToRotateOn;
	private float gravityStrength;
	private bool usingLaunchPad;
	private bool respawning;
	private int launchStrength;
	private bool isDashing;
	private int dashingDurationSave;
	private int fallDistance;
	private bool countingFallDistance;
	private int dashingCoolDownSave;
	private float gravityStrengthSave;
	private bool worldRotating;
	private float degreesRotated;
	private int degreesToRotate;
	private float mouseRotY;
    private float mouseRotX;

	//	Debugging variables
	private ILogger logger;

	//	Use this for initialization
	public void Start () {
		//	Other objects
		playerCamera = GetComponentInChildren <Camera> ();
		controller = GetComponent <CharacterController> ();
		worldAxis = GameObject.FindGameObjectWithTag ("World Axis");
		player = this.gameObject;

		//	player stuff
		playerVelocity = Vector3.zero;
		worldRotating = false;
		respawning = false;
		usingLaunchPad = false;
		gravityFrom = gravityDirection;
		gravityStrength = gravityStrengthGUI / 19.62f;
		gravityStrengthSave = gravityStrength;
		//	Save what the game dev wants but set to zero so we can dash when world is loaded
		dashingCoolDownSave = dashingCoolDown;
		dashingDurationSave = dashingDuration;
		dashingCoolDown = 0;
		degreesRotated = 0;
		degreesToRotate = 0;
		countingFallDistance = false;

		//	Camera stuff
		Vector3 mouseRot = transform.rotation.eulerAngles;
		mouseRotY = mouseRot.y;
        mouseRotX = mouseRot.x;

		if (respawnPosition == Vector3.zero) {
			respawnPosition = new Vector3 (0, 9.08f, -8);
		}
	}

	private void printGravityDirection () {
		print ("gravityDirection: " + gravityDirection);
	}

	//	Called indepently from fps
	public void FixedUpdate () {
		//if (!respawning) {
			movePlayer ();
		//}
	}
	
	//	Update is called once per frame
	public void Update () {
		if (!respawning) {
			if (worldRotating) {
				gravityChangeRotateWorld ();
			}
			rotatePlayerToFaceMouseDirection ();
			getKeyboardInput ();
			invokeKeyboardInput ();

			if (playerVelocity.y < -5 && !controller.isGrounded && !countingFallDistance && !isDashing && !worldRotating) {
				countingFallDistance = true;
				fallDistance = 0;
				incrementFallDistance ();
			}
			else {
				if (playerVelocity.y >= -5 && controller.isGrounded) {
					countingFallDistance = false;
					fallDistance = 0;
				}
			}
		}
		else {
			if (worldRotating) {
				gravityChangeRotateWorld ();
			}
			else {
				transform.position = respawnPosition;
				respawning = false;
			}
		}
	}

	public void LateUpdate () {
		if (!respawning) {
			moveCameraWithMouse ();
		}
	}
	
	public void changeGravity (directions dir) {
		if (gravityDirection != dir) {
			gravityFrom = gravityDirection;
			gravityDirection = dir;
			worldRotating = true;
			gravityStrength = 0;

			processPlayerRotationRequest ();
		}
	}
	
	public bool getWorldRotating () {
		return worldRotating;
	}
	
	public directions getGravityDirection () {
		return gravityDirection;
	}
	
	public void respawnPlayer () {
		/*Scene scene = SceneManager.GetActiveScene();
		SceneManager.LoadScene(scene.name);

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
		}*/

		respawning = true;
		changeGravity (directions.down);
	}
	
	private void getKeyboardInput () {
		if (Input.GetKey (KeyCode.Escape)) {
            playerKeysInput.escape = true;
        }
		else {
			playerKeysInput.escape = false;
		}

		if (Input.GetKey (KeyCode.W) || Input.GetKeyDown (KeyCode.W)) {
            playerKeysInput.forward = true;
        }
        else {
            playerKeysInput.forward = false;
        }

        if (Input.GetKey (KeyCode.A) || Input.GetKeyDown (KeyCode.A)) {
           playerKeysInput.left = true;
        }
        else {
            playerKeysInput.left = false;
        }

        if (Input.GetKey (KeyCode.S) || Input.GetKeyDown (KeyCode.S)) {
           playerKeysInput.backward = true;
        }
        else {
            playerKeysInput.backward = false;
        }

        if (Input.GetKey (KeyCode.D) || Input.GetKeyDown (KeyCode.D)) {
            playerKeysInput.right = true;
        }
        else {
            playerKeysInput.right = false;
        }

		if (Input.GetKey (KeyCode.Space) || Input.GetKeyDown (KeyCode.Space)) {
            playerKeysInput.space = true;
        }
        else {
            playerKeysInput.space = false;
        }

		if (Input.GetKey (KeyCode.LeftShift) || Input.GetKeyDown (KeyCode.LeftShift)) {
			playerKeysInput.lShift = true;
		}
		else {
			playerKeysInput.lShift = false;
		}

		//	Print something for debugging on command
		if (Input.GetKeyDown (KeyCode.J)) {
			changeGravity (directions.left);
		}

		if (Input.GetKeyDown (KeyCode.L)) {
			changeGravity (directions.right);
		}

		if (Input.GetKeyDown (KeyCode.I)) {
			changeGravity (directions.forward);
		}

		if (Input.GetKeyDown (KeyCode.K)) {
			changeGravity (directions.backward);
		}

		if (Input.GetKeyDown (KeyCode.U)) {
			changeGravity (directions.up);
		}

		if (Input.GetKeyDown (KeyCode.O)) {
			changeGravity (directions.down);
		}
	}

	private void invokeKeyboardInput () {
		if (playerKeysInput.escape) {
			lockedCursor = !lockedCursor;
		}
		
		if (controller.isGrounded) {
			playerVelocity = Vector3.zero;
		}
		else {
			playerVelocity.x = 0;
			playerVelocity.z = 0;
		}

		if (!worldRotating) {
			if (playerKeysInput.forward) {
				playerVelocity.z += walkSpeed;
			}
			if (playerKeysInput.backward) {
				playerVelocity.z -= walkSpeed;
			}
			if (playerKeysInput.left) {
				playerVelocity.x -= walkSpeed;
			}
			if (playerKeysInput.right) {
				playerVelocity.x += walkSpeed;
			}

			if (playerKeysInput.lShift && dashingCoolDown == 0) {
				isDashing = true;
				dashingCoolDown = dashingCoolDownSave;

				//	start cool down after one second to match unity settings in scene
				Invoke ("decrementDashCoolDown", 1);
			}
		}

		if (playerKeysInput.space && controller.isGrounded) {
			playerVelocity.y += jumpHeight;

			//	Apply Jumping boost horizotally only
			playerVelocity.x *= horizontalJumpingBoost;
			playerVelocity.z *= horizontalJumpingBoost;
		}
	}

	private void decrementDashCoolDown () {
		if (dashingCoolDown > 0) {
			dashingCoolDown --;
			if (dashingCoolDown != 0) {
				Invoke("decrementDashCoolDown", 1);
			}
		}
	}

	private void incrementFallDistance () {
		fallDistance ++;
		if (fallDistance >= deathFallDistance) {
			respawnPlayer ();
			fallDistance = 0;
			countingFallDistance = false;
		}
		else {
			if (countingFallDistance){
				Invoke ("incrementFallDistance", 1);
			}
		}
	}
	
	private void calculatePlayersVelocityBeforeMove () {
		if (isDashing) {
			//	Guarentee forward movement
			playerVelocity.z += dashSpeed;
			if (dashingDuration == 0) {
				isDashing = false;
				dashingDuration = dashingDurationSave;
			}
			else {
				//	This works because this function is called from fixedUpdate
				dashingDuration --;
			}
		}

		if ((!controller.isGrounded || playerVelocity.y > gravityStrength || controller.velocity.y > gravityStrength) && !usingLaunchPad) {
			playerVelocity.y -= gravityStrength;
		}

		if (usingLaunchPad) {
			playerVelocity.y += launchStrength;
			usingLaunchPad = false;
		}
	}
	
	private void movePlayer () {
		calculatePlayersVelocityBeforeMove ();

		if (playerVelocity != Vector3.zero) {
			transform.eulerAngles = new Vector3 (transform.eulerAngles.x, playerCamera.transform.eulerAngles.y, transform.eulerAngles.z);
			playerVelocity = transform.TransformDirection (playerVelocity);
		}

		controller.Move (playerVelocity * Time.deltaTime);
	}
	
	private void processPlayerRotationRequest ()
    {
       if (gravityFrom == directions.down && gravityDirection == directions.up) {
		   degreesToRotate = 180;
	   }
	   else if (gravityFrom == directions.up && gravityDirection == directions.down) {
		   degreesToRotate = 180;
	   }
	   else if (gravityFrom == directions.left && gravityDirection == directions.right) {
		   degreesToRotate = 180;
	   }
	   else if (gravityFrom == directions.right && gravityDirection == directions.left) {
		   degreesToRotate = 180;
	   }
	   else if (gravityFrom == directions.forward && gravityDirection == directions.backward) {
		   degreesToRotate = 180;
	   }
	   else if (gravityFrom == directions.backward && gravityDirection == directions.forward) {
		   degreesToRotate = 180;
	   }
	   else {
		   degreesToRotate = 90;
	   }

		// counter-clockwise rotation on z-axis is negative
	   if (gravityFrom == directions.down && gravityDirection == directions.right) {
		   playerRotateSpeed = -playerRotateSpeed;
	   }
	   else if (gravityFrom == directions.right && gravityDirection == directions.up) {
		    playerRotateSpeed = -playerRotateSpeed;
	   }
	   else if (gravityFrom == directions.up && gravityDirection == directions.left) {
		    playerRotateSpeed = -playerRotateSpeed;
	   }
	   else if (gravityFrom == directions.left && gravityDirection == directions.down) {
		    playerRotateSpeed = -playerRotateSpeed;
	   }
	   else if (gravityFrom == directions.down && gravityDirection == directions.forward) {
		   playerRotateSpeed = -playerRotateSpeed;
	   }
	   else if (gravityFrom == directions.left && gravityDirection == directions.forward) {
		   playerRotateSpeed = -playerRotateSpeed;
	   }
	   else {
		   playerRotateSpeed = Mathf.Abs (playerRotateSpeed);
	   }
    }

	private List <GameObject> getListOfGameObjects () {
		GameObject[] go = GameObject.FindGameObjectsWithTag("Floor");
		List <GameObject> gameObjects = new List <GameObject> ();

		gameObjects.Add (GameObject.FindGameObjectWithTag ("World Axis"));

		for (int i = 0; i < go.Length; i++) {
			//	Add all the floors
			gameObjects.Add (go [i]);
		}

		go = GameObject.FindGameObjectsWithTag("Button Platform");
		
		for (int i = 0; i < go.Length; i++) {
			//	Add all the Button Platforms, this also adds all Buttons since they are children of the platforms
			gameObjects.Add (go [i]);
		}

		go = GameObject.FindGameObjectsWithTag("Jump Pad");

		for (int i = 0; i < go.Length; i++) {
			//	Add all the Jump Pads
			gameObjects.Add (go [i]);
		}

		go = GameObject.FindGameObjectsWithTag("Player");

		for (int i = 0; i < go.Length; i++) {
			//	Add all the Jump Pads
			gameObjects.Add (go [i]);
		}

		return gameObjects;
	}
	
	private void gravityChangeRotateWorld () {
		List <GameObject> gameObjects = getListOfGameObjects ();

		if ((gravityDirection == directions.forward || gravityDirection == directions.backward || gravityFrom == directions.forward || gravityFrom == directions.backward) && degreesRotated < degreesToRotate) {
            for (int i = 0; i < gameObjects.Count; i++) {
				gameObjects [i].transform.RotateAround (Vector3.zero, new Vector3 (1, 0, 0), playerRotateSpeed);
			}

			player.transform.RotateAround (player.transform.position, new Vector3 (1, 0, 0), -playerRotateSpeed);
        }
		else if (degreesRotated < degreesToRotate) {
			for (int i = 0; i < gameObjects.Count; i++) {
				gameObjects [i].transform.RotateAround (Vector3.zero, new Vector3 (0, 0, 1), playerRotateSpeed);
			}

			player.transform.RotateAround (player.transform.position, new Vector3 (0, 0, 1), -playerRotateSpeed);
		}

		degreesRotated += Mathf.Abs(playerRotateSpeed);

		if (degreesRotated >= degreesToRotate) {
			worldRotating = false;
			degreesRotated = 0;
			gravityStrength = gravityStrengthSave;
			playerRotateSpeed = Mathf.Abs (playerRotateSpeed);
		}
    }

	private void gravityChangeRotatePlayer () {
		if (axisToRotateOn == axis.x) {
			player.transform.RotateAround (player.transform.position, new Vector3 (1, 0, 0), -playerRotateSpeed);
		}
		else if (axisToRotateOn == axis.z) {
			player.transform.RotateAround (player.transform.position, new Vector3 (0, 0, 1), -playerRotateSpeed);
		}
	}
	
	private void rotatePlayerToFaceMouseDirection () {
		transform.eulerAngles = new Vector3 (transform.eulerAngles.x, playerCamera.transform.eulerAngles.y, transform.eulerAngles.z);
	}
	
	private void moveCameraWithMouse () {
		if (lockedCursor) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

		mouseRotX += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
		mouseRotY += Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

		//  Prevents you from looking 360 on the X-axis (looking up and down in game)
        mouseRotY = Mathf.Clamp (mouseRotY, -90, 90);
        
		playerCamera.transform.eulerAngles = new Vector3 (-mouseRotY, mouseRotX, transform.eulerAngles.z);
    }

	private void OnControllerColliderHit (ControllerColliderHit collider) {
		if (collider.gameObject.tag == "Button") {
			ButtonObject thisButton = (ButtonObject) collider.gameObject.GetComponent(typeof(ButtonObject));
			if (thisButton.getStartingOrientation () == gravityDirection) {
				thisButton.setBeingPressed (true);
			}
		}
		else if (collider.transform.tag == "Jump Pad") {
            usingLaunchPad = true;
			launchStrength = collider.gameObject.GetComponent<JumpPadObject> ().launchStrength;
        }
	}
}