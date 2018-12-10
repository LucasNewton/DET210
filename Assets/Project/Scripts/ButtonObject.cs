using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonObject : MonoBehaviour {
    //  Enums
    private enum buttonType {
        Winning, Gravity
    };

    //Serialized Fields
    //  Direction gravity is changed to when pressed
    [SerializeField]                        private PlayerObject.directions gravityChangeDirection;
    [SerializeField]                        private buttonType type;
    [SerializeField]                        private PlayerObject.directions startingOrientation;
    // A percent of the height that doesn't get pressed down, default is 0.1 (which would be 10%)
    [SerializeField] [Range (0f, 1f)]       private float percentNotPressable;
    [SerializeField] [Range (0.01f, 1f)]    private float pressSpeed;
    [SerializeField]                        private bool finalWinButton;

    private PlayerObject player;
    private Vector3 size;
    private Vector3 telePlayerLocation;
    private float traveledDistance;
    private bool beingPressed;
    private bool buttonActionBegin;
    private bool buttonActionContinue;
    private float totalTravelDistanceAllowed;
    private bool pressedLastFrame;
    
    public void setBeingPressed (bool newValue) {
        beingPressed = newValue;
    }

    public bool getBeingPressed () {
        return beingPressed;
    }
    
    public PlayerObject.directions getStartingOrientation () {
        return startingOrientation;
    }
    
    // Use this for initialization
	private void Start () {
        size = GetComponent <BoxCollider>().bounds.size;
        player = (PlayerObject) GameObject.FindGameObjectWithTag("Player").GetComponent(typeof(PlayerObject));

        traveledDistance = 0;
        beingPressed = false;
        pressedLastFrame = false;
        buttonActionBegin = false;
        buttonActionContinue = false;

        telePlayerLocation = Vector3.zero;

        //  assume that buttons that aren't orientated down from world creation is created with rotation
        totalTravelDistanceAllowed = size.y - (size.y * percentNotPressable);
	}

	// Update is called once per frame
	public void Update ()
    {
        if (beingPressed) {
            pressedLastFrame = true;
            //  If percent that we have travled is less than max percent that is pressable
            if (traveledDistance / size.y < 1 - percentNotPressable) {
                float newAxisValue = transform.position.y - pressSpeed;
                transform.position = new Vector3 (transform.position.x, newAxisValue, transform.position.z);    
                
                traveledDistance += pressSpeed;
            }
        }
        else {
            if (traveledDistance != 0) {
                float newAxisValue = transform.position.y + pressSpeed;
                transform.position = new Vector3 (transform.position.x, newAxisValue, transform.position.z);    
                
                if (traveledDistance < 0 || traveledDistance - pressSpeed < 0) {
                    traveledDistance = 0;
                }
                else {
                    traveledDistance -= pressSpeed;
                }
            }
        }

        if (traveledDistance >= totalTravelDistanceAllowed && !buttonActionBegin && !buttonActionContinue)
        {
            buttonActionBegin = true;
            buttonActionContinue = false;
        }
        else if (traveledDistance >= totalTravelDistanceAllowed && buttonActionBegin && !buttonActionContinue)
        {
            buttonActionBegin = false;
            buttonActionContinue = true;
        }
        else if (traveledDistance < totalTravelDistanceAllowed)
        {
            buttonActionBegin = false;
            buttonActionContinue = false;
        }


        if (buttonActionBegin)
        {
            //  Do whatever you want the button to do on push (called once)

            if (type == buttonType.Winning)
            {
                //  Do this if there is a different scene that needs to be loaded
                // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

                if (!finalWinButton) {
                    player.respawnPosition = new Vector3 (-213, -2, -585);
                    player.respawnPlayer ();
                }
            }
            else if (type == buttonType.Gravity)
            {
                //  Change gravity pressDirection (yes, it must be done with random middle man for some unreasonable stupid reason!)
                player.changeGravity(gravityChangeDirection);
                pressedLastFrame = !pressedLastFrame;
            }
        }

        if (buttonActionContinue)
        {
            // Do whatever you want to button to do while pushed here (called while pushed down)
        }
	}

    private void LateUpdate () {
        if (!pressedLastFrame) {
            beingPressed = false;
        }
    }
}
