using System.Collections.Generic;
using UnityEngine;
using SIGVerse.ToyotaHSR;
using SIGVerse.Common;
using SIGVerse.FCSC.Common;
using SIGVerse.Common.Recorder;

namespace SIGVerse.FCSC.InteractiveCustomerService
{
	public class IcsPlaybackCommon : CompetitionPlaybackCommon
	{
		public const string FilePathFormat = "/../SIGVerseConfig/InteractiveCustomerService/Playback{0:D2}.dat";

		//---------------------------------------

		public GameObject playbackPanel;

		public virtual void Awake()
		{
			if(IcsConfig.Instance.info.playbackType != IcsPlaybackCommon.PlaybackTypePlay)
			{
				this.playbackPanel.SetActive(false);
			}
		}
	}
}

