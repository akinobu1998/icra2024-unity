using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using SIGVerse.Common;
using SIGVerse.ToyotaHSR;
using UnityEngine.UI;
using SIGVerse.FCSC.Common;
using SIGVerse.Common.Recorder;
using TMPro;
using UnityEngine.Animations;

namespace SIGVerse.FCSC.InteractiveCustomerService
{
	public enum ModeratorStep
	{
		//RobotState.Standby
		Initialize,
		WaitForStart,
		TaskStart,
		WaitForIamReady, 

		//RobotState.InConversation
		SendFirstMessage,
		InConversation,
		GiveItem, 
		Judgement,
		WaitForNextTask,
		
		//RobotState.Moving
		TakeItemGo, 
		TakeItemBack,
	}


	public class IcsModerator : MonoBehaviour, IRosConversationReceiveHandler, ITimeIsUpHandler, IGiveUpHandler
	{
		//private const string TagRobot     = "Robot";

		private const int   AreYouReadyInterval = 1000;
		private const float RobotStatusInterval = 1.0f;


		private const string MsgAreYouReady       = "Are_you_ready?";
		private const string MsgCustomerMessage   = "customer_message";
		private const string MsgRobotMsgSucceeded = "robot_message_succeeded";
		private const string MsgRobotMsgFailed    = "robot_message_failed";
		private const string MsgTakeItemSucceeded = "take_item_succeeded";
		private const string MsgTakeItemFailed    = "take_item_failed";
		private const string MsgGiveItemSucceeded = "give_item_succeeded";
		private const string MsgGiveItemFailed    = "give_item_failed";
		private const string MsgTaskSucceeded     = "Task_succeeded";
		private const string MsgTaskFailed        = "Task_failed";
		private const string MsgMissionComplete   = "Mission_complete";

		private const string MsgYes = "Yes";
		private const string MsgNo  = "No";
		private const string MsgIdontKnow = "I don't know";

		private const string MsgBadTiming       = "Bad_timing";
		private const string MsgItemNotFound    = "Item not found";
		private const string MsgYouHaveNothing  = "You have nothing";
		private const string MsgWrongProduct    = "Wrong product";

		private const string MsgIamReady     = "I_am_ready";
		private const string MsgRobotMessage = "robot_message";
		private const string MsgTakeItem     = "take_item";
		private const string MsgGiveItem     = "give_item";
		private const string MsgGiveUp       = "Give_up";

		private const string ReasonTimeIsUp = "Time_is_up";
		private const string ReasonGiveUp   = MsgGiveUp;

		//-----------------------------

		[HeaderAttribute("GUI")]
		public GameObject mainMenu;
		public TMP_Text customerMessageText;
		public TMP_Text robotStateText;
		public TMP_Text graspedItemText;
		public TMP_Text robotMessageText;
		public Button   customerYesButton;
		public Button   customerNoButton;
		public Button   customerIdontKnowButton;
		public Image    taskImageImage;
		public CameraForUI cameraForTargetObj;
		public CameraForUI cameraForGraspedObj;

		[HeaderAttribute("Objects")]
		public IcsScoreManager scoreManager;
		public GameObject playbackManager;

		public AudioSource bgmAudioSource;
		//public GameObject basket;
		public IcsNavMeshRoutingRobot robotRouting;
		public IcsRobotArmController armController;
		public GameObject robotRosBridgeScripts;

		//-----------------------------

		//private GameObject robot;

		private IcsModeratorTool tool;
		private StepTimer stepTimer;

		private PanelMainController mainPanelController;

		private IcsPubRobotStatus icsPubRobotStatus;
		private IcsPubCustomerImage       icsPubImage;

		private string taskMessage;
		private Texture2D flippedTaskImageTexture;

		private ModeratorStep step;
		private string robotMessage;
		private string itemThatRobotWants;
		private GameObject graspedItem;
		private ushort robotSpeechCnt;

		private Dictionary<string, bool> receivedMessageMap;

		private string customerButtonMsg;

		//private Vector3 takeItemDestination;

		private bool isAllTaskFinished;
		private string interruptedReason;

		private string lastPanelMessage;

		private float robotStatusElapsedTime;

