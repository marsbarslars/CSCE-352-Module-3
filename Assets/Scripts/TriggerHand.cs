//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: The object attached to the player's hand that spawns and fires the
//			bullet
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

		private GameObject currentBullet;
		public GameObject bulletPrefab;

		public Transform bulletNockTransform;

		public float nockDistance = 0.1f;
		public float lerpCompleteDistance = 0.08f;
		public float rotationLerpThreshold = 0.15f;
		public float positionLerpThreshold = 0.15f;

		private bool allowBulletSpawn = true;
		private bool nocked;
        private GrabTypes nockedWithType = GrabTypes.None;
		private bool firedBullet = false;

		private bool inNockRange = false;
		private bool bulletLerpComplete = false;

		private AllowTeleportWhileAttachedToHand allowTeleport = null;

		public int maxBulletCount = 10;
		private List<GameObject> bulletList;


		//-------------------------------------------------
		void Awake()
		{
			allowTeleport = GetComponent<AllowTeleportWhileAttachedToHand>();
			//allowTeleport.teleportAllowed = true;
			allowTeleport.overrideHoverLock = false;

			bulletList = new List<GameObject>();
		}


		//-------------------------------------------------
		private void OnAttachedToHand( Hand attachedHand )
		{
			hand = attachedHand;
			FindRifle();
		}


		//-------------------------------------------------
		private GameObject InstantiateBullet()
		{
			GameObject bullet = Instantiate( bulletPrefab, bulletNockTransform.position, bulletNockTransform.rotation ) as GameObject;
			bullet.name = "Rifle Bullet";
			bullet.transform.parent = bulletNockTransform;
//			Util.ResetTransform( bullet.transform );

			bulletList.Add( bullet );

			while ( bulletList.Count > maxBulletCount )
			{
				GameObject oldBullet = bulletList[0];
				bulletList.RemoveAt( 0 );
				if ( oldBullet )
				{
					Destroy( oldBullet );
				}
			}

			return bullet;
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

			float distanceToNockPosition = Vector3.Distance( transform.parent.position, rifle.nockTransform.position );

			// If there's an arrow spawned in the hand and it's not nocked yet
			if ( !nocked )
			{
				// If we're close enough to nock position that we want to start arrow rotation lerp, do so
				if ( distanceToNockPosition < rotationLerpThreshold )
				{
					float lerp = Util.RemapNumber( distanceToNockPosition, rotationLerpThreshold, lerpCompleteDistance, 0, 1 );

					bulletNockTransform.rotation = Quaternion.Lerp( bulletNockTransform.parent.rotation, rifle.nockRestTransform.rotation, lerp );
				}
				else // Not close enough for rotation lerp, reset rotation
				{
					bulletNockTransform.localRotation = Quaternion.identity;
				}

				// If we're close enough to the nock position that we want to start arrow position lerp, do so
				if ( distanceToNockPosition < positionLerpThreshold )
				{
					float posLerp = Util.RemapNumber( distanceToNockPosition, positionLerpThreshold, lerpCompleteDistance, 0, 1 );

					posLerp = Mathf.Clamp( posLerp, 0f, 1f );

					bulletNockTransform.position = Vector3.Lerp( bulletNockTransform.parent.position, rifle.nockRestTransform.position, posLerp );
				}
				else // Not close enough for position lerp, reset position
				{
					bulletNockTransform.position = bulletNockTransform.parent.position;
				}


				// Give a haptic tick when lerp is visually complete
				if ( distanceToNockPosition < lerpCompleteDistance )
				{
					if ( !bulletLerpComplete )
					{
						bulletLerpComplete = true;
						hand.TriggerHapticPulse( 500 );
					}
				}
				else
				{
					if ( bulletLerpComplete )
					{
						bulletLerpComplete = false;
					}
				}

				// Allow nocking the bullet when controller is close enough
				if ( distanceToNockPosition < nockDistance )
				{
					if ( !inNockRange )
					{
						inNockRange = true;
						rifle.TriggerHandInPosition();
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

                // If bullet is close enough to the nock position, and we're not nocked yet, Nock
                if ( ( distanceToNockPosition < nockDistance ) && !nocked )
				{
					if ( currentBullet == null )
					{
						currentBullet = InstantiateBullet();
					}

					nocked = true;
					nockedWithType = bestGrab;
					rifle.StartNock( this );
					hand.HoverLock( GetComponent<Interactable>() );
					allowTeleport.teleportAllowed = false;
					currentBullet.transform.parent = rifle.bulletNockTransform;
					Util.ResetTransform( currentBullet.transform );
					Util.ResetTransform( bulletNockTransform );
				}
			}

            // If bullet is nocked, and we pull the trigger
            if (nocked && (hand.IsGrabbingWithType(nockedWithType) == true) && !firedBullet)
            {
                
                FireBullet();
				firedBullet = true;

            }

            // If bullet is nocked, and we release the trigger
            if ( nocked && hand.IsGrabbingWithType(nockedWithType) == false )
			{
				
				nocked = false;
                nockedWithType = GrabTypes.None;
				rifle.ReleaseNock();
				hand.HoverUnlock( GetComponent<Interactable>() );
				allowTeleport.teleportAllowed = true;
				firedBullet = false;
				

				rifle.StartRotationLerp(); // bullet is releasing from the rifle, tell the rifle to lerp back to controller rotation
			}
		}


		//-------------------------------------------------
		private void OnDetachedFromHand( Hand hand )
		{
			Destroy( gameObject );
		}


		//-------------------------------------------------
		private void FireBullet()
		{
			currentBullet.transform.parent = null;

			Bullet bullet = currentBullet.GetComponent<Bullet>();
            bullet.StartRelease();
            bullet.shaftRB.isKinematic = false;
			bullet.shaftRB.useGravity = true;
			bullet.shaftRB.transform.GetComponent<BoxCollider>().enabled = true;

			bullet.bulletHeadRB.isKinematic = false;
			bullet.bulletHeadRB.useGravity = true;
			bullet.bulletHeadRB.transform.GetComponent<BoxCollider>().enabled = true;

			bullet.bulletHeadRB.AddForce( currentBullet.transform.forward * rifle.GetBulletVelocity(), ForceMode.VelocityChange );
			bullet.bulletHeadRB.AddTorque( currentBullet.transform.forward * 10 );

			bullet.shaftRB.velocity = bullet.bulletHeadRB.velocity;
			bullet.shaftRB.angularVelocity = bullet.bulletHeadRB.angularVelocity;

			nocked = false;
            nockedWithType = GrabTypes.None;

			currentBullet.GetComponent<Bullet>().BulletReleased( rifle.GetBulletVelocity() );
			rifle.BulletReleased();

			allowBulletSpawn = false;
			Invoke( "EnableBulletSpawn", 0.5f );
			StartCoroutine( ArrowReleaseHaptics() );

			currentBullet = null;
			allowTeleport.teleportAllowed = true;
		}


		//-------------------------------------------------
		private void EnableArrowSpawn()
		{
			allowBulletSpawn = true;
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
