using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Unity.Burst.CompilerServices;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public enum Angle
{
	Up,
	Forward
}
public class MouseClickCut : MonoBehaviour
{
    public Angle angle;
   
    //drag drop the Joystick child in the Inspector to animate
    // the joystick when moved
    public Transform Joystick;

    //this refers to the vive's touch pad or oculus's joystick
    public SteamVR_Action_Vector2 moveAction = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("platformer", "Move");
    //this refers to a click event on the touch pad/joystick
    public SteamVR_Action_Boolean jumpAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("platformer", "Jump");

    //multiplier for ball movement
    public float forceMult = 2.0f;

    //vertical force to add for jumping
    public float upMult = 250.0f;

    //the original scene was on a different scale, so we've modified the multipler
    public float joyMove = 0.01f;

    //Interactable script of this GameObject
    private Interactable interactable;

    void Start()
    {
        //get the Interactable script on this GameObject (the controller)
        interactable = GetComponent<Interactable>();
    }

    void Update(){

        Vector3 movement = Vector2.zero;
        bool jump = false;
        //if the controller is attached to the hand...
        if (interactable.attachedToHand)
        {
            //get the hand's type, LeftHand or RightHand so that the controller can be used in either hand
            SteamVR_Input_Sources hand = interactable.attachedToHand.handType;
            //get the touch pad/joystick x/y coordniates of that particular hand
            Vector2 m = moveAction[hand].axis;
            movement = new Vector3(m.x, 0, m.y);

            //if someone has "clicked" the touchpad/joystick, then they jump
            jump = jumpAction[hand].stateDown;
        }

        void OnCollisionStay(Collision collision)
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                print(contact.thisCollider.tag + " hit " + contact.otherCollider.tag);
                // Visualize the contact point
                Debug.DrawRay(contact.point, contact.normal, Color.white);
                
                if (contact.thisCollider.tag == contact.otherCollider.tag)
                {
                    GameObject victim = contact.otherCollider.gameObject;
                    if (angle == Angle.Up)
                    {
                        Cutter.Cut(victim, contact.point, Vector3.up);

                    }
                    else if (angle == Angle.Forward)
                    {
                        Cutter.Cut(victim, contact.point, Vector3.forward);

                    }
                }
            }

        
            // Check if the object the player collided with has the "PickUp" tag.


        }
    }
}

