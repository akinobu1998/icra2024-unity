using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SIGVerse.FCSC.Common
{
	public interface IPanelNoticeHandler : IEventSystemHandler
	{
		void OnPanelNoticeChange(PanelNoticeStatus panelNoticeStatus);
	}
	
	public class PanelNoticeStatus
	{
		public static readonly Color Green = new Color(  0/255f, 143/255f,  36/255f, 255/255f);
		public static readonly Color Red   = new Color(255/255f,   0/255f,   0/255f, 255/255f);
		public static readonly Color Blue  = new Color(  0/255f,   0/255f, 255/255f, 255/255f);

		public string Message  { get; set; }
		public int    FontSize { get; set; }
		public Color  Color    { get; set; }
		public float  Duration { get; set; }

		public PanelNoticeStatus(string message, int fontSize, Color color, float duration)
		{
			this.Message  = message;
			this.FontSize = fontSize;
			this.Color    = color;
			this.Duration = duration;
		}

		public PanelNoticeStatus(PanelNoticeStatus panelNoticeStatus)
		{
			this.Message  = panelNoticeStatus.Message;
			this.FontSize = panelNoticeStatus.FontSize;
			this.Color    = panelNoticeStatus.Color;
			this.Duration = panelNoticeStatus.Duration;
		}
	}


	public class PanelNoticeController : MonoBehaviour, IPanelNoticeHandler
	{
		public GameObject noticePanel;

		private Text noticeText;
		private float maxDuration = 1000f;

		void Awake()
		{
			this.noticeText = this.noticePanel.GetComponentInChildren<Text>();
		}

		void Start()
		{
			this.noticePanel.SetActive(false);
		}

		public void SetMaxDuration(float maxDuration)
		{
			this.maxDuration = maxDuration;
		}

		private void ShowNotice(PanelNoticeStatus panelNoticeStatus)
		{
			this.ShowNoticeExe(panelNoticeStatus);
		}

		private void ShowNoticeExe(PanelNoticeStatus panelNoticeStatus)
		{
			this.noticePanel.SetActive(true);

			noticeText.text     = panelNoticeStatus.Message;
			noticeText.fontSize = panelNoticeStatus.FontSize;
			noticeText.color    = panelNoticeStatus.Color;

			StartCoroutine(this.HideNotice(panelNoticeStatus.Duration)); // Hide
		}

		private IEnumerator HideNotice(float duration)
		{
			if (duration > this.maxDuration)
			{
				duration = this.maxDuration;
			}

			float hideTime = UnityEngine.Time.time + duration;

			while(UnityEngine.Time.time < hideTime)
			{
				yield return null;
			}

			this.noticePanel.SetActive(false);
		}

		public void OnPanelNoticeChange(PanelNoticeStatus panelNoticeStatus)
		{
			this.ShowNotice(panelNoticeStatus);
		}
	}
}

