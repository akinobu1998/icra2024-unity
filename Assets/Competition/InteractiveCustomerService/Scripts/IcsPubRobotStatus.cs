using UnityEngine.EventSystems;
using SIGVerse.Common;
using SIGVerse.RosBridge;
using UnityEngine;
using System.Threading;
using SIGVerse.RosBridge.interactive_customer_service;
using System;

namespace SIGVerse.FCSC.InteractiveCustomerService
{
	//public interface IRosRobotStatusSendHandler : IEventSystemHandler
	//{
	//	void OnSendRosMessage(string state, string graspedItem);
	//}

	public enum RobotState
	{
		Standby,
		InConversation,
		Moving,
	}

	public class IcsPubRobotStatus : RosPubMessage<RosBridge.interactive_customer_service.RobotStatus>//, IRosRobotStatusSendHandler
	{
		private const string StateStandby        = "standby";
		private const string StateInConversation = "in_conversation";
		private const string StateMoving         = "moving";

		public bool logSendMessage = true;

		protected override void Start()
		{
			base.Start();

#if !UNITY_EDITOR
			this.logSendMessage = true;
#endif
		}

		public void SendRobotStatus(RobotState robotState, bool isSpeaking, string graspedItem)
		{
			string state;

			switch (robotState)
			{
				case RobotState.Standby:        { state = StateStandby;        break; }
				case RobotState.InConversation: { state = StateInConversation; break; }
				case RobotState.Moving:         { state = StateMoving;         break; }
				default:
				{
					throw new Exception("Illegal RobotState:" + robotState);
				}
			}

			OnSendRosMessage(state, isSpeaking, graspedItem);
		}

		//public void UpdateAndSendRobotSpeaking(bool isSpeaking)
		//{
		//	SendRobotStatus(isSpeaking);
		//}

		//public void SendRobotStatus(bool isSpeaking)
		//{
		//	OnSendRosMessage(this.state, isSpeaking, this.graspedItem);
		//}

		private void OnSendRosMessage(string state, bool speaking, string graspedItem)
		{
			if(this.logSendMessage)
			{
				SIGVerseLogger.Info("Sending the robot status message: state=" + state + ", speaking="+speaking+"+, graspedItem=" + graspedItem);
			}

			RobotStatus robotStatusMsg = new RobotStatus();
			robotStatusMsg.state        = state;
			robotStatusMsg.speaking     = speaking;
			robotStatusMsg.grasped_item = graspedItem;

			this.publisher.Publish(robotStatusMsg);
		}
	}
}