		void Awake()
		{
			try
			{
				if(IcsConfig.Instance.info.playbackType == WorldPlaybackCommon.PlaybackTypePlay) { return; }

				//this.robot = GameObject.FindGameObjectWithTag(TagRobot);

				this.tool = new IcsModeratorTool(this);

				this.stepTimer = new StepTimer();

				this.mainPanelController = this.mainMenu.GetComponent<PanelMainController>();

				this.icsPubRobotStatus = this.robotRosBridgeScripts.GetComponent<IcsPubRobotStatus>();
				this.icsPubImage       = this.robotRosBridgeScripts.GetComponent<IcsPubCustomerImage>();
			}
			catch (Exception exception)
			{
				Debug.LogError(exception);
				SIGVerseLogger.Error(exception.Message);
				SIGVerseLogger.Error(exception.StackTrace);
				this.ApplicationQuitAfter1sec();
			}
		}


		// Use this for initialization
		void Start()
		{
			this.step = ModeratorStep.Initialize;
			this.receivedMessageMap = new Dictionary<string, bool>
			{
				{ MsgIamReady,     false },
				{ MsgRobotMessage, false },
				{ MsgTakeItem,     false },
				{ MsgGiveItem,     false },
				{ MsgGiveUp,       false }
			};
			this.customerButtonMsg = string.Empty;

			this.isAllTaskFinished = false;
			this.interruptedReason = string.Empty;

			List<GameObject> graspables = this.tool.GetGraspables();

			for (int i=0; i<graspables.Count; i++)
			{
				Rigidbody rigidbody = graspables[i].GetComponent<Rigidbody>();

				rigidbody.constraints
					= RigidbodyConstraints.FreezeRotation |
					  RigidbodyConstraints.FreezePositionX |
					  RigidbodyConstraints.FreezePositionZ;

				rigidbody.maxDepenetrationVelocity = 0.5f;

				StartCoroutine(this.tool.LoosenRigidbodyConstraints(rigidbody));
			}

			this.bgmAudioSource.volume = Mathf.Clamp01(IcsConfig.Instance.info.bgmVolume);

			this.robotStatusElapsedTime = 0.0f;
		}


		private void PreProcess()
		{
			this.graspedItem = null;
			this.robotSpeechCnt = 0;
			UpdateRobotStatus();
			this.robotMessage = string.Empty;
			this.itemThatRobotWants = string.Empty;
			//this.takeItemDestinatin = Vector3.zero;

			this.mainPanelController.SetTeamNameText("Team: " + IcsConfig.Instance.info.teamName);
			this.mainPanelController.SetTrialNumberText(IcsConfig.Instance.numberOfTrials);

			SIGVerseLogger.Info("##### " + this.mainPanelController.GetTrialNumberText() + " #####");

			this.scoreManager.ResetTimeLeftText();

			this.taskMessage = this.tool.GetTaskInfo().message;
			this.customerMessageText.text = this.taskMessage;
			this.robotMessageText.text = "---";

			if(this.tool.GetTaskInfo().hasImage)
			{
				this.flippedTaskImageTexture = this.tool.GetFlippedTaskImage();
				
				this.taskImageImage.preserveAspect = true;
				this.taskImageImage.sprite = Sprite.Create(this.flippedTaskImageTexture, new Rect(0f, 0f, this.flippedTaskImageTexture.width, this.flippedTaskImageTexture.height), new Vector2(0.5f, 0.5f), 100f);
			}
			else
			{
				this.taskImageImage.enabled = false;
			}

			this.cameraForTargetObj.Capture(this.tool.GetTargetItem());

			SIGVerseLogger.Info("Task message="+this.taskMessage);

			foreach(string key in new List<string>(this.receivedMessageMap.Keys))
			{
				this.receivedMessageMap[key] = false;
			}
			
			this.customerButtonMsg = string.Empty;

			this.robotStatusElapsedTime = 0.0f;

			this.tool.InitializePlayback();

			SIGVerseLogger.Info("End of PreProcess: Session " + IcsConfig.Instance.numberOfTrials);
		}


