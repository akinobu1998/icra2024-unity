using UnityEngine;
using UnityEngine.EventSystems;
using SIGVerse.Common;
using SIGVerse.RosBridge;
using System.Collections.Generic;

namespace SIGVerse.FCSC.InteractiveCustomerService
{
	public interface IRosConversationReceiveHandler : IEventSystemHandler
	{
		void OnReceiveRosMessage(RosBridge.interactive_customer_service.Conversation conversationMsg);
	}

	public class IcsSubConversation : RosSubMessage<RosBridge.interactive_customer_service.Conversation>
	{
		public List<GameObject> destinations;

		protected override void SubscribeMessageCallback(RosBridge.interactive_customer_service.Conversation conversationMsg)
		{
			SIGVerseLogger.Info("Received Conversation message: type="+conversationMsg.type+", detail="+conversationMsg.detail);

			foreach(GameObject destination in this.destinations)
			{
				ExecuteEvents.Execute<IRosConversationReceiveHandler>
				(
					target: destination,
					eventData: null,
					functor: (reciever, eventData) => reciever.OnReceiveRosMessage(conversationMsg)
				);
			}
		}
	}
}
