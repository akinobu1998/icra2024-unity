using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace SIGVerse.FCSC.InteractiveCustomerService
{
	public class ThumbnailCreator: MonoBehaviour
	{
		public Camera targetCamera;
		public GameObject targetObjectsRoot;
		public string folderName = "GoodsScreenshot";
		public int textureWidth  = 800;
		public int textureHeight = 800;

		// ----------------------------------------

		private GameObject replicatedObject = null;

		void Start()
		{
			this.targetCamera.aspect = 1f;

			DirectoryInfo directoryInfo = new DirectoryInfo(Application.dataPath + "/../" + folderName);

			if (!directoryInfo.Exists){ Debug.LogError("No Folder! name="+directoryInfo.FullName); return; }

			StartCoroutine(Capture());
		}

		public IEnumerator Capture()
		{
			Rigidbody[] Rigidboies = this.targetObjectsRoot.GetComponentsInChildren<Rigidbody>();

			foreach (Rigidbody rb in Rigidboies)
			{
				Debug.Log("Capture Start! Name="+rb.name);
			
				GameObject targetObj = rb.gameObject;

				this.targetCamera.targetTexture = null;
				if (this.replicatedObject != null) { Destroy(this.replicatedObject);}

				RenderTexture renderTexture = new RenderTexture(this.textureWidth, this.textureHeight, 0);
				renderTexture.name = this.name;
				renderTexture.Create();

				this.targetCamera.targetTexture = renderTexture;
				this.replicatedObject = Instantiate(targetObj);
				this.replicatedObject.transform.parent = this.targetCamera.transform;
				this.replicatedObject.transform.SetLocalPositionAndRotation(new Vector3(0, 0, 1f), Quaternion.Euler(30, 180, 0));
				replicatedObject.GetComponentInChildren<Rigidbody>().isKinematic = true;

				(Vector3 centerPos, Vector3 boundsExtents) = GetCenterAndBoundsExtents(this.replicatedObject.transform);

				this.replicatedObject.transform.position = this.replicatedObject.transform.position - centerPos;

				this.targetCamera.orthographicSize = Mathf.Max(boundsExtents.x, boundsExtents.y) * 1.3f;

				yield return StartCoroutine(CaptureCoroutine(targetObj.name));
			}

			Debug.Log("Finished");
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

		private IEnumerator CaptureCoroutine(string targetObjName)
		{
			yield return new WaitForEndOfFrame();

			Texture2D texture = new Texture2D(this.textureWidth, this.textureHeight, TextureFormat.RGB24, false);  //Square

			this.targetCamera.Render();
			RenderTexture.active = this.targetCamera.targetTexture;
			texture.ReadPixels(new Rect(0, 0, this.textureWidth, this.textureHeight), 0, 0);  //Square
			texture.Apply();

			var png = texture.EncodeToPNG();
			string destFilePath = Application.dataPath + "/../" + folderName + "/" + targetObjName + ".png";
			File.WriteAllBytes(destFilePath, png);

			RenderTexture.active = null;

			yield return null;
		}
	}
}

