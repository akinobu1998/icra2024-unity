using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using SIGVerse.Common;
using SIGVerse.ToyotaHSR;
using UnityEngine.EventSystems;
using SIGVerse.FCSC.Common;

namespace SIGVerse.FCSC.InteractiveCustomerService
{
	public static class Score
	{
		public const int MaxScore = +999;
		public const int MinScore = -999;

		public enum Type
		{
			RobotSpeech,
			PlacementSuccess,
			PlacementFailure,
		}

		public static int GetScore(Type scoreType, params object[] args)
		{
			switch(scoreType)
			{
				case Score.Type.RobotSpeech      : { return GetSpeechCountDeduction((ushort)args[0]); }
				case Score.Type.PlacementSuccess : { return +10 + GetSpeechCountBonus((ushort)args[0]); }
				case Score.Type.PlacementFailure : { return -10; }
			}

			throw new Exception("Illegal score type. Type = " + (int)scoreType + ", method name=(" + System.Reflection.MethodBase.GetCurrentMethod().Name + ")");
		}

		private static int GetSpeechCountDeduction(ushort speechCount)
		{
			if(speechCount > 5)
			{
				return -1;
			}
			else
			{
				return 0;
			}
		}

		private static int GetSpeechCountBonus(ushort speechCount)
		{
			if(speechCount < 5)
			{
				return (5-speechCount) * 2;
			}
			else
			{
				return 0;
			}
		}
	}


	public class IcsScoreManager : MonoBehaviour
	{
		private const float DefaultTimeScale = 1.0f;

		public List<GameObject> scoreNotificationDestinations;

		public List<string> timeIsUpDestinationTags;

		//---------------------------------------------------
		private int timeLimit = 0;

		private GameObject mainMenu;
		private PanelMainController panelMainController;

		private List<GameObject> timeIsUpDestinations;

		private float timeLeft;
		
		private int score;


		void Awake()
		{
			this.timeLimit = IcsConfig.Instance.info.sessionTimeLimit;

			this.mainMenu = GameObject.FindGameObjectWithTag("MainMenu");

			this.panelMainController = this.mainMenu.GetComponent<PanelMainController>();


			this.timeIsUpDestinations = new List<GameObject>();

			foreach (string timeIsUpDestinationTag in this.timeIsUpDestinationTags)
			{
				GameObject[] timeIsUpDestinationArray = GameObject.FindGameObjectsWithTag(timeIsUpDestinationTag);

				foreach(GameObject timeIsUpDestination in timeIsUpDestinationArray)
				{
					this.timeIsUpDestinations.Add(timeIsUpDestination);
				}
			}
		}

		// Use this for initialization
		void Start()
		{
			this.UpdateScoreText(0, IcsConfig.Instance.GetTotalScore());
			
			this.score = 0;

			this.timeLeft = (float)timeLimit;

			this.panelMainController.SetTimeLeft(this.timeLeft);

			Time.timeScale = 0.0f;
		}


		// Update is called once per frame
		void Update()
		{
			this.timeLeft = Mathf.Max(0.0f, this.timeLeft-Time.deltaTime);

			this.panelMainController.SetTimeLeft(this.timeLeft);

			if(this.timeLeft == 0.0f)
			{
				foreach(GameObject timeIsUpDestination in this.timeIsUpDestinations)
				{
					ExecuteEvents.Execute<ITimeIsUpHandler>
					(
						target: timeIsUpDestination,
						eventData: null,
						functor: (reciever, eventData) => reciever.OnTimeIsUp()
					);
				}
			}
		}

		public void AddScore(Score.Type scoreType, params object[] args)
		{
			int additionalScore = Score.GetScore(scoreType, args);

			this.score = Mathf.Clamp(this.score + additionalScore, Score.MinScore, Score.MaxScore);

			this.UpdateScoreText(this.score);

			SIGVerseLogger.Info("Score add [" + additionalScore + "], Challenge " + IcsConfig.Instance.numberOfTrials + " Score=" + this.score);

			// Send the Score Notification
			ScoreStatus scoreStatus = new ScoreStatus(additionalScore, this.score, IcsConfig.Instance.GetTotalScore());

			foreach(GameObject scoreNotificationDestination in this.scoreNotificationDestinations)
			{
				ExecuteEvents.Execute<IScoreHandler>
				(
					target: scoreNotificationDestination,
					eventData: null,
					functor: (reciever, eventData) => reciever.OnScoreChange(scoreStatus)
				);
			}
		}

		public void TaskStart()
		{
			this.UpdateScoreText(this.score);

			Time.timeScale = IcsScoreManager.DefaultTimeScale;
		}

		public void TaskEnd()
		{
			Time.timeScale = 0.0f;

			IcsConfig.Instance.AddScore(this.score);

			this.UpdateScoreText(this.score, IcsConfig.Instance.GetTotalScore());

			SIGVerseLogger.Info("Total Score=" + IcsConfig.Instance.GetTotalScore().ToString());

			IcsConfig.Instance.RecordScoreInFile();
		}


		public void ResetTimeLeftText()
		{
			this.timeLeft = (float)timeLimit;
			this.panelMainController.SetTimeLeft(this.timeLeft);
		}

		private void UpdateScoreText(float score)
		{
			ExecuteEvents.Execute<IPanelScoreHandler>
			(
				target: this.mainMenu,
				eventData: null,
				functor: (reciever, eventData) => reciever.OnScoreChange(score)
			);
		}

		private void UpdateScoreText(float score, float total)
		{
			ExecuteEvents.Execute<IPanelScoreHandler>
			(
				target: this.mainMenu,
				eventData: null,
				functor: (reciever, eventData) => reciever.OnScoreChange(score, total)
			);
		}
	}
}

