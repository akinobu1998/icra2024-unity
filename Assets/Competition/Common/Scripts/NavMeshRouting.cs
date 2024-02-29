using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SIGVerse.FCSC.Common
{
	public class NavMeshRouting : MonoBehaviour
	{
		private enum WalkState
		{
			Start,
			Walking,
			AdjustDirection,
			GotoNextState,
		}
		
		public const string MobObstacleTag = "MobObstacle";

		public Transform[] points;
		public bool stopAfter1Lap = false;
		public string[] idleTypes;

//		public float animationSpeed = 1.0f;
		public float angularVelForAdjust = 60f;
		public int numberOfIdle = 3;

		// ----------------------------------------
		private const float AnimRatio = 1.0f/100;
		private NavMeshAgent navMeshAgent;
		private Animator animator;

		private int pointIdx = 0;

		private WalkState walkState = WalkState.Start;
		private Quaternion startQua;
		private Quaternion endQua;
		private float angleForAdjust;
		private float turnSpeed = 0.0f;
		private float adjustedAngle = 0.0f;

		private string currentIdleType = string.Empty;

		void Awake()
		{
			this.navMeshAgent = this.GetComponent<NavMeshAgent>();
			this.animator = this.GetComponent<Animator>();
		}

		void Start()
		{

//			this.animator.speed = this.animationSpeed;
			this.navMeshAgent.updatePosition = false;
			this.animator.applyRootMotion = true;
		}

		void Update()
		{
			if (this.navMeshAgent.isStopped) { return; }

			if (this.points.Length == 0) { return; }

			if (this.animator.GetCurrentAnimatorStateInfo(0).IsTag("Walk"))
			{
				switch (this.walkState)
				{
					case WalkState.Start:
						this.navMeshAgent.destination = this.points[this.pointIdx].position;
						this.walkState++;
						break;

					case WalkState.Walking:

						Vector3 worldDeltaPosition = this.navMeshAgent.nextPosition - transform.position;

						float dx = Vector3.Dot(transform.right,   worldDeltaPosition);
						float dy = Vector3.Dot(transform.forward, worldDeltaPosition);

						this.animator.SetFloat("Forward", dy / Time.deltaTime / 2.0f);
						this.animator.SetFloat("Turn", Mathf.Atan2(dx, dy));
						
						if (!this.navMeshAgent.pathPending && this.navMeshAgent.remainingDistance < 0.1f)
						{
							this.adjustedAngle = 0.0f;
							this.startQua = this.transform.rotation;
							this.endQua = Quaternion.LookRotation(this.points[this.pointIdx].forward);
							this.angleForAdjust = Vector3.Angle(transform.forward, this.points[this.pointIdx].forward);
							this.walkState++;
						}

						break;

					case WalkState.AdjustDirection:

						if (this.adjustedAngle == 0.0f)
						{
							this.animator.SetFloat("Forward", 0.0f);

							float signedAngle = Vector3.SignedAngle(transform.forward, this.points[this.pointIdx].forward, Vector3.up);

							this.turnSpeed = (signedAngle > 0) ? +this.angularVelForAdjust * AnimRatio : -this.angularVelForAdjust * AnimRatio;
						}

						this.adjustedAngle = Mathf.Clamp(this.adjustedAngle + this.angularVelForAdjust * Time.deltaTime, 0f, this.angleForAdjust);
						float adjustedRatio = this.adjustedAngle / this.angleForAdjust;

						// Smoothly changing turn speed
						if      (adjustedRatio < 0.1f) { this.animator.SetFloat("Turn", this.turnSpeed * adjustedRatio * 10); }
						else if (adjustedRatio < 0.9f) { this.animator.SetFloat("Turn", this.turnSpeed); }
						else                           { this.animator.SetFloat("Turn", this.turnSpeed * (1-adjustedRatio) * 10); }

						this.transform.rotation = Quaternion.Slerp(this.startQua, this.endQua, adjustedRatio);
						
						if (adjustedRatio > 0.999f)
						{
							this.currentIdleType = idleTypes[Random.Range(0, idleTypes.Length)];

							this.animator.SetFloat("Turn", 0.0f);
							this.animator.SetBool(this.currentIdleType, true);
							this.walkState++;
						}
						break;

					case WalkState.GotoNextState:
						if (!this.animator.GetBool(this.currentIdleType))
						{
							this.pointIdx = (this.pointIdx + 1) % this.points.Length;
							this.currentIdleType = string.Empty;

							this.walkState = WalkState.Start;

							if(pointIdx==0 && this.stopAfter1Lap)
							{
								this.Stop();
							}
						}
						break;
				}
			}
			if(this.currentIdleType!=string.Empty)
			{
				if (this.animator.GetCurrentAnimatorStateInfo(0).IsTag(this.currentIdleType))
				{
					if (this.animator.GetCurrentAnimatorStateInfo(0).normalizedTime > (this.numberOfIdle - 0.5f))
					{
						this.animator.SetBool(this.currentIdleType, false);
					}
				}
			}
		}

		void OnAnimatorMove()
		{
			transform.position = this.navMeshAgent.nextPosition;
		}

		private void Stop()
		{
			this.navMeshAgent.isStopped = true;
			this.animator.SetBool(this.idleTypes[0], true);
		}

		private void Resume()
		{
			this.navMeshAgent.isStopped = false;
			this.animator.SetBool(this.idleTypes[0], false);
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!this.enabled) { return; }

			if (other.tag == MobObstacleTag)
			{
				this.Stop();
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if (!this.enabled) { return; }

			if (other.tag == MobObstacleTag)
			{
				this.Resume();
			}
		}
	}
}

