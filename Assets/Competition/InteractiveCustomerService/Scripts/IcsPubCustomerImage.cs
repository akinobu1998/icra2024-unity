using UnityEngine;
using System;
using System.Collections;
using SIGVerse.Common;
using SIGVerse.RosBridge;
using SIGVerse.RosBridge.sensor_msgs;
using SIGVerse.RosBridge.std_msgs;

namespace SIGVerse.FCSC.InteractiveCustomerService
{
	public class IcsPubCustomerImage : RosPubMessage<RosBridge.sensor_msgs.Image>
	{
		//--------------------------------------------------
		private Header header;
		private Image imageData;

		protected override void Start()
		{
			base.Start();

			this.header = new Header(0, new SIGVerse.RosBridge.msg_helpers.Time(0, 0), "");

			this.imageData = new Image(null, 0, 0, "rgb8", 0, 0, null);
		}

		public void PubImage(Texture2D flippedTaskImage)
		{
			if(!this.IsConnected()) 
			{
				SIGVerseLogger.Warn("IcsCustomerImagePublisher is NOT connected");
				return; 
			}

			this.header.Update();

			this.imageData.header = this.header;
			this.imageData.height = (uint)flippedTaskImage.height;
			this.imageData.width  = (uint)flippedTaskImage.width;
			this.imageData.step = this.imageData.width * 3;
			this.imageData.data = flippedTaskImage.GetRawTextureData();

			this.publisher.Publish(this.imageData);

			SIGVerseLogger.Info("Send the customer image.");
		}
	}
}