		private void PostProcess()
		{
			SIGVerseLogger.Info("Task end");
			
			if (IcsConfig.Instance.numberOfTrials == IcsConfig.Instance.info.maxNumberOfTrials)
			{
				this.SendRosMessage(MsgMissionComplete, string.Empty);

				StartCoroutine(this.tool.CloseRosConnections());

				StartCoroutine(this.DisplayEndMessage());

				this.isAllTaskFinished = true;
			}
			else
			{
				this.tool.AddSpeechQueModerator("Let's go to the next session");

				StartCoroutine(this.tool.ClearRosConnections());

				this.step = ModeratorStep.WaitForNextTask;
			}
		}

		// Update is called once per frame
		void Update ()
		{
			try
			{
				this.tool.ControlSpeech(this.step==ModeratorStep.WaitForNextTask); // Speech

				if (this.isAllTaskFinished) { return; }

				if(this.interruptedReason!=string.Empty && this.step != ModeratorStep.WaitForNextTask)
				{
					SIGVerseLogger.Info("Failed '" + this.interruptedReason + "'");
					this.SendPanelNotice("Failed\n"+ this.interruptedReason.Replace('_',' '), 100, PanelNoticeStatus.Red);

					this.tool.StopSpeechForcefully();

					if(this.interruptedReason==ReasonTimeIsUp)
					{
						this.tool.AddSpeechQueModerator(ReasonTimeIsUp);
					}
					else if(this.interruptedReason==ReasonGiveUp)
					{
						this.tool.AddSpeechQueHsr(ReasonGiveUp);
					}
					
					this.GoToNextTaskTaskFailed(this.interruptedReason);
				}

				// Send Robot Status
				this.robotStatusElapsedTime += Time.deltaTime;

				if (this.robotStatusElapsedTime >= RobotStatusInterval)
				{
					UpdateRobotStatus();
					this.robotStatusElapsedTime = 0.0f;
				}

				if(this.receivedMessageMap[MsgRobotMessage] && this.step!=ModeratorStep.InConversation)
				{
					SIGVerseLogger.Warn("Bad timing. message : " + MsgRobotMessage + ", step=" + this.step);
					SendRosMessage(MsgRobotMsgFailed, MsgBadTiming+":"+this.robotMessage);
					this.receivedMessageMap[MsgRobotMessage] = false;
					this.robotMessage = string.Empty;
				}

				if(this.receivedMessageMap[MsgTakeItem] && this.step!=ModeratorStep.InConversation)
				{
					SIGVerseLogger.Warn("Bad timing. message : " + MsgTakeItem + ", step="+this.step);
					SendRosMessage(MsgTakeItemFailed, MsgBadTiming+":"+this.itemThatRobotWants);
					this.receivedMessageMap[MsgTakeItem] = false;
					this.itemThatRobotWants = string.Empty;
				}

				if(this.receivedMessageMap[MsgGiveItem] && this.step!=ModeratorStep.InConversation)
				{
					SIGVerseLogger.Warn("Bad timing. message : " + MsgGiveItem + ", step="+this.step);
					SendRosMessage(MsgGiveItemFailed, MsgBadTiming);
					this.receivedMessageMap[MsgGiveItem] = false;
				}

				if(this.customerButtonMsg!=string.Empty && (this.step!=ModeratorStep.InConversation || ExistsUnreadRobotMessage() || this.tool.IsSpeaking()))
				{
					SIGVerseLogger.Warn("Disable Customer Button Event: "+this.customerButtonMsg);
					this.customerButtonMsg = string.Empty;
				}

				switch (this.step)
				{
					case ModeratorStep.Initialize:

						SIGVerseLogger.Info("Initialize");
						this.PreProcess();
						this.step++;
						break;

					case ModeratorStep.WaitForStart:

						if (this.stepTimer.IsTimePassed((int)this.step, 3000))
						{
							if (this.tool.IsPlaybackInitialized() && this.tool.IsConnectedToRos())
							{
								this.step++; break;
							}
						}
						break;

					case ModeratorStep.TaskStart:

						SIGVerseLogger.Info("Task start!");
						this.tool.AddSpeechQueModerator("Task start!");

						this.scoreManager.TaskStart();

						this.tool.StartPlayback();

						this.step++;
						break;

					case ModeratorStep.WaitForIamReady:

//						Debug.LogError("WaitForIamReady");
						if (this.receivedMessageMap[MsgIamReady] && !this.tool.IsSpeaking())
						{
							this.step++;
							UpdateRobotStatus();
							break;
						}

						if (this.stepTimer.IsTimePassed((int)this.step, AreYouReadyInterval))
						{
							this.SendRosMessage(MsgAreYouReady, string.Empty);
						}
						break;

					case ModeratorStep.SendFirstMessage:

						if (this.stepTimer.IsTimePassed((int)this.step, 1000))
						{
							this.SendRosMessage(MsgCustomerMessage, this.taskMessage);
							if(this.tool.GetTaskInfo().hasImage)
							{
								this.icsPubImage.PubImage(this.flippedTaskImageTexture);
							}
							this.tool.AddSpeechQueModerator(this.taskMessage);

							SIGVerseLogger.Info("Sent the first message.");

							this.step++; break;
						}

						break;

					case ModeratorStep.InConversation:

						if (this.receivedMessageMap[MsgRobotMessage] && !this.tool.IsSpeaking())
						{
							SIGVerseLogger.Info("Receive Robot Message.");

							this.robotMessageText.text = this.robotMessage;
							this.tool.AddSpeechQueHsr(this.robotMessage);

							SendRosMessage(MsgRobotMsgSucceeded, this.robotMessage);
							// this.robotSpeechCnt++;
							this.robotSpeechCnt = 0;
							SIGVerseLogger.Info("Robot Speech Count="+this.robotSpeechCnt);
							this.scoreManager.AddScore(Score.Type.RobotSpeech, new object[] { this.robotSpeechCnt });

							this.receivedMessageMap[MsgRobotMessage] = false;
							this.robotMessage = string.Empty;

							UpdateRobotStatus();

							break;
						}
						if (this.receivedMessageMap[MsgTakeItem])
						{
							SIGVerseLogger.Info("Receive Take Item.");

							SendRosMessage(MsgTakeItemSucceeded, this.itemThatRobotWants);

							this.receivedMessageMap[MsgTakeItem] = false;

							if(this.graspedItem!=null && this.graspedItem.name==this.itemThatRobotWants)
							{
								SIGVerseLogger.Warn("The robot has already grasped the item. Item name="+this.itemThatRobotWants);
							}
							else
							{
								this.robotRouting.SetItem(TakeItem(this.itemThatRobotWants).transform);
								this.step = ModeratorStep.TakeItemGo;
							}
							
							UpdateRobotStatus();
							break;
						}
						if (this.receivedMessageMap[MsgGiveItem])
						{
							SIGVerseLogger.Info("Receive GiveItem.");

							SendRosMessage(MsgGiveItemSucceeded, string.Empty);

							this.armController.DownArmLittle();

							this.receivedMessageMap[MsgGiveItem] = false;
							this.step++;

							break;
						}

						if (this.customerButtonMsg != string.Empty)
						{
							if (this.customerButtonMsg == MsgYes)
							{
								this.SendRosMessage(MsgCustomerMessage, MsgYes);
								this.tool.AddSpeechQueModerator(MsgYes);
							}
							else if (this.customerButtonMsg == MsgNo)
							{
								this.SendRosMessage(MsgCustomerMessage, MsgNo);
								this.tool.AddSpeechQueModerator(MsgNo);
							}
							else if (this.customerButtonMsg == MsgIdontKnow)
							{
								this.SendRosMessage(MsgCustomerMessage, MsgIdontKnow);
								this.tool.AddSpeechQueModerator(MsgIdontKnow);
							}
							this.customerButtonMsg = string.Empty;
							break;
						}
						break;

					case ModeratorStep.GiveItem:

						if (this.armController.IsReachedDestination())
						{
							EnableCollision(this.graspedItem.transform);

							ParentConstraint parentConstraint = this.graspedItem.GetComponent<ParentConstraint>();

							if(parentConstraint==null)
							{
								SIGVerseLogger.Error("parentConstraint==null !!");
							}
							else
							{
								parentConstraint.constraintActive = false;
							}
							this.step++;
							break;
						}
						break;

					case ModeratorStep.Judgement:

						if (this.stepTimer.IsTimePassed((int)this.step, 3000))
						{
							bool isSucceeded = this.tool.GetTargetItem() == this.graspedItem;

							// if (isSucceeded)
							// {
							SIGVerseLogger.Info("Task Completed");
							// this.SendPanelNotice("Task Completed", 120, PanelNoticeStatus.Green);
							this.scoreManager.AddScore(Score.Type.PlacementSuccess, new object[] { this.robotSpeechCnt });
							// this.tool.AddSpeechQueModerator("Excellent!");

							// this.GoToNextTaskTaskSucceeded();
							this.step = ModeratorStep.InConversation;
							// }
							// else
							// {
							// 	SIGVerseLogger.Info("Failed '" + MsgWrongProduct + "'");
							// 	this.SendPanelNotice("Failed\n" + MsgWrongProduct, 100, PanelNoticeStatus.Red);
							// 	this.scoreManager.AddScore(Score.Type.PlacementFailure, new object[] { this.robotSpeechCnt });
							// 	// this.tool.AddSpeechQueModeratorFailed();

							// 	// this.GoToNextTaskTaskFailed(MsgWrongProduct);
							// }
						}
						break;

					case ModeratorStep.WaitForNextTask:

						if (this.stepTimer.IsTimePassed((int)this.step, 5000) && !this.tool.IsSpeaking())
						{
							if (!this.tool.IsPlaybackFinished()) { break; }

							SceneManager.LoadScene(SceneManager.GetActiveScene().name);
						}

						break;

					case ModeratorStep.TakeItemGo:

						if (this.robotRouting.IsItemTaken())
						{
							this.graspedItem = TakeItem(this.itemThatRobotWants);
							this.cameraForGraspedObj.Capture(this.graspedItem);

							this.step++;
							UpdateRobotStatus();
							break;
						}
						break;

					case ModeratorStep.TakeItemBack:

						if (this.robotRouting.IsReturned())
						{
//							this.graspedItem = TakeItem(this.itemThatRobotWants);

							this.itemThatRobotWants = string.Empty;
							this.step = ModeratorStep.InConversation;
							UpdateRobotStatus();
							break;
						}
						break;
				}

				// Enable Customer Buttons (Confirm only the Yes button instead of all buttons)
				if(this.customerYesButton.interactable == false && this.step==ModeratorStep.InConversation && !this.tool.IsSpeaking())
				{
					EnableCustomerButtons();
				}

				// Disable Customer Buttons (Confirm only the Yes button instead of all buttons)
				if(this.customerYesButton.interactable == true && (this.step!=ModeratorStep.InConversation || this.tool.IsSpeaking()))
				{
					DisableCustomerButtons();
				}
			}
			catch (Exception exception)
			{
				Debug.LogError(exception);
				SIGVerseLogger.Error(exception.Message);
				SIGVerseLogger.Error(exception.StackTrace);
				this.ApplicationQuitAfter1sec();
			}
		}

