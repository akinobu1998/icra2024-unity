using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;
using UnityEngine.Scripting;

namespace SIGVerse.FCSC.InteractiveCustomerService
{
	public class IcsNavMeshRoutingRobot : MonoBehaviour
	{
		private enum WalkState
		{
			Standby,
			TurnTowardDestination,
			GoToDestination,
			TurnTowardItem,
			TakeItemGo,
			TakeItemBack,
			TurnTowardInitialPos,
			BackToInitialPos,
			TurnTowardCustomer,
		}

		private const float TopShelfHeight = 1.0f;
		private const float MiddleShelfHeight = 0.5f;
//		private const float BottomShelfHeight = 0.0f;

		private const float AdjustAngle = 90.0f;

		public Transform graspPosition;
		public Transform navMeshLink;

		public float angularVel = 60f;

		// ----------------------------------------
		private ConstraintSource constraintSourceGraspPosition;

		private NavMeshAgent navMeshAgent;
		private IcsRobotArmController armController;

		private WalkState walkState = WalkState.Standby;
		private Transform targetItem = null;
		private Vector3    initPos;
		private Quaternion initRot;
		private Quaternion startQua;
		private Quaternion endQua;
		private float adjustedAngle = 0.0f;

		private Transform  graspedItem = null;
		private Vector3    graspedItemIniPos;
		private Quaternion graspedItemIniRot;

		void Awake()
		{
			this.navMeshAgent = this.navMeshLink.GetComponent<NavMeshAgent>();
			this.armController = this.GetComponent<IcsRobotArmController>();
		}

		void Start()
		{
			this.constraintSourceGraspPosition = new ConstraintSource();
			this.constraintSourceGraspPosition.sourceTransform = this.graspPosition;
			this.constraintSourceGraspPosition.weight = 1.0f;
		}

		void Update()
		{
			switch (this.walkState)
			{
				case WalkState.Standby:

					if(this.targetItem!=null)
					{
						this.initPos = this.navMeshLink.position;
						this.initRot = this.navMeshLink.rotation;

						Vector3 turnTarget = new Vector3(this.initPos.x, this.initPos.y, this.targetItem.position.z);

						ResetTurnParam(Quaternion.LookRotation(turnTarget - this.navMeshLink.position));
						this.walkState++;
					}
					break;

				case WalkState.TurnTowardDestination:
						
					if (IsTurned())
					{
						this.navMeshAgent.destination = new Vector3(this.initPos.x, this.initPos.y, this.targetItem.position.z);
						this.walkState++;
					}
					break;

				case WalkState.GoToDestination:
						
					if (!this.navMeshAgent.pathPending && this.navMeshAgent.remainingDistance < 0.01f)
					{
						Vector3 turnTarget = new Vector3(this.targetItem.position.x, this.initPos.y, this.targetItem.position.z);

						ResetTurnParam(Quaternion.LookRotation(turnTarget - this.navMeshLink.position));
						this.walkState++;
					}
					break;

				case WalkState.TurnTowardItem:

					if (IsTurned())
					{
						if(this.targetItem.position.y > TopShelfHeight)
						{
							this.armController.UpArmHigh();
						}
						else if(this.targetItem.position.y > MiddleShelfHeight)
						{
							this.armController.UpArm();
						}
						else
						{
							this.armController.DownArm();
						}
						this.walkState++;
					}
					break;

				case WalkState.TakeItemGo:

					if(this.armController.IsReachedDestination())
					{
						if(this.graspedItem!=null)
						{
							Destroy(this.graspedItem.GetComponent<ParentConstraint>());
							this.graspedItem.position = this.graspedItemIniPos;
							this.graspedItem.rotation = this.graspedItemIniRot;
						}

						//Debug.Log("TakeItemGo Finished");
						DisableCollision(this.targetItem);

						this.graspedItem       = this.targetItem;
						this.graspedItemIniPos = this.targetItem.position;
						this.graspedItemIniRot = this.targetItem.rotation;

						ParentConstraint parentConstraint = this.targetItem.gameObject.AddComponent<ParentConstraint>();
						parentConstraint.AddSource(this.constraintSourceGraspPosition);
						parentConstraint.constraintActive = true;

						// Adust the grasped item
						(Vector3 centerPos, Vector3 boundsExtents) = GetCenterAndBoundsExtents(this.graspedItem);

						Vector3 rotationOffset = CalcParentConstraintRotationOffset(boundsExtents);
						parentConstraint.SetRotationOffset(0, rotationOffset);
						parentConstraint.SetTranslationOffset(0, Quaternion.Euler(rotationOffset)*(-centerPos));

						this.armController.ResetArm();
						this.walkState++;
					}
					break;

				case WalkState.TakeItemBack:

					if(this.armController.IsReachedDestination())
					{
						ResetTurnParam(Quaternion.LookRotation(this.initPos - this.navMeshLink.position));
						this.walkState++;
					}
					break;

				case WalkState.TurnTowardInitialPos:

					if (IsTurned())
					{
						this.navMeshAgent.destination = this.initPos;
						this.walkState++;
					}
					break;

				case WalkState.BackToInitialPos:
						
					if (!this.navMeshAgent.pathPending && this.navMeshAgent.remainingDistance < 0.01f)
					{
						ResetTurnParam(this.initRot);
						this.walkState++;
					}
					break;

				case WalkState.TurnTowardCustomer:

					if (IsTurned())
					{
						//Debug.LogWarning("Reached!!!");

						this.targetItem = null;

						this.walkState = WalkState.Standby;
					}
					break;
			}
		}

