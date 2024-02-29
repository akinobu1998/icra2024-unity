using SIGVerse.Common;
using SIGVerse.Common.Recorder;
using SIGVerse.FCSC.InteractiveCustomerService;
using SIGVerse.ToyotaHSR;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace SIGVerse.FCSC.Common
{
	[RequireComponent(typeof (CompetitionPlaybackCommon))]
	public class CompetitionPlaybackRecorder : WorldPlaybackRecorder, IScoreHandler, IPanelNoticeHandler
	{
		[HeaderAttribute("Competition GUI")]
		public GameObject mainPanel;
		public GameObject scorePanel;

		protected bool willRecord = true;

		protected ScoreStatus latestScoreStatus = new ScoreStatus();

		protected Text teamNameText;
		protected Text trialNumberText;
		protected Text timeLeftValText;
		protected Text taskMessageText;

		protected Text totalValText;


		public virtual void DisableRecorder()
		{
			this.willRecord = false;
		}

		protected virtual void Awake()
		{
//			base.Awake();

			if(!this.willRecord)
			{
				this.enabled = false;
				return;
			}

			this.teamNameText    = this.mainPanel.transform.Find("TargetsOfHiding/TeamNameText")                .GetComponent<Text>();
			this.trialNumberText = this.mainPanel.transform.Find("TargetsOfHiding/TrialNumberText")             .GetComponent<Text>();
			this.timeLeftValText = this.mainPanel.transform.Find("TargetsOfHiding/TimeLeftInfo/TimeLeftValText").GetComponent<Text>();
			this.taskMessageText = this.mainPanel.transform.Find("TargetsOfHiding/TaskMessageText")             .GetComponent<Text>();

			this.totalValText = this.scorePanel.transform.Find("TotalValText").GetComponent<Text>();
		}

		protected override List<string> GetDefinitionLines()
		{
			List<string> definitionLines = base.GetDefinitionLines();

			// Task Info
			definitionLines.Add(PlaybackTaskInfoEventController.GetDefinitionLine(this.teamNameText.text, this.trialNumberText.text, this.timeLeftValText.text, this.taskMessageText.text));

			// Score (Initial status of score)
			definitionLines.Add(PlaybackScoreEventController.GetDefinitionLine(this.totalValText.text));

			return definitionLines;
		}


		protected override void StopRecording()
		{
			// Add a line of latest total score
			if( this.latestScoreStatus.Score > 0)
			{
				this.latestScoreStatus.Total += this.latestScoreStatus.Score;
			}

			this.latestScoreStatus.Subscore = 0;

			base.AddDataLine(PlaybackScoreEventController.GetDataLine(this.GetHeaderElapsedTime(), this.latestScoreStatus));

			base.StopRecording();
		}

		public void OnScoreChange(ScoreStatus scoreStatus)
		{
			base.AddDataLine(PlaybackScoreEventController.GetDataLine(this.GetHeaderElapsedTime(), scoreStatus));

			this.latestScoreStatus = scoreStatus;
		}

		public void OnPanelNoticeChange(PanelNoticeStatus panelNoticeStatus)
		{
			base.AddDataLine(PlaybackPanelNoticeEventController.GetDataLine(this.GetHeaderElapsedTime(), panelNoticeStatus));
		}
	}
}
