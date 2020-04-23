using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using SIGVerse.Common;

#if SIGVERSE_PUN
using Photon.Pun;
#endif

namespace SIGVerse.ExampleScenes.Hsr.HsrCleanupVR
{
	public interface IChatRegistrationHandler : IEventSystemHandler
	{
		void OnAddChatUser(string userName);
		void OnRemoveChatUser(string userName);
	}

	public interface IChatMessageHandler : IEventSystemHandler
	{
		void OnReceiveChatMessage(string senderName, string message);
	}

#if SIGVERSE_PUN

	public class ChatManager : MonoBehaviour, IChatRegistrationHandler, IChatMessageHandler
	{

		public const string ChatManagerName = "ChatManager";
		public const string MainMenuName    = "MainMenu";

		//-----------------------------

		private PhotonView photonView;

		private GameObject mainMenu;

		private Dictionary<string, GameObject> userMap;

		void Awake()
		{
			this.mainMenu = GameObject.Find(ChatManager.MainMenuName);

			if (this.mainMenu == null)
			{
				SIGVerseLogger.Warn("Could not find MainMenu.");
			}
		}

		void Start()
		{
			this.photonView = this.GetComponent<PhotonView>();

			this.ClearChatUserList();
		}

		public void OnReceiveChatMessage(string senderName, string message)
		{
			SIGVerseLogger.Info("Receive ChatMessage on ChatManager. user=" + senderName + ", message=" + message);

			this.photonView.RPC("ForwardMessage", RpcTarget.All, senderName, message);
		}

		[PunRPC]
		private void ForwardMessage(string senderName, string message)
		{
			SIGVerseLogger.Info("ForwardMessage userName=" + senderName + ", message=" + message);

			// Forward the message 
			foreach (KeyValuePair<string, GameObject> user in this.userMap)
			{
				// Publish a message to logged-in user objects.
				ExecuteEvents.Execute<IChatMessageHandler>
				(
					target: user.Value,
					eventData: null,
					functor: (reciever, eventData) => reciever.OnReceiveChatMessage(senderName, message)
				);
			}
		}

		public void OnAddChatUser(string userName)
		{
			this.photonView.RPC("AddChatUser", RpcTarget.AllBuffered, userName);
		}

		[PunRPC]
		private void AddChatUser(string userName)
		{
			// Wait for GameObject creation
			StartCoroutine(AddChatUserAfter3sec(userName));
		}

		private IEnumerator AddChatUserAfter3sec(string userName)
		{
			yield return new WaitForSeconds(3.0f);

			SIGVerseLogger.Info("AddChatUser name="+userName);

			this.userMap.Add(userName, GameObject.Find(userName));
		}

		public void OnRemoveChatUser(string userName)
		{
			this.photonView.RPC("RemoveChatUser", RpcTarget.AllBuffered, userName);
		}

		[PunRPC]
		private void RemoveChatUser(string userName)
		{
			this.userMap.Remove(userName);
		}

		public void ClearChatUserList()
		{
			this.userMap = new Dictionary<string, GameObject>();
		}
	}
#endif
}

