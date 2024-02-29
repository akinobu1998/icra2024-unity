using UnityEngine.EventSystems;
using SIGVerse.Common;
using SIGVerse.RosBridge;

namespace SIGVerse.FCSC.InteractiveCustomerService
{
	public interface IRosConversationSendHandler : IEventSystemHandler
	{
		void OnSendRosMessage(string type, string detail);
	}

	public class IcsPubConversation : RosPubMessage<RosBridge.interactive_customer_service.Conversation>, IRosConversationSendHandler
	{
		public void OnSendRosMessage(string type, string detail)
		{
			SIGVerseLogger.Info("Sending Conversation message: type=" + type + ", detail=" + detail);

			RosBridge.interactive_customer_service.Conversation conversationMsg = new RosBridge.interactive_customer_service.Conversation();
			conversationMsg.type   = type;
			conversationMsg.detail = detail;

			this.publisher.Publish(conversationMsg);
		}
	}
}

