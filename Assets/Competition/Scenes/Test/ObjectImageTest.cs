using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using UnityEngine.Animations;
using System;
using SIGVerse.FCSC.Common;

namespace SIGVerse.FCSC.InteractiveCustomerService
{
	public class ObjectImageTest : MonoBehaviour
	{
		public Transform itemRoot;
		public CameraForUI cameraForTargetObj;

		// ----------------------------------------
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
			StartCoroutine(DisplayImages());
		}

		private IEnumerator DisplayImages()
		{
			yield return new WaitForSeconds(3.0f);

			Debug.Log("Start");

			int cnt = 0;

			foreach(Transform targetItem in this.targetItems)
			{
				Debug.Log("No:"+(++cnt)+" Item Name="+targetItem.name);

				this.cameraForTargetObj.Capture(targetItem.gameObject);

				yield return new WaitForSeconds(0.5f);
			}

			Debug.Log("End");
		}
	}
}
