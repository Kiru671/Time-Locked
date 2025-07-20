using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CMF
{
	//This script controls the character's animation by passing velocity values and other information ('isGrounded') to an animator component;
	public class AnimationControl : MonoBehaviour {

		Controller controller;
		public List<Animator> animators;
		Transform animatorTransform;
		Transform tr;
		private int moveType = 0;

		//Whether the character is using the strafing blend tree;
		public bool useStrafeAnimations = false;

		//Velocity threshold for landing animation;
		//Animation will only be triggered if downward velocity exceeds this threshold;
		public float landVelocityThreshold = 5f;

		private float smoothingFactor = 40f;
		Vector3 oldMovementVelocity = Vector3.zero;

		//Setup;
		void Awake () {
			controller = GetComponent<Controller>();
			animators = new List<Animator>();
			animators.Add(GetComponentInChildren<Animator>());
			animatorTransform = animators[0].transform;

			tr = transform;
			StartCoroutine(SwapMoveType());
		}

		
		//OnEnable;
		void OnEnable()
		{
			//Connect events to controller events;
			controller.OnLand += OnLand;
			controller.OnJump += OnJump;
		}

		public void SwapState(string stateName)
		{
			animators[0].SetTrigger("ChangeState");
			animators[0].SetBool("Bike", false);
			animators[0].SetBool("Pedestrian", false);
			animators[0].SetBool(stateName, true);
		}
		
		//OnDisable;
		void OnDisable()
		{
			//Disconnect events to prevent calls to disabled gameobjects;
			controller.OnLand -= OnLand;
			controller.OnJump -= OnJump;
		}

		IEnumerator SwapMoveType()
		{
			while (true)
			{
				yield return new WaitForSeconds(10);
				moveType = moveType == 0 ? 1 : 0;

				foreach (Animator animator in animators)
				{
					animator.SetInteger("MoveType", moveType);
				}
			}
		}
		
		//Update;
		void Update () {

			//Get controller velocity;
			Vector3 _velocity = controller.GetVelocity();

			//Split up velocity;
			Vector3 _horizontalVelocity = VectorMath.RemoveDotVector(_velocity, tr.up);
			Vector3 _verticalVelocity = _velocity - _horizontalVelocity;

			//Smooth horizontal velocity for fluid animation;
			_horizontalVelocity = Vector3.Lerp(oldMovementVelocity, _horizontalVelocity, smoothingFactor * Time.deltaTime);
			oldMovementVelocity = _horizontalVelocity;

			foreach (Animator animator in animators)
			{
				animator.SetFloat("VerticalSpeed", _verticalVelocity.magnitude * VectorMath.GetDotProduct(_verticalVelocity.normalized, tr.up));
				animator.SetFloat("HorizontalSpeed", _horizontalVelocity.magnitude);

				//If animator is strafing, split up horizontal velocity;
				if(useStrafeAnimations)
				{
					Vector3 _localVelocity = animatorTransform.InverseTransformVector(_horizontalVelocity);
					animator.SetFloat("ForwardSpeed", _localVelocity.z);
					animator.SetFloat("StrafeSpeed", _localVelocity.x);
				}

				//Pass values to animator;
				animator.SetBool("IsGrounded", controller.IsGrounded());
				animator.SetBool("IsStrafing", useStrafeAnimations);
			}
			
		}

		void OnLand(Vector3 _v)
		{
			//Only trigger animation if downward velocity exceeds threshold;
			if(VectorMath.GetDotProduct(_v, tr.up) > -landVelocityThreshold)
				return;

			foreach (Animator animator in animators)
				animator.SetTrigger("OnLand");
		}

		void OnJump(Vector3 _v)
		{
			
		}
	}
}