		private static (Vector3 centerPos, Vector3 boundsExtents) GetCenterAndBoundsExtents(Transform target)
		{
			Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);

			if (renderers.Length == 0) { throw new Exception("No renderers"); }

			Vector3 minPos = Vector3.positiveInfinity;
			Vector3 maxPos = Vector3.negativeInfinity;

			for (int i = 0;i < renderers.Length;i++)
			{
				Vector3 center  = renderers[i].localBounds.center;
				Vector3 extents = renderers[i].localBounds.extents;

				if (minPos.x > center.x - extents.x) { minPos.x = center.x - extents.x; }
				if (minPos.y > center.y - extents.y) { minPos.y = center.y - extents.y; }
				if (minPos.z > center.z - extents.z) { minPos.z = center.z - extents.z; }
				if (maxPos.x < center.x + extents.x) { maxPos.x = center.x + extents.x; }
				if (maxPos.y < center.y + extents.y) { maxPos.y = center.y + extents.y; }
				if (maxPos.z < center.z + extents.z) { maxPos.z = center.z + extents.z; }
			}

			return ((minPos + maxPos) / 2, (maxPos - minPos) / 2);
		}

		private static Vector3 CalcParentConstraintRotationOffset(Vector3 boundsExtents)
		{
			string orderStr = GetBoundsExtentsOrderString(boundsExtents);

			Debug.Log("OrderString = "+orderStr);

			if (orderStr == "xyz") { return new Vector3( 0, 90, 90); }
			if (orderStr == "xzy") { return new Vector3( 0,  0, 90); }
			if (orderStr == "yxz") { return new Vector3( 0, 90,  0); }
			if (orderStr == "yzx") { return new Vector3( 0,  0,  0); }
			if (orderStr == "zxy") { return new Vector3(90, 90,  0); }
			if (orderStr == "zyx") { return new Vector3(90,  0,  0); }

			throw new Exception("Invalid OrderString = "+orderStr);
		}

		private static string GetBoundsExtentsOrderString(Vector3 boundsExtents)
		{
			float x = boundsExtents.x;
			float y = boundsExtents.y * 1.05f; // Prioritize the y direction.
			float z = boundsExtents.z;

			Debug.Log("BoundsExtents = "+boundsExtents);

			if (x > y)
			{
				if (x > z){ return y > z ? "xyz" : "xzy"; }
				else      { return "zxy"; }
			}
			else
			{
				if (x > z){ return "yxz"; }
				else      { return y > z ? "yzx" : "zyx"; }
			}
		}

		private void ResetTurnParam(Quaternion endQuaternion)
		{
			this.adjustedAngle = 0.0f;
			this.startQua = this.navMeshLink.rotation;
			this.endQua = endQuaternion;
		}

		private bool IsTurned()
		{
			this.adjustedAngle = Mathf.Clamp(this.adjustedAngle + this.angularVel * Time.deltaTime, 0f, AdjustAngle);

			float adjustedRatio = this.adjustedAngle / AdjustAngle;

			this.navMeshLink.rotation = Quaternion.Slerp(this.startQua, this.endQua, adjustedRatio);

			return adjustedRatio > 0.999f;
		}

		private void DisableCollision(Transform item)
		{
			item.GetComponent<Rigidbody>().isKinematic = true;
			List<Collider> colliders = item.GetComponentsInChildren<Collider>().ToList();

			foreach (Collider collider in colliders)
			{
				collider.enabled = false;
			}
		}

		public void SetItem(Transform item)
		{
			this.targetItem = item;
		}

		public bool IsItemTaken()
		{
			return this.walkState >= WalkState.TakeItemBack;
		}

		public bool IsReturned()
		{
			return this.targetItem==null;
		}
	}
}

