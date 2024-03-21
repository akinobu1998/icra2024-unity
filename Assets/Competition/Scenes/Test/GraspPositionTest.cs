using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using UnityEngine.Animations;
using System.Reflection;
using System;

namespace SIGVerse.FCSC.InteractiveCustomerService
{
	public class GraspPositionTest : MonoBehaviour
	{
		public GameObject robot;
		public Transform graspPosition;
		public Transform itemRoot;

		// ----------------------------------------
		private ConstraintSource constraintSourceGraspPosition;

		private List<Transform> targetItems = new List<Transform>();

		void Awake()
		{
			Rigidbody[] rigidbodies = this.itemRoot.GetComponentsInChildren<Rigidbody>();

			foreach (Rigidbody rigidbody in rigidbodies)
			{
				this.targetItems.Add(rigidbody.gameObject.transform);
			}
		}

		void Start()
		{
			this.constraintSourceGraspPosition = new ConstraintSource();
			this.constraintSourceGraspPosition.sourceTransform = this.graspPosition;
			this.constraintSourceGraspPosition.weight = 1.0f;

			DisableRobotComponents(this.robot);

			StartCoroutine(TakeItems());
		}

		private void DisableRobotComponents(GameObject robot)
		{
			List<Component> components = robot.GetComponentsInChildren<Component>().ToList();

			components.ForEach(component => this.DisableComponent(component));
		}

		protected virtual void DisableComponent(Component component)
		{
			Type type = component.GetType();

			if (type.IsSubclassOf(typeof(Collider)))
			{
				((Collider)component).enabled = false;
			}
			else if (type == typeof(Rigidbody))
			{
				((Rigidbody)component).isKinematic = true;
				((Rigidbody)component).velocity = Vector3.zero;
				((Rigidbody)component).angularVelocity = Vector3.zero;
			}
			else
			{
				if (type.IsSubclassOf(typeof(Behaviour)))
				{
					((Behaviour)component).enabled = false;
				}
			}
		}

		private IEnumerator TakeItems()
		{
			yield return new WaitForSeconds(3.0f);

			Debug.Log("Start");

			int cnt = 0;

			Transform graspedItem;

			foreach(Transform targetItem in this.targetItems)
			{
				graspedItem = targetItem;

				Debug.Log("No:"+(++cnt)+" Item Name="+graspedItem.name);

				TakeItem(graspedItem);

				yield return new WaitForSeconds(0.5f);

				Destroy(graspedItem.GetComponent<ParentConstraint>());
				graspedItem.position = Vector3.zero;
				graspedItem.rotation = Quaternion.identity;
			}

			Debug.Log("End");
		}

		private void TakeItem(Transform targetItem)
		{
			ParentConstraint parentConstraint = targetItem.gameObject.AddComponent<ParentConstraint>();
			parentConstraint.AddSource(this.constraintSourceGraspPosition);
			parentConstraint.constraintActive = true;

			MethodInfo getCenterAndBoundsExtents = typeof(IcsNavMeshRoutingRobot).GetMethod("GetCenterAndBoundsExtents", BindingFlags.NonPublic | BindingFlags.Static);

			// Adust the grasped item
			(Vector3 centerPos, Vector3 boundsExtents) = (ValueTuple<Vector3, Vector3>)(getCenterAndBoundsExtents.Invoke(null, new object[] { targetItem }));

			MethodInfo calcParentConstraintRotationOffset = typeof(IcsNavMeshRoutingRobot).GetMethod("CalcParentConstraintRotationOffset", BindingFlags.NonPublic | BindingFlags.Static);

			Vector3 rotationOffset = (Vector3)(calcParentConstraintRotationOffset.Invoke(null, new object[] { boundsExtents }));
			parentConstraint.SetRotationOffset(0, rotationOffset);
			parentConstraint.SetTranslationOffset(0, Quaternion.Euler(rotationOffset) * (-centerPos));
		}
	}
}
