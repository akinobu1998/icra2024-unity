using UnityEngine;
using SIGVerse.FCSC.Common;
using SIGVerse.Common;

namespace SIGVerse.FCSC.InteractiveCustomerService
{ 
	[System.Serializable]
	public class IcsConfigFileInfo: CompetitionConfigFileInfo
	{
		public int dummyMode; // for test
	}

	public class IcsConfig : CompetitionConfigSingleton<IcsConfig>
	{
		public const string FolderPath     = "/../SIGVerseConfig/InteractiveCustomerService/";
		public const string ConfigFileName = "IcsConfig.json";
		public const string ScoreFileName  = "IcsScore.txt";

		public new IcsConfigFileInfo info;
		
		protected IcsConfig() { } // guarantee this will be always a singleton only - can't use the constructor!

		public override void Awake()
		{
			SetFilePath();

			this.info = ReadConfigFile<IcsConfigFileInfo>();

			Initialize(this.info);
		}
		
		protected override void SetFilePath()
		{
			this.configFilePath = Application.dataPath + FolderPath+ ConfigFileName;
			this.scoreFilePath  = Application.dataPath + FolderPath+ ScoreFileName;
		}

		protected override string CreateConfigFileInfo()
		{
			IcsConfigFileInfo configFileInfo = new IcsConfigFileInfo();

			configFileInfo.teamName          = "Inter@ctiveCustomerService";
			configFileInfo.sessionTimeLimit  = 600;
			configFileInfo.maxNumberOfTrials = 15;
			configFileInfo.isScoreFileRead   = false;
			configFileInfo.playbackType      = IcsPlaybackCommon.PlaybackTypeRecord;
			configFileInfo.bgmVolume         = 0.01f;
			configFileInfo.dummyMode         = 0;

			return JsonUtility.ToJson(configFileInfo);
		}
	}
}

