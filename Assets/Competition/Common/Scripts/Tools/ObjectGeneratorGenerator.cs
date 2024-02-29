using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SIGVerse.Common;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SIGVerse.FCSC.Common
{
	public class ObjectGeneratorGenerator : MonoBehaviour
	{
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(ObjectGeneratorGenerator))]
	public class ObjectGeneratorGeneratorEditor : Editor
	{
		private const string OutputFolderName = "Output";

		public override void OnInspectorGUI()
		{
			ObjectGeneratorGenerator generator = (ObjectGeneratorGenerator)target;

			base.OnInspectorGUI();

			EditorGUILayout.LabelField("Generate ObjectGenerator.");
			EditorGUILayout.LabelField("Please place the prefab of the 3D models as child objects of this object.");

			GUILayout.Space(10);

			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.FlexibleSpace();

				if (GUILayout.Button("Generate ObjectGenerator", GUILayout.Width(250), GUILayout.Height(40)))
				{
					Undo.RecordObject(target, "Generate ObjectGenerator");

					Transform oldFolder = generator.transform.Find(OutputFolderName);

					// Delete old folder
					if(oldFolder!=null)
					{
						Undo.DestroyObjectImmediate(oldFolder.gameObject);
					}
					
					Debug.Log("Objects count=" + generator.transform.childCount);

					// Create new folder
					GameObject newFolder = new GameObject(OutputFolderName);
					newFolder.transform.parent = generator.transform;
					newFolder.transform.SetSiblingIndex(0);


					foreach(Transform child in generator.transform)
					{
						if (child.name==OutputFolderName) { continue; }

						GameObject childFolder = new GameObject(child.name);
						childFolder.transform.parent = newFolder.transform;

						ObjectGenerator objectGenerator = childFolder.AddComponent<ObjectGenerator>();
						objectGenerator.objPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(child.gameObject));
					}
				}
				GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndHorizontal();
		}
	}
#endif
}
