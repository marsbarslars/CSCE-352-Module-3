//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: The rifle
//
//=============================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Valve.VR.InteractionSystem
{
	//-------------------------------------------------------------------------
	[RequireComponent( typeof( Interactable ) )]
	public class Rifle : MonoBehaviour
	{
		public enum Handedness { Left, Right };

		public Handedness currentHandGuess = Handedness.Left;
		private float timeOfPossibleHandSwitch = 0f;
		private float timeBeforeConfirmingHandSwitch = 1.5f;
		private bool possibleHandSwitch = false;

		public Transform pivotTransform;
		public Transform handleTransform;

		private Hand hand;
		private TriggerHand triggerHand;

		public Transform nockTransform;
		public Transform nockRestTransform;
		public Transform bulletNockTransform;

		public bool autoSpawnTriggerHand = true;
		public ItemPackage triggerHandItemPackage;
		public GameObject triggerHandPrefab;

		public bool nocked;
		public bool pulled;

		private const float minPull = 0.05f;
		private const float maxPull = 0.5f;
		private float nockDistanceTravelled = 0f;
		private float hapticDistanceThreshold = 0.01f;
		private float lastTickDistance;
		private Vector3 rifleLeftVector;

		private float bulletVelocity = 120f;

		private float minStrainTickTime = 0.1f;
		private float maxStrainTickTime = 0.5f;
		private float nextStrainTick = 0;

		private bool lerpBackToZeroRotation;
		private float lerpStartTime;
		private float lerpDuration = 0.15f;
		private Quaternion lerpStartRotation;

		private float nockLerpStartTime;

		private Quaternion nockLerpStartRotation;

		public float drawOffset = 0.06f;

		public LinearMapping rifleLinearMapping;

		private Vector3 lateUpdatePos;
		private Quaternion lateUpdateRot;

		private float drawTension;
		public SoundPlayOneshot releaseSound;

		SteamVR_Events.Action newPosesAppliedAction;


		//-------------------------------------------------
		private void OnAttachedToHand( Hand attachedHand )
		{
			hand = attachedHand;
		}


		//-------------------------------------------------
		private void HandAttachedUpdate( Hand hand )
		{
			// Reset transform since we cheated it right after getting poses on previous frame
			// transform.localPosition = Vector3.zero;
			// transform.localRotation = Quaternion.identity;

			// Update handedness guess
			EvaluateHandedness();

			if ( nocked )
			{

				// Align bow
				// Time lerp value used for ramping into drawn bow orientation
				float lerp = Util.RemapNumberClamped( Time.time, nockLerpStartTime, ( nockLerpStartTime + lerpDuration ), 0f, 1f );

				Vector3 bulletNockTransformToHeadset = ( ( Player.instance.hmdTransform.position + ( Vector3.down * 0.05f ) ) - triggerHand.bulletNockTransform.parent.position ).normalized;
				Vector3 triggerHandPosition = ( triggerHand.bulletNockTransform.parent.position + ( ( bulletNockTransformToHeadset * drawOffset ) ) ); // Use this line to lerp arrowHand nock position
				//Vector3 arrowHandPosition = arrowHand.arrowNockTransform.position; // Use this line if we don't want to lerp arrowHand nock position

				Vector3 pivotToString = ( triggerHandPosition - pivotTransform.position ).normalized;
				Vector3 pivotToLowerHandle = ( handleTransform.position - pivotTransform.position ).normalized;
				rifleLeftVector = -Vector3.Cross( pivotToLowerHandle, pivotToString );
				pivotTransform.rotation = Quaternion.Lerp( nockLerpStartRotation, Quaternion.LookRotation( pivotToString, rifleLeftVector ), lerp );

			}
			else
			{
				if ( lerpBackToZeroRotation )
				{
					float lerp = Util.RemapNumber( Time.time, lerpStartTime, lerpStartTime + lerpDuration, 0, 1 );

					pivotTransform.localRotation = Quaternion.Lerp( lerpStartRotation, Quaternion.identity, lerp );

					if ( lerp >= 1 )
					{
						lerpBackToZeroRotation = false;
					}
				}
			}
		}


		//-------------------------------------------------
		public void BulletReleased()
		{
			nocked = false;
			hand.HoverUnlock( GetComponent<Interactable>() );
			hand.otherHand.HoverUnlock( triggerHand.GetComponent<Interactable>() );

			if ( releaseSound != null )
			{
				releaseSound.Play();
			}

			this.StartCoroutine( this.ResetDrawAnim() );
		}


		//-------------------------------------------------
		private IEnumerator ResetDrawAnim()
		{
			float startTime = Time.time;
			float startLerp = drawTension;

			while ( Time.time < ( startTime + 0.02f ) )
			{
				float lerp = Util.RemapNumberClamped( Time.time, startTime, startTime + 0.02f, startLerp, 0f );
				this.rifleLinearMapping.value = lerp;
				yield return null;
			}

			this.rifleLinearMapping.value = 0;

			yield break;
		}


		//-------------------------------------------------
		public float GetBulletVelocity()
		{
			return bulletVelocity;
		}


		//-------------------------------------------------
		public void StartRotationLerp()
		{
			lerpStartTime = Time.time;
			lerpBackToZeroRotation = true;
			lerpStartRotation = pivotTransform.localRotation;

			Util.ResetTransform( nockTransform );
		}


		//-------------------------------------------------
		public void StartNock( TriggerHand currentTriggerHand )
		{
			triggerHand = currentTriggerHand;
			hand.HoverLock( GetComponent<Interactable>() );
			nocked = true;

			// Decide which hand we're drawing with and lerp to the correct side
			DoHandednessCheck();
		}


		//-------------------------------------------------
		private void EvaluateHandedness()
		{
            var handType = hand.handType;

			if ( handType == SteamVR_Input_Sources.LeftHand )// Rifle hand is further left than trigger hand.
			{
				// We were considering a switch, but the current controller orientation matches our currently assigned handedness, so no longer consider a switch
				if ( possibleHandSwitch && currentHandGuess == Handedness.Left )
				{
					possibleHandSwitch = false;
				}

				// If we previously thought the rifle was right-handed, and were not already considering switching, start considering a switch
				if ( !possibleHandSwitch && currentHandGuess == Handedness.Right )
				{
					possibleHandSwitch = true;
					timeOfPossibleHandSwitch = Time.time;
				}

				// If we are considering a handedness switch, and it's been this way long enough, switch
				if ( possibleHandSwitch && Time.time > ( timeOfPossibleHandSwitch + timeBeforeConfirmingHandSwitch ) )
				{
					currentHandGuess = Handedness.Left;
					possibleHandSwitch = false;
				}
			}
			else // Rifle hand is further right than trigger hand
			{
				// We were considering a switch, but the current controller orientation matches our currently assigned handedness, so no longer consider a switch
				if ( possibleHandSwitch && currentHandGuess == Handedness.Right )
				{
					possibleHandSwitch = false;
				}

				// If we previously thought the rifle was right-handed, and were not already considering switching, start considering a switch
				if ( !possibleHandSwitch && currentHandGuess == Handedness.Left )
				{
					possibleHandSwitch = true;
					timeOfPossibleHandSwitch = Time.time;
				}

				// If we are considering a handedness switch, and it's been this way long enough, switch
				if ( possibleHandSwitch && Time.time > ( timeOfPossibleHandSwitch + timeBeforeConfirmingHandSwitch ) )
				{
					currentHandGuess = Handedness.Right;
					possibleHandSwitch = false;
				}
			}
		}


		//-------------------------------------------------
		private void DoHandednessCheck()
		{
			// Based on our current best guess about hand, switch rifle orientation and arrow lerp direction
			if ( currentHandGuess == Handedness.Left )
			{
				pivotTransform.localScale = new Vector3( 1f, 1f, 1f );
			}
			else
			{
				pivotTransform.localScale = new Vector3( 1f, -1f, 1f );
			}
		}


		//-------------------------------------------------
		public void TriggerHandInPosition()
		{
			DoHandednessCheck();

		}


		//-------------------------------------------------
		public void ReleaseNock()
		{
			// TriggerHand tells us to do this when we release the buttons when rifle is nocked but not drawn far enough
			nocked = false;
			hand.HoverUnlock( GetComponent<Interactable>() );
			this.StartCoroutine( this.ResetDrawAnim() );
		}


		//-------------------------------------------------
		private void ShutDown()
		{
			if ( hand != null && hand.otherHand.currentAttachedObject != null )
			{
				if ( hand.otherHand.currentAttachedObject.GetComponent<ItemPackageReference>() != null )
				{
					if ( hand.otherHand.currentAttachedObject.GetComponent<ItemPackageReference>().itemPackage == triggerHandItemPackage )
					{
						hand.otherHand.DetachObject( hand.otherHand.currentAttachedObject );
					}
				}
			}
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
			OnAttachedToHand( hand );
		}


		//-------------------------------------------------
		private void OnDetachedFromHand( Hand hand )
		{
			Destroy( gameObject );
		}


		//-------------------------------------------------
		void OnDestroy()
		{
			ShutDown();
		}
	}
}
