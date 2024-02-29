using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SIGVerse.Common;
using UnityEngine.UI;
using System.Linq;
using Unity.Netcode.Components;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SIGVerse.FCSC.Common
{
	public class ObjectManager : MonoBehaviour
	{
		public GameObject[] rootsOfSyncTarget;

		//public float interactableThrowSmoothingDuration = 0.15f;
		//public float interactableThrowVelocityScale = 1.25f;

		[HeaderAttribute("Auto Assignment")]
		public List<GameObject> roomObjects;

		// -----------------------

		void Start()
		{
			// Check for duplication
			List<string> duplicatePathList = roomObjects.GroupBy(obj => SIGVerseUtils.GetHierarchyPath(obj.transform)).Where(g => g.Count() > 1).Select(g => g.Key).ToList();

			if (duplicatePathList.Count > 0)
			{
				throw new Exception("There are multiple objects with the same path. e.g. " + duplicatePathList[0]);
			}

			// Manage the synchronized room objects using singleton
			RoomObjectManager.Instance.roomObjects = roomObjects;
		}

		public void SetRoomObjects(List<GameObject> roomObjects)
		{
			this.roomObjects = roomObjects;
		}
	}


#if UNITY_EDITOR
	[CustomEditor(typeof(ObjectManager))]
	public class ObjectManagerEditor : Editor
	{
		//public const string TagGraspable = "Graspable";

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			GUILayout.Space(10);

			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.FlexibleSpace();

				if (GUILayout.Button("Update Scripts of Goods", GUILayout.Width(200), GUILayout.Height(40)))
				{
					Undo.RecordObject(target, "Update Scripts of Goods");

					UpdateObjectList();
				}

				GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(10);
		}

		public void UpdateObjectList()
		{
			ObjectManager objectManager = (ObjectManager)target;

			//RemoveScripts<NetworkRigidbody>();
			//RemoveScripts<ClientNetworkTransform>();
			//RemoveScripts<NetworkTransform>();
			//RemoveScripts<NetworkObject>();
			//RemoveScripts<OwnerChanger>();
			//RemoveScripts<ThrowableWithoutSuctionNgo>();
			//RemoveScripts<XRGrabInteractable>();
			//RemoveScripts<XRGrabInteractableNoParentChange>();

			// Add scripts
			List<GameObject> roomObjects = new List<GameObject>();

			foreach (GameObject sourceOfSyncTarget in objectManager.rootsOfSyncTarget)
			{
				Rigidbody[] syncTargetRigidbodies = sourceOfSyncTarget.GetComponentsInChildren<Rigidbody>();

				foreach (Rigidbody syncTargetRigidbody in syncTargetRigidbodies)
				{
					roomObjects.Add(syncTargetRigidbody.gameObject);
				}
			}

			objectManager.SetRoomObjects(roomObjects);

			foreach (GameObject roomObject in roomObjects)
			{
				RemoveScripts<NetworkRigidbody>(roomObject);

				//NetworkObject networkObject = Undo.AddComponent<NetworkObject>(roomObject);
				//networkObject.DontDestroyWithOwner = true;
				//ClientNetworkTransform clientNetworkTransform = Undo.AddComponent<ClientNetworkTransform>(roomObject);
				//clientNetworkTransform.SyncScaleX = false;
				//clientNetworkTransform.SyncScaleY = false;
				//clientNetworkTransform.SyncScaleZ = false;
				//clientNetworkTransform.InLocalSpace = true;
				//Undo.AddComponent<NetworkRigidbody>(roomObject);
				//Undo.AddComponent<OwnerChanger>(roomObject);

				//if (roomObject.tag == TagGraspable)
				//{
				//	XRGrabInteractableNoParentChange interactable = Undo.AddComponent<XRGrabInteractableNoParentChange>(roomObject);
				//	interactable.movementType = XRBaseInteractable.MovementType.VelocityTracking;
				//	interactable.retainTransformParent = false;
				//	interactable.throwSmoothingDuration = objectManager.interactableThrowSmoothingDuration;
				//	interactable.throwVelocityScale = objectManager.interactableThrowVelocityScale;
				//	interactable.useDynamicAttach = true;
				//	interactable.snapToColliderVolume = false;
				//}
			}

			Debug.Log("Object update has been completed.");
		}

		private void RemoveScripts<T>() where T : Component
		{
			ObjectManager objectManager = (ObjectManager)target;

			foreach (GameObject sourceOfSyncTarget in objectManager.rootsOfSyncTarget)
			{
				RemoveScripts<T>(sourceOfSyncTarget);
			}
		}

		private void RemoveScripts<T>(GameObject roomObject) where T : Component
		{
			T[] scripts = roomObject.GetComponentsInChildren<T>();

			foreach (T script in scripts)
			{
				Undo.DestroyObjectImmediate(script);
			}
		}
	}
#endif
}

