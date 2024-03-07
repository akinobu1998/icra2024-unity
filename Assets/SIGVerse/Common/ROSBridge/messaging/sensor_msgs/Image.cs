// Generated by gencs from sensor_msgs/Image.msg
// DO NOT EDIT THIS FILE BY HAND!

using System;
using System.Collections;
using System.Collections.Generic;
using SIGVerse.RosBridge;
using UnityEngine;

using SIGVerse.RosBridge.std_msgs;

namespace SIGVerse.RosBridge 
{
	namespace sensor_msgs 
	{
		[System.Serializable]
		public class Image : RosMessage
		{
			public std_msgs.Header header;
			public System.UInt32 height;
			public System.UInt32 width;
			public string encoding;
			public byte is_bigendian;
			public System.UInt32 step;
			//public System.Collections.Generic.List<byte>  data;
			public byte[] data;


			public Image()
			{
				this.header = new std_msgs.Header();
				this.height = 0;
				this.width = 0;
				this.encoding = "";
				this.is_bigendian = 0;
				this.step = 0;
				//this.data = new System.Collections.Generic.List<byte>();
				this.data = null;
			}

//			public Image(std_msgs.Header header, System.UInt32 height, System.UInt32 width, string encoding, byte is_bigendian, System.UInt32 step, System.Collections.Generic.List<byte>  data)
			public Image(std_msgs.Header header, System.UInt32 height, System.UInt32 width, string encoding, byte is_bigendian, System.UInt32 step, byte[]  data)
			{
				this.header = header;
				this.height = height;
				this.width = width;
				this.encoding = encoding;
				this.is_bigendian = is_bigendian;
				this.step = step;
				this.data = data;
			}

			new public static string GetMessageType()
			{
				return "sensor_msgs/Image";
			}

			new public static string GetMD5Hash()
			{
				return "060021388200f6f0f447d0fcd9c64743";
			}
		} // class Image
	} // namespace sensor_msgs
} // namespace SIGVerse.ROSBridge

