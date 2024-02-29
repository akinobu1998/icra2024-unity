using System;
using System.Collections.Generic;
using UnityEngine;

namespace SIGVerse.FCSC.Common
{
	public class PanelPlaybackController : MonoBehaviour
	{
		public CompetitionPlaybackPlayer playbackPlayer;

		public void OnReadFileButtonClick()
		{
			this.playbackPlayer.OnReadFileButtonClick();
		}

		public void OnPlayButtonClick()
		{
			this.playbackPlayer.OnPlayButtonClick();
		}

		public void OnStartTimeEndEdit()
		{
			this.playbackPlayer.OnStartTimeEndEdit();
		}

		public void OnEndTimeEndEdit()
		{
			this.playbackPlayer.OnEndTimeEndEdit();
		}

		public void OnSliderChanged()
		{
			this.playbackPlayer.OnSliderChanged();
		}
	}
}

