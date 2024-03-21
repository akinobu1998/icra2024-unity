#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using UnityEngine;
using System.Linq;

namespace SIGVerse.FCSC.Common
{
	public class ScreenShotCapture : MonoBehaviour
	{
		public Camera mainCamera;

		public float cameraPosY = 0.2f;
		public float cameraPosZ = 0.2f;
		public float cameraRotX = 20.0f;
		public float cameraRotY = 180.0f;

		public int outputWidth = 640;
		public int outputHeight = 480;

		private Transform targetParentObject;
		private string mediaOutputFolder;

		private List<Transform> targets = new List<Transform>();

		private RecorderController recorderController;

		private bool isStarted = false;

		void Awake()
		{
			this.targetParentObject = this.transform;
			this.mediaOutputFolder = Path.Combine(Application.dataPath, "..", "GoodsScreenshot");

			foreach (Transform child in this.targetParentObject)
			{
				this.targets.Add(child);
			}
		}

		void Update()
		{
			if (!this.isStarted)
			{
				InitRecorderController();

				StartCoroutine(Capture());

				this.isStarted = true;
			}

//			Debug.Log("Time="+Time.time);
		}

		private IEnumerator Capture()
		{
			Debug.Log("Start Capture");

			foreach(Transform child in this.targets)
			{
				yield return new WaitWhile(()=>this.recorderController.IsRecording());

				foreach (Transform tmpChild in targets)
				{
					tmpChild.gameObject.SetActive(false);
				}

				child.gameObject.SetActive(true);

				this.mainCamera.transform.position = new Vector3(child.transform.position.x, cameraPosY, child.transform.position.z + cameraPosZ);
				this.mainCamera.transform.rotation = Quaternion.Euler(this.cameraRotX, cameraRotY, 0.0f);

				this.recorderController.PrepareRecording();
				this.recorderController.StartRecording();
			}

			Debug.Log("End Capture");
		}

		private void InitRecorderController()
		{
			ImageRecorderSettings imageRecorder = ScriptableObject.CreateInstance<ImageRecorderSettings>();
			imageRecorder.name = "My Image Recorder";
			imageRecorder.Enabled = true;
			imageRecorder.OutputFormat = ImageRecorderSettings.ImageRecorderOutputFormat.PNG;
			imageRecorder.CaptureAlpha = false;

			imageRecorder.OutputFile = Path.Combine(this.mediaOutputFolder, targetParentObject.name+"_") + DefaultWildcard.Take;

			imageRecorder.imageInputSettings = new GameViewInputSettings
			{
				OutputWidth = this.outputWidth,
				OutputHeight = this.outputHeight,
			};

			RecorderControllerSettings controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();

			// Setup Recording
			controllerSettings.AddRecorderSettings(imageRecorder);
			controllerSettings.SetRecordModeToSingleFrame(0);

			this.recorderController = new RecorderController(controllerSettings);
		}

		//void OnGUI()
		//{
		//	if (GUI.Button(new Rect(10, 10, 150, 50), "Capture ScreenShot"))
		//	{
		//		recorderController.PrepareRecording();
		//		recorderController.StartRecording();
		//	}
		//}
	}
}

#endif
