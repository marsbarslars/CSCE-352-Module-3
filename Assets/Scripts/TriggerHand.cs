//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: The object attached to the player's hand that spawns and fires the
//			arrow
//
//=============================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Valve.VR.InteractionSystem
{
	//-------------------------------------------------------------------------
	public class TriggerHand : MonoBehaviour
	{
		private Hand hand;
		private Rifle rifle;

		private GameObject currentRound;
		public GameObject roundPrefab;

		public Transform roundNockTransform;

		public float nockDistance = 0.1f;
		public float lerpCompleteDistance = 0.08f;
		public float rotationLerpThreshold = 0.15f;
		public float positionLerpThreshold = 0.15f;

		private bool allowRoundSpawn = true;
		private bool nocked;
        private GrabTypes nockedWithType = GrabTypes.None;

		private bool inNockRange = false;
		private bool roundLerpComplete = false;

		public SoundPlayOneshot roundSpawnSound;

		private AllowTeleportWhileAttachedToHand allowTeleport = null;

		public int maxRoundCount = 10;
		private List<GameObject> roundList;


		//-------------------------------------------------
		void Awake()
		{
			allowTeleport = GetComponent<AllowTeleportWhileAttachedToHand>();
			//allowTeleport.teleportAllowed = true;
			allowTeleport.overrideHoverLock = false;

			roundList = new List<GameObject>();
		}


		//-------------------------------------------------
		private void OnAttachedToHand( Hand attachedHand )
		{
			hand = attachedHand;
			FindRifle();
		}


		//-------------------------------------------------
		private GameObject InstantiateRound()
		{
			GameObject round = Instantiate( roundPrefab, roundNockTransform.position, roundNockTransform.rotation ) as GameObject;
			round.name = "Rifle Round";
			round.transform.parent = roundNockTransform;
			Util.ResetTransform( round.transform );

			roundList.Add( round );

			while ( roundList.Count > maxRoundCount )
			{
				GameObject oldRound = roundList[0];
				roundList.RemoveAt( 0 );
				if ( oldRound )
				{
					Destroy( oldRound );
				}
			}

			return round;
		}


		//-------------------------------------------------
		private void HandAttachedUpdate( Hand hand )
		{
			if ( rifle == null )
			{
				FindRifle();
			}

			if ( rifle == null )
			{
				return;
			}

			if ( allowRoundSpawn && ( currentRound == null ) ) // If we're allowed to have an active arrow in hand but don't yet, spawn one
			{
				currentRound = InstantiateRound();
				roundSpawnSound.Play();
			}

			float distanceToNockPosition = Vector3.Distance( transform.parent.position, rifle.nockTransform.position );

			// If there's an arrow spawned in the hand and it's not nocked yet
			if ( !nocked )
			{
				// If we're close enough to nock position that we want to start arrow rotation lerp, do so
				if ( distanceToNockPosition < rotationLerpThreshold )
				{
					float lerp = Util.RemapNumber( distanceToNockPosition, rotationLerpThreshold, lerpCompleteDistance, 0, 1 );

					roundNockTransform.rotation = Quaternion.Lerp( roundNockTransform.parent.rotation, rifle.nockRestTransform.rotation, lerp );
				}
				else // Not close enough for rotation lerp, reset rotation
				{
					roundNockTransform.localRotation = Quaternion.identity;
				}

				// If we're close enough to the nock position that we want to start arrow position lerp, do so
				if ( distanceToNockPosition < positionLerpThreshold )
				{
					float posLerp = Util.RemapNumber( distanceToNockPosition, positionLerpThreshold, lerpCompleteDistance, 0, 1 );

					posLerp = Mathf.Clamp( posLerp, 0f, 1f );

					roundNockTransform.position = Vector3.Lerp( roundNockTransform.parent.position, rifle.nockRestTransform.position, posLerp );
				}
				else // Not close enough for position lerp, reset position
				{
					roundNockTransform.position = roundNockTransform.parent.position;
				}


				// Give a haptic tick when lerp is visually complete
				if ( distanceToNockPosition < lerpCompleteDistance )
				{
					if ( !roundLerpComplete )
					{
						roundLerpComplete = true;
						hand.TriggerHapticPulse( 500 );
					}
				}
				else
				{
					if ( roundLerpComplete )
					{
						roundLerpComplete = false;
					}
				}

				// Allow nocking the arrow when controller is close enough
				if ( distanceToNockPosition < nockDistance )
				{
					if ( !inNockRange )
					{
						inNockRange = true;
						rifle.ArrowInPosition();
					}
				}
				else
				{
					if ( inNockRange )
					{
						inNockRange = false;
					}
				}

                GrabTypes bestGrab = hand.GetBestGrabbingType(GrabTypes.Pinch, true);

                // If round is close enough to the nock position and we're pressing the trigger, and we're not nocked yet, Nock
                if ( ( distanceToNockPosition < nockDistance ) && bestGrab != GrabTypes.None && !nocked )
				{
					if ( currentRound == null )
					{
						currentRound = InstantiateRound();
					}

					nocked = true;
                    nockedWithType = bestGrab;
					rifle.StartNock( this );
					hand.HoverLock( GetComponent<Interactable>() );
					allowTeleport.teleportAllowed = false;
					currentRound.transform.parent = rifle.nockTransform;
					Util.ResetTransform( currentRound.transform );
					Util.ResetTransform( roundNockTransform );
				}
			}


			// If arrow is nocked, and we release the trigger
			if ( nocked && hand.IsGrabbingWithType(nockedWithType) == false )
			{
				if ( rifle.pulled ) // If bow is pulled back far enough, fire arrow, otherwise reset arrow in arrowhand
				{
					FireRound();
				}
				else
				{
					roundNockTransform.rotation = currentRound.transform.rotation;
					currentRound.transform.parent = roundNockTransform;
					Util.ResetTransform( currentRound.transform );
					nocked = false;
                    nockedWithType = GrabTypes.None;
					rifle.ReleaseNock();
					hand.HoverUnlock( GetComponent<Interactable>() );
					allowTeleport.teleportAllowed = true;
				}

				rifle.StartRotationLerp(); // Arrow is releasing from the bow, tell the bow to lerp back to controller rotation
			}
		}


		//-------------------------------------------------
		private void OnDetachedFromHand( Hand hand )
		{
			Destroy( gameObject );
		}


		//-------------------------------------------------
		private void FireRound()
		{
			currentRound.transform.parent = null;

			Round round = currentRound.GetComponent<Round>();
            round.StartRelease();
            round.shaftRB.isKinematic = false;
			round.shaftRB.useGravity = true;
			round.shaftRB.transform.GetComponent<BoxCollider>().enabled = true;

			round.roundHeadRB.isKinematic = false;
			round.roundHeadRB.useGravity = true;
			round.roundHeadRB.transform.GetComponent<BoxCollider>().enabled = true;

			round.roundHeadRB.AddForce( currentRound.transform.forward * rifle.GetRoundVelocity(), ForceMode.VelocityChange );
			round.roundHeadRB.AddTorque( currentRound.transform.forward * 10 );

			round.shaftRB.velocity = round.roundHeadRB.velocity;
			round.shaftRB.angularVelocity = round.roundHeadRB.angularVelocity;

			nocked = false;
            nockedWithType = GrabTypes.None;

			currentRound.GetComponent<Round>().ArrowReleased( rifle.GetRoundVelocity() );
			rifle.RoundReleased();

			allowRoundSpawn = false;
			Invoke( "EnableRoundSpawn", 0.5f );
			StartCoroutine( ArrowReleaseHaptics() );

			currentRound = null;
			allowTeleport.teleportAllowed = true;
		}


		//-------------------------------------------------
		private void EnableArrowSpawn()
		{
			allowRoundSpawn = true;
		}


		//-------------------------------------------------
		private IEnumerator ArrowReleaseHaptics()
		{
			yield return new WaitForSeconds( 0.05f );

			hand.otherHand.TriggerHapticPulse( 1500 );
			yield return new WaitForSeconds( 0.05f );

			hand.otherHand.TriggerHapticPulse( 800 );
			yield return new WaitForSeconds( 0.05f );

			hand.otherHand.TriggerHapticPulse( 500 );
			yield return new WaitForSeconds( 0.05f );

			hand.otherHand.TriggerHapticPulse( 300 );
		}


		//-------------------------------------------------
		private void OnHandFocusLost( Hand hand )
		{
			gameObject.SetActive( false );
		}


		//-------------------------------------------------
		private void OnHandFocusAcquired( Hand hand )
		{
			gameObject.SetActive( true );
		}


		//-------------------------------------------------
		private void FindRifle()
		{
			rifle = hand.otherHand.GetComponentInChildren<Rifle>();
		}
	}
}
