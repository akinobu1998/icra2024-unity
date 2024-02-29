using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using System.Linq;

namespace SIGVerse.FCSC.InteractiveCustomerService
{
	[InitializeOnLoad]
	public class ConfigInitializer
	{
		static ConfigInitializer()
		{
			FileInfo configFileInfo = new FileInfo(Application.dataPath + IcsConfig.FolderPath + "sample/" + IcsConfig.ConfigFileName);

			if(!configFileInfo.Exists) { return; }

			DirectoryInfo sampleDirectoryInfo = new DirectoryInfo(Application.dataPath + IcsConfig.FolderPath + "sample/");

			foreach (FileInfo fileInfo in sampleDirectoryInfo.GetFiles().Where(fileinfo => fileinfo.Name != ".gitignore"))
			{
				string destFilePath = Application.dataPath + IcsConfig.FolderPath + fileInfo.Name;

				if (!File.Exists(destFilePath))
				{
					fileInfo.CopyTo(destFilePath);
				}
			}
		}
	}
}