		private void ApplicationQuitAfter1sec()
		{
			Thread.Sleep(1000);
			Application.Quit();
		}


		private void GoToNextTaskTaskSucceeded()
		{
			this.GoToNextTask(MsgTaskSucceeded, string.Empty);
		}

		private void GoToNextTaskTaskFailed(string detail)
		{
			this.GoToNextTask(MsgTaskFailed, detail);
		}

		private void GoToNextTask(string message, string detail)
		{
			this.tool.StopPlayback();

			this.scoreManager.TaskEnd();

			this.SendRosMessage(message, detail);

			this.PostProcess();
		}

		private void UpdateRobotStatus()
		{
			string graspedItemName = this.graspedItem==null ? string.Empty : this.graspedItem.name;

			SIGVerseLogger.Info("UpdateRobotState RobotState=" + GetRobotState().ToString()+", GraspedItem="+graspedItemName);

			this.robotStateText.text = GetRobotState().ToString();
			this.graspedItemText.text = graspedItemName==string.Empty ? "-" : graspedItemName;

			this.icsPubRobotStatus.SendRobotStatus(GetRobotState(), this.tool.IsSpeaking(), graspedItemName);
		}

		private RobotState GetRobotState()
		{
			if(this.step<=ModeratorStep.WaitForIamReady)
			{
				return RobotState.Standby;
			}
			else if(this.step<=ModeratorStep.WaitForNextTask)
			{
				return RobotState.InConversation;
			}
			else
			{
				return RobotState.Moving;
			}
		}

