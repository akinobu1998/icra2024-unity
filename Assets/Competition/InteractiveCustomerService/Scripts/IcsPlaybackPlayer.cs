using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using SIGVerse.FCSC.Common;
using SIGVerse.Common.Recorder;

namespace SIGVerse.FCSC.InteractiveCustomerService
{
	[RequireComponent(typeof (IcsPlaybackCommon))]
	public class IcsPlaybackPlayer : CompetitionPlaybackPlayer
	{
		[HeaderAttribute("Interactive Customer Service Objects")]
		public IcsScoreManager scoreManager;

		//---------------------------------------

		protected override void Awake()
		{
			if(IcsConfig.Instance.info.playbackType != IcsPlaybackCommon.PlaybackTypePlay)
			{
				DisablePlayer();
			}

			base.Awake();

			if (this.willPlay)
			{
				Transform robot = GameObject.FindGameObjectWithTag("Robot").transform;

//				robot.Find("CompetitionScripts").gameObject.SetActive(false);
				robot.Find("RosBridgeScripts")  .gameObject.SetActive(false);

				Rigidbody[] robotRigidbodies = robot.GetComponentsInChildren<Rigidbody>(true);
				foreach(Rigidbody rigidbody in robotRigidbodies) { rigidbody.isKinematic = true; }


				Transform moderator = GameObject.FindGameObjectWithTag("Moderator").transform;

				moderator.GetComponent<IcsModerator>() .enabled = false;
				moderator.GetComponent<IcsPubConversation>().enabled = false;
				moderator.GetComponent<IcsSubConversation>().enabled = false;

				Rigidbody[] moderatorRigidbodies = moderator.GetComponentsInChildren<Rigidbody>(true);
				foreach(Rigidbody rigidbody in moderatorRigidbodies) { rigidbody.isKinematic = true; }


				this.scoreManager.enabled = false;

				foreach(GameObject graspingCandidatePosition in GameObject.FindGameObjectsWithTag("GraspingCandidatesPosition"))
				{
					graspingCandidatePosition.SetActive(false);
				}

				this.timeLimit = IcsConfig.Instance.info.sessionTimeLimit;
			}
		}


		// Use this for initialization
		//protected override void Start()
		//{
		//	base.Start();

		//	IcsPlaybackCommon common = this.GetComponent<IcsPlaybackCommon>();
		//}

		//protected override void ReadData(string[] headerArray, string dataStr)
		//{
		//	base.ReadData(headerArray, dataStr);

		//	this.environmentController.ReadEvents(headerArray, dataStr); // Environment
		//}

		//protected override void StartInitializing()
		//{
		//	base.StartInitializing();

		//	this.environmentController.StartInitializingEvents(); // Environment
		//}

		public override void OnReadFileButtonClick()
		{
			this.trialNo = int.Parse(this.trialNoInputField.text);

			string filePath = string.Format(Application.dataPath + IcsPlaybackCommon.FilePathFormat, this.trialNo);

			this.Initialize(filePath);

			this.StartCoroutine(this.ActivateEnvironment());
		}

		private IEnumerator ActivateEnvironment()
		{
			float startTime = Time.time;

			while(this.step != Step.Waiting && (Time.time - startTime) < 30.0f) // Wait at most 30 seconds
			{
				yield return null;
			}

//			Debug.Log("reading file time="+(Time.time - startTime));

			base.transformController.ExecuteFirstEvent();  // Initialize transforms
		}
	}
}

