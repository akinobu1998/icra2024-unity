using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using SIGVerse.Common;
using System.Linq;
using static Unity.Collections.AllocatorManager;

namespace SIGVerse.FCSC.Common
{ 
	[System.Serializable]
	public class CompetitionConfigFileInfo
	{
		public string teamName;
		public int    sessionTimeLimit;
		public int    maxNumberOfTrials;
		public bool   isScoreFileRead;
		public int    playbackType;
		public float  bgmVolume;
	}

	public abstract class CompetitionConfigSingleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		public CompetitionConfigFileInfo info;

		public int numberOfTrials;
		public List<int> scores;

		protected CompetitionConfigSingleton() { } // guarantee this will be always a singleton only - can't use the constructor!

		protected string configFilePath;
		protected string scoreFilePath;

		public virtual void Awake()
		{
			SetFilePath();

			this.info = ReadConfigFile<CompetitionConfigFileInfo>();
			Initialize(this.info);
		}

		protected virtual void SetFilePath()
		{
			this.configFilePath = Application.dataPath + "/../SIGVerseConfig/Common/CommonConfig.json"; // Sample File Path
			this.scoreFilePath  = Application.dataPath + "/../SIGVerseConfig/Common/CommonScore.txt";   // Sample File Path
		}

		protected virtual TConfigInfo ReadConfigFile<TConfigInfo>() where TConfigInfo : CompetitionConfigFileInfo
		{
			TConfigInfo configFileInfo;

			if (File.Exists(this.configFilePath))
			{
				// File open
				StreamReader streamReader = new StreamReader(this.configFilePath, Encoding.UTF8);

				configFileInfo = JsonUtility.FromJson<TConfigInfo>(streamReader.ReadToEnd());

				streamReader.Close();
			}
			else
			{
#if UNITY_EDITOR
				SIGVerseLogger.Warn("Config file does not exists.");

				configFileInfo = JsonUtility.FromJson<TConfigInfo>(CreateConfigFileInfo());

				SaveConfig(configFileInfo);
#else
				SIGVerseLogger.Error("Config file does not exists.");
				Application.Quit();
#endif
			}

			return configFileInfo;
		}

		protected virtual string CreateConfigFileInfo()
		{
			CompetitionConfigFileInfo configFileInfo = new CompetitionConfigFileInfo();

			configFileInfo.teamName          = "XXXX";
			configFileInfo.sessionTimeLimit  = 600;
			configFileInfo.maxNumberOfTrials = 15;
			configFileInfo.isScoreFileRead   = false;
			configFileInfo.playbackType      = CompetitionPlaybackCommon.PlaybackTypeRecord;
			configFileInfo.bgmVolume         = 0.01f;

			return JsonUtility.ToJson(configFileInfo);
		}

		protected virtual void SaveConfig<TConfigInfo>(TConfigInfo configFileInfo) where TConfigInfo : CompetitionConfigFileInfo
		{
			StreamWriter streamWriter = new StreamWriter(configFilePath, false, Encoding.UTF8);

			SIGVerseLogger.Info("Save config file: " + JsonUtility.ToJson(configFileInfo));

			streamWriter.WriteLine(JsonUtility.ToJson(configFileInfo, true));

			streamWriter.Flush();
			streamWriter.Close();
		}

		protected virtual void Initialize<TConfigInfo>(TConfigInfo configFileInfo) where TConfigInfo : CompetitionConfigFileInfo
		{
			this.scores = new List<int>();

			if (configFileInfo.isScoreFileRead)
			{
				if(!System.IO.File.Exists(this.scoreFilePath))
				{
					SIGVerseLogger.Error("Score file does not exists.");
					Application.Quit();
				}

				// File open
				StreamReader streamReader = new StreamReader(scoreFilePath, Encoding.UTF8);

				string line;

				while ((line = streamReader.ReadLine()) != null)
				{
					string scoreStr = line.Trim();

					if (scoreStr == string.Empty) { continue; }

					this.scores.Add(Int32.Parse(scoreStr));
				}

				streamReader.Close();

				this.numberOfTrials = this.scores.Count;

				if (this.numberOfTrials >= configFileInfo.maxNumberOfTrials)
				{
					SIGVerseLogger.Error("this.numberOfTrials >= this.configFileInfo.maxNumberOfTrials");
					Application.Quit();
				}
			}
			else
			{
				this.numberOfTrials = 0;
			}
		}

		public virtual void InclementNumberOfTrials()
		{
			this.numberOfTrials++; 
		}

		public virtual void AddScore(int score)
		{
			this.scores.Add(score);
		}

		public virtual int GetTotalScore()
		{
			return this.scores.Where(score => score > 0).Sum();
		}

		public virtual void RecordScoreInFile()
		{
			bool append = true;

			if(this.numberOfTrials==1) { append = false; }

			StreamWriter streamWriter = new StreamWriter(this.scoreFilePath, append, Encoding.UTF8);

			SIGVerseLogger.Info("Record the socre in a file. path=" + this.scoreFilePath);

			streamWriter.WriteLine(this.scores[this.scores.Count - 1]);

			streamWriter.Flush();
			streamWriter.Close();
		}



		//////////     Singleton     //////////////////
		private static T _instance;

		private static object _lock = new object();

		public static T Instance
		{
			get
			{
				if (isApplicationQuitting)
				{
					Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
						"' already destroyed on application quit." +
						" Won't create again - returning null.");
					return null;
				}

				lock (_lock)
				{
					if (_instance == null)
					{
						_instance = (T)FindObjectOfType(typeof(T));

						if (FindObjectsOfType(typeof(T)).Length > 1)
						{
							Debug.LogError("[Singleton] Something went really wrong " +
								" - there should never be more than 1 singleton!" +
								" Reopening the scene might fix it.");
							return _instance;
						}

						if (_instance == null)
						{
							GameObject singleton = new GameObject();
							_instance = singleton.AddComponent<T>();
							singleton.name = "(singleton) " + typeof(T).ToString();

							DontDestroyOnLoad(singleton);

							Debug.Log("[Singleton] An instance of " + typeof(T) +
								" is needed in the scene, so '" + singleton +
								"' was created with DontDestroyOnLoad.");
						}
						else
						{
							Debug.Log("[Singleton] Using instance already created: " +
								_instance.gameObject.name);
						}
					}

					return _instance;
				}
			}
		}

		private static bool isApplicationQuitting = false;

		public virtual void OnDestroy()
		{
			isApplicationQuitting = true;
		}
	}
}