		private void SendRosMessage(string message, string detail)
		{
			Debug.Log("SendRosMessage: "+message+","+detail);

			ExecuteEvents.Execute<IRosConversationSendHandler>
			(
				target: this.robotRosBridgeScripts, 
				eventData: null, 
				functor: (reciever, eventData) => reciever.OnSendRosMessage(message, detail)
			);
		}

		private void SendPanelNotice(string message, int fontSize, Color color, bool shouldSendToPlaybackManager = true)
		{
			PanelNoticeStatus noticeStatus = new PanelNoticeStatus(message, fontSize, color, 2.0f);

			// For changing the notice of a panel
			ExecuteEvents.Execute<IPanelNoticeHandler>
			(
				target: this.mainMenu, 
				eventData: null, 
				functor: (reciever, eventData) => reciever.OnPanelNoticeChange(noticeStatus)
			);

			if(shouldSendToPlaybackManager)
			{
				// For recording
				ExecuteEvents.Execute<IPanelNoticeHandler>
				(
					target: this.playbackManager,
					eventData: null,
					functor: (reciever, eventData) => reciever.OnPanelNoticeChange(noticeStatus)
				);
			}

			this.lastPanelMessage = message;
		}

		private void EnableCollision(Transform item)
		{
			item.GetComponent<Rigidbody>().isKinematic = false;
			List<Collider> colliders = item.GetComponentsInChildren<Collider>().ToList();

			foreach (Collider collider in colliders)
			{
				collider.enabled = true;
			}
		}

