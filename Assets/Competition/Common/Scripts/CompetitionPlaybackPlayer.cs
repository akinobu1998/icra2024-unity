using SIGVerse.Common;
using SIGVerse.Common.Recorder;
using SIGVerse.FCSC.InteractiveCustomerService;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SIGVerse.FCSC.Common
{
	[RequireComponent(typeof (CompetitionPlaybackCommon))]
	public class CompetitionPlaybackPlayer : WorldPlaybackPlayer
	{
		private const string ElapsedTimeFormat = "###0.0";
		private const string TotalTimeFormat   = "###0";

		[HeaderAttribute("Competition GUI")]
		public GameObject mainPanel;
		public GameObject scorePanel;

		//--------------------------------------------------------------------------------------------

		protected bool willPlay = true;

		protected int  trialNo = 0;

		protected int  timeLimit = 0;

		protected PlaybackTaskInfoEventController     taskInfoController;      // Task Info
		protected PlaybackScoreEventController        scoreController;         // Score
		protected PlaybackPanelNoticeEventController  panelNoticeController;   // Notice of a Panel

		//----------------------------

		protected PanelMainController mainPanelController;

		protected GameObject mainMenu;

		protected Button giveUpButton;

		protected Text teamNameText;
		protected Text trialNumberText;
		protected Text timeLeftValText;
		protected Text taskMessageText;

		protected Text scoreText;
		protected Text totalText;

		protected InputField trialNoInputField;
		protected GameObject collisionEffect;


		public virtual void DisablePlayer()
		{
			this.willPlay = false;
		}

		protected override void Awake()
		{
//			base.Awake();

			if(!this.willPlay)
			{
				this.enabled = false;
				return;
			}
			
			this.mainMenu = GameObject.FindGameObjectWithTag("MainMenu");

			this.mainPanelController = this.mainMenu.GetComponentInChildren<PanelMainController>();

			this.mainMenu.GetComponentInChildren<PanelMainController>().enabled = false;

			this.giveUpButton = this.mainPanel.transform.Find("TargetsOfHiding/Buttons/GiveUpButton").GetComponent<Button>();
			this.giveUpButton.interactable = false;

			this.teamNameText    = this.mainPanel.transform.Find("TargetsOfHiding/TeamNameText")                .GetComponent<Text>();
			this.trialNumberText = this.mainPanel.transform.Find("TargetsOfHiding/TrialNumberText")             .GetComponent<Text>();
			this.timeLeftValText = this.mainPanel.transform.Find("TargetsOfHiding/TimeLeftInfo/TimeLeftValText").GetComponent<Text>();
			this.taskMessageText = this.mainPanel.transform.Find("TargetsOfHiding/TaskMessageText")             .GetComponent<Text>();

			this.scoreText = this.scorePanel.transform.Find("ScoreValText").GetComponent<Text>();
			this.totalText = this.scorePanel.transform.Find("TotalValText").GetComponent<Text>();

			this.statusText          = this.playbackPanel.transform.Find("StatusText")                 .GetComponent<Text>();
			this.trialNoInputField   = this.playbackPanel.transform.Find("ReadFile/TrialNoInputField") .GetComponent<InputField>();
			this.readFileButton      = this.playbackPanel.transform.Find("ReadFile/ReadFileButton")    .GetComponent<Button>();
			this.elapsedTimeText     = this.playbackPanel.transform.Find("ElapsedTime/ElapsedTimeText").GetComponent<Text>();
			this.totalTimeText       = this.playbackPanel.transform.Find("TotalTime/TotalTimeText")    .GetComponent<Text>();
			this.timeSlider          = this.playbackPanel.transform.Find("TimeSlider")                 .GetComponent<Slider>();
			this.playButton          = this.playbackPanel.transform.Find("PlayButton")                 .GetComponent<Button>();
			this.speedDropdown       = this.playbackPanel.transform.Find("Speed/SpeedDropdown")        .GetComponent<Dropdown>();
			this.repeatToggle        = this.playbackPanel.transform.Find("Repeat/RepeatToggle")        .GetComponent<Toggle>();
			this.startTimeInputField = this.playbackPanel.transform.Find("StartTimeInputField")        .GetComponent<InputField>();
			this.endTimeInputField   = this.playbackPanel.transform.Find("EndTimeInputField")          .GetComponent<InputField>();

			this.collisionEffect = (GameObject)Resources.Load(SIGVerseUtils.CollisionEffectPath);
		}

		// Use this for initialization
		protected override void Start()
		{
			base.Start();

			this.taskInfoController     = new PlaybackTaskInfoEventController(this.teamNameText, this.trialNumberText, this.timeLeftValText, this.taskMessageText);
			this.scoreController        = new PlaybackScoreEventController(this.scoreText, this.totalText); // Score
			this.panelNoticeController  = new PlaybackPanelNoticeEventController(this, this.mainMenu);      // Notice of a Panel
		}


		protected virtual void Update()
		{
			this.isStepChanged = this.step!=this.preStep;

			this.isFileRead = this.step==Step.Waiting && this.preStep==Step.Initializing;

			this.preStep = this.step;

//			base.Update();
		}


		protected override void ReadData(string[] headerArray, string dataStr)
		{
			base.ReadData(headerArray, dataStr);

			this.taskInfoController    .ReadEvents(headerArray, dataStr); // Task Info
			this.scoreController       .ReadEvents(headerArray, dataStr); // Score
			this.panelNoticeController .ReadEvents(headerArray, dataStr); // Notice of a Panel
		}

		protected override void StartInitializingEvents()
		{
			base.StartInitializingEvents();

			this.taskInfoController    .StartInitializingEvents(); // Task Info
			this.scoreController       .StartInitializingEvents(); // Score
			this.panelNoticeController .StartInitializingEvents(); // Notice of a Panel
		}

		protected override void UpdateIndexAndElapsedTime(float elapsedTime)
		{
			base.UpdateIndexAndElapsedTime(elapsedTime);

			this.taskInfoController    .UpdateIndex(elapsedTime); // Task Info
			this.scoreController       .UpdateIndex(elapsedTime); // Score
			this.panelNoticeController .UpdateIndex(elapsedTime); // Notice of a Panel
		}


		protected override void UpdateData(float deltaTime)
		{
			base.UpdateData(deltaTime);

			this.taskInfoController    .ExecutePassedLatestEvents(this.elapsedTime, this.deltaTime); // Task Info
			this.scoreController       .ExecutePassedLatestEvents(this.elapsedTime, this.deltaTime); // Score
			this.panelNoticeController .ExecutePassedAllEvents(this.elapsedTime, this.deltaTime);    // Notice of a Panel
		}

		protected override void UpdateDataByLatest(float elapsedTime)
		{
			base.UpdateDataByLatest(elapsedTime);

			this.taskInfoController.ExecuteLatestEvents(); // Task Info
			this.scoreController   .ExecuteLatestEvents(); // Score
		}

		protected override float GetTotalTime()
		{
			return Mathf.Max(
				base.GetTotalTime(), 
				this.taskInfoController    .GetTotalTime(), 
				this.scoreController       .GetTotalTime(), 
				this.panelNoticeController .GetTotalTime()
			);
		}


		//----------------------------   GUI related codes are below   ---------------------------------------------


		protected override void OnGUI()
		{
			// Update a text of status
			if(this.errorMsg != string.Empty)
			{
				this.statusText.text = this.errorMsg;
				this.SetTextColorAlpha(this.statusText, 1.0f);

				return;
			}

			switch(this.step)
			{
				case Step.Waiting:
				{
					if(this.isStepChanged)
					{
						Debug.Log("Waiting");

						if(this.isInitialized)
						{
							this.trialNoInputField  .interactable = true;
							this.readFileButton     .interactable = true;
							this.timeSlider         .interactable = true;
							this.playButton         .interactable = true;
							this.speedDropdown      .interactable = true;
							this.repeatToggle       .interactable = true;
							this.startTimeInputField.interactable = true;
							this.endTimeInputField  .interactable = true;
						}

						if(this.isFileRead)
						{
							this.SetTextColorAlpha(this.statusText, 0.0f);

							this.totalTimeText.text = this.elapsedTime.ToString(ElapsedTimeFormat);

							this.totalTimeText.text = Math.Ceiling(this.GetTotalTime()).ToString(TotalTimeFormat);
							this.totalTimeInt   = int.Parse(this.totalTimeText.text);

							this.ResetTimeSlider();
							this.SetStartTime(0);
							this.SetEndTime(this.totalTimeInt);
						
							this.UpdateDataByLatest(0);

							this.isFileRead = false;
						}

						this.SetTextColorAlpha(this.statusText, 0.0f);

						this.playButton.image.sprite= this.playSprite;

						this.isStepChanged = false;
					}

					this.UpdateTimeDisplay();

					break;
				}
				case Step.Initializing:
				{
					if(this.isStepChanged)
					{
						SIGVerseLogger.Info("Initializing");

						this.statusText.text = "Reading...";

						this.isStepChanged = false;
					}

					this.SetTextColorAlpha(this.statusText, Mathf.Sin(5.0f * Time.time) * 0.5f + 0.5f);
					break;
				}
				case Step.Playing:
				{
					if(this.isStepChanged)
					{
						SIGVerseLogger.Info("Playing");

						this.statusText.text = "Playing...";

						this.playButton.image.sprite= this.pauseSprite;

						this.trialNoInputField  .interactable = false;
						this.readFileButton     .interactable = false;
						this.timeSlider         .interactable = false;
						this.speedDropdown      .interactable = false;
						this.repeatToggle       .interactable = false;
						this.startTimeInputField.interactable = false;
						this.endTimeInputField  .interactable = false;

						this.isStepChanged = false;
					}

					this.SetTextColorAlpha(this.statusText, Mathf.Sin(5.0f * Time.time) * 0.5f + 0.5f);

					this.UpdateTimeDisplay();

					this.timeSlider.value = Mathf.Clamp((this.elapsedTime-this.startTime)/(this.endTime-this.startTime), 0.0f, 1.0f);

					break;
				}
			}
		}

		private float GetElapsedTimeUsingSlider()
		{
			return this.startTime + (this.endTime - this.startTime) * this.timeSlider.value;
		}

		private void SetTextColorAlpha(Text text, float alpha)
		{
			text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
		}

		private void ResetTimeSlider()
		{
			this.timeSlider.value = 0.0f;
		}

		private void UpdateTimeDisplay()
		{
			float time = (this.elapsedTime < this.endTime)? this.elapsedTime : this.endTime; 

			this.elapsedTimeText.text = time.ToString(ElapsedTimeFormat);

			this.mainPanelController.SetTimeLeft(this.timeLimit - time);
		}

		private void SetStartTime(int startTime)
		{
			this.startTime                = startTime;
			this.startTimeInputField.text = startTime.ToString();
		}

		private void SetEndTime(int endTime)
		{
			this.endTime                = endTime;
			this.endTimeInputField.text = endTime.ToString();
		}



		public virtual void OnReadFileButtonClick()
		{
			this.Initialize(this.filePath);
		}
		public Step GetStep()
		{
			return this.step;
		}

		public float GetPlayingSpeed()
		{
			return this.playingSpeed;
		}
	
	}
}

