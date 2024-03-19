using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SIGVerse.Common;
using UnityEngine.UI;
using System.Linq;

namespace SIGVerse.FCSC.Common
{
	public class CameraForUI: MonoBehaviour
	{
		private static string TagUntagged = "Untagged";

		public int textureWidth  = 200;
		public int textureHeight = 200;
		public RawImage uiImage;

		// ----------------------------------------
		private Camera targetCamera;
		private GameObject replicatedObject = null;

		void Awake()
		{
			this.targetCamera = GetComponentInChildren<Camera>();
		}


		public void Capture(GameObject targetObj)
		{
			Debug.Log("Capture for UI. targetObj="+targetObj.name);

			this.targetCamera.targetTexture = null;
			if (this.replicatedObject != null) { Destroy(this.replicatedObject);}

			RenderTexture renderTexture = new RenderTexture(this.textureWidth, this.textureHeight, 0);
			renderTexture.name = this.name;
			renderTexture.Create();

			this.targetCamera.targetTexture = renderTexture;
			this.replicatedObject = Instantiate(targetObj);
			this.replicatedObject.name += "_"+this.name;
			this.replicatedObject.tag = TagUntagged;
			this.replicatedObject.layer = 0;
			this.replicatedObject.transform.parent = this.targetCamera.transform;
			this.replicatedObject.transform.SetLocalPositionAndRotation(new Vector3(0, 0, 1f), Quaternion.Euler(30, 180, 0));
			DisableComponents(this.replicatedObject);

			(Vector3 centerPos, Vector3 boundsExtents) = GetCenterAndBoundsExtents(this.replicatedObject.transform);

			this.replicatedObject.transform.position = this.replicatedObject.transform.position - centerPos;

			this.targetCamera.orthographicSize = Mathf.Max(boundsExtents.x, boundsExtents.y) * 1.2f;

			StartCoroutine(CaptureCoroutine());
		}

		private void DisableComponents(GameObject replicatedObject)
		{
			List<Component> components = replicatedObject.GetComponentsInChildren<Component>().ToList();

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

		private IEnumerator CaptureCoroutine()
		{
			yield return new WaitForEndOfFrame();

			Texture2D texture = new Texture2D(this.textureWidth, this.textureHeight, TextureFormat.RGB24, false);  //Square
			this.targetCamera.Render();
			RenderTexture.active = this.targetCamera.targetTexture;
			texture.ReadPixels(new Rect(0, 0, this.textureWidth, this.textureHeight), 0, 0);  //Square
			texture.Apply();
			this.uiImage.texture = texture;
			RenderTexture.active = null;
		}

		private static (Vector3, Vector3) GetCenterAndBoundsExtents(Transform target)
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
	}
}