		public IEnumerator DisplayEndMessage()
		{
			yield return new WaitForSecondsRealtime(7);

			string endMessage = "All sessions have ended";

			SIGVerseLogger.Info(endMessage);
			this.tool.AddSpeechQueModerator(endMessage);

			string panelMessage = endMessage + "\n"
				+"(" + SIGVerseUtils.GetOrdinal(IcsConfig.Instance.numberOfTrials) + ": " + this.lastPanelMessage.Replace("\n", " - ") + ")";

			this.SendPanelNotice(panelMessage, 80, PanelNoticeStatus.Blue, false);

			this.bgmAudioSource.enabled = false;
		}


		public void OnReceiveRosMessage(RosBridge.interactive_customer_service.Conversation conversationMsg)
		{
			if(this.receivedMessageMap.ContainsKey(conversationMsg.type))
			{
				// Check message order
				if(conversationMsg.type==MsgIamReady)
				{
					if(this.step!=ModeratorStep.WaitForIamReady) { SIGVerseLogger.Warn("Bad timing. message : " + conversationMsg.type + ", step="+this.step); return; }
				}

				if(conversationMsg.type==MsgRobotMessage)
				{
					if(ExistsUnreadRobotMessage() || GetRobotState()!=RobotState.InConversation || this.tool.IsSpeaking())
					{
						SIGVerseLogger.Warn("Bad timing. message : " + conversationMsg.type + ", ExistsUnreadRobotMessage=" + ExistsUnreadRobotMessage());//+", Speaking="+this.tool.IsSpeaking());
						SendRosMessage(MsgRobotMsgFailed, MsgBadTiming+":"+conversationMsg.detail);
						return; 
					}
					else
					{
						this.robotMessage = conversationMsg.detail;
					}
				}

				if(conversationMsg.type==MsgTakeItem)
				{
					if(ExistsUnreadRobotMessage() || GetRobotState()!=RobotState.InConversation || this.tool.IsSpeaking())
					{ 
						SIGVerseLogger.Warn("Bad timing. message : " + conversationMsg.type + ", step="+this.step+", ExistsUnreadRobotMessage="+ExistsUnreadRobotMessage());
						SendRosMessage(MsgTakeItemFailed, MsgBadTiming+":"+conversationMsg.detail);
						return; 
					}
					else if(!CanTakeItem(conversationMsg.detail))
					{
						SIGVerseLogger.Warn("Item not found. message : " + conversationMsg.type + "," + conversationMsg.detail+ ", step="+this.step);
						SendRosMessage(MsgTakeItemFailed, MsgItemNotFound+":"+conversationMsg.detail);
						return; 
					}
					else
					{
						this.itemThatRobotWants = conversationMsg.detail;
					}
				}

				if(conversationMsg.type==MsgGiveItem)
				{
					if(ExistsUnreadRobotMessage() || GetRobotState()!=RobotState.InConversation || this.tool.IsSpeaking())
					{
						SIGVerseLogger.Warn("Bad timing. message : " + conversationMsg.type + ", step="+this.step+", ExistsUnreadRobotMessage="+ExistsUnreadRobotMessage());
						SendRosMessage(MsgGiveItemFailed, MsgBadTiming);
						return; 
					}
					else if(!CanGiveItem())
					{
						SIGVerseLogger.Warn("Robot has nothing. message : " + conversationMsg.type + ", step="+this.step);
						SendRosMessage(MsgGiveItemFailed, MsgYouHaveNothing);
						return; 
					}
				}

				if(conversationMsg.type==MsgGiveUp)
				{
					this.OnGiveUp();
				}

				this.receivedMessageMap[conversationMsg.type] = true;
			}
			else
			{
				SIGVerseLogger.Warn("Received Illegal Conversation message. type=" + conversationMsg.type+", detail="+conversationMsg.detail);
			}
		}

		public void OnCustomerYesButtonClick()
		{
			if(this.customerButtonMsg!=string.Empty) //this.step!=ModeratorStep.RobotIsInFrontOfMe || 
			{
				SIGVerseLogger.Warn("Bad timing. Button Click: "+MsgYes);
				return; 
			}
			
			SIGVerseLogger.Info("[Yes] button clicked");

			this.customerButtonMsg = MsgYes;
		}

		public void OnCustomerNoButtonClick()
		{
			if(this.customerButtonMsg!=string.Empty) //this.step!=ModeratorStep.RobotIsInFrontOfMe || 
			{
				SIGVerseLogger.Warn("Bad timing. Button Click: "+MsgNo);
				return; 
			}
			
			SIGVerseLogger.Info("[No] button clicked");
			
			this.customerButtonMsg = MsgNo;
		}

		public void OnCustomerIdontKnowButtonClick()
		{
			if(this.customerButtonMsg!=string.Empty) //this.step!=ModeratorStep.RobotIsInFrontOfMe || 
			{
				SIGVerseLogger.Warn("Bad timing. Button Click: "+MsgIdontKnow);
				return; 
			}
			
			SIGVerseLogger.Info("[I don't know] button clicked");
			
			this.customerButtonMsg = MsgIdontKnow;
		}

		private void EnableCustomerButtons()
		{
			this.customerYesButton      .interactable = true;
			this.customerNoButton       .interactable = true;
			this.customerIdontKnowButton.interactable = true;
		}

		private void DisableCustomerButtons()
		{
			this.customerYesButton      .interactable = false;
			this.customerNoButton       .interactable = false;
			this.customerIdontKnowButton.interactable = false;
		}

		private bool ExistsUnreadRobotMessage()
		{
			// return this.receivedMessageMap[MsgRobotMessage] || this.receivedMessageMap[MsgTakeItem] || this.receivedMessageMap[MsgGiveItem];
			return false;
		}

		private GameObject TakeItem(string productName)
		{
			return this.tool.GetGraspables().FirstOrDefault(graspable => graspable.name == productName);
		}

		private bool CanTakeItem(string productName)
		{
			return TakeItem(productName) != null;
		}

		private bool CanGiveItem()
		{
			return this.graspedItem != null;
		}

		public void OnTimeIsUp()
		{
			this.interruptedReason = ReasonTimeIsUp;
		}

		public void OnGiveUp()
		{
			if((this.step > ModeratorStep.TaskStart && this.step < ModeratorStep.WaitForNextTask) || this.step==ModeratorStep.TakeItemGo || this.step==ModeratorStep.TakeItemBack)
			{
				this.interruptedReason = ReasonGiveUp;
			}
			else
			{
				SIGVerseLogger.Warn("It is a timing not allowed to give up.");
			}
		}
	}
}

