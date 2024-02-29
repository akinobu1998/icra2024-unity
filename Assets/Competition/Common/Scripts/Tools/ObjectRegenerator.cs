using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static SIGVerse.FCSC.Common.ObjectGenerator;

#if UNITY_EDITOR
using UnityEditor;
using Unity.EditorCoroutines.Editor;
#endif

namespace SIGVerse.FCSC.Common
{
	public class ObjectRegenerator : MonoBehaviour
	{
		public ObjectManager objectManager;

		public bool overwriteGenerateAllObjectsSettings = false;
		public bool generateAllObjectsAsStaticObjects = false;
		public bool generateAllObjectsAsDynamicObjects = false;

		private void Reset()
		{
			this.enabled = false;
		}
		private void Start()
		{
			// Do not remove this method to enable ObjectRegeneratorEditor functionality.
		}

#if UNITY_EDITOR
		[CustomEditor(typeof(ObjectRegenerator))]
		public class ObjectRegeneratorEditor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				ObjectRegenerator objRegenerator = (ObjectRegenerator)target;

//				base.OnInspectorGUI();

				objRegenerator.objectManager = EditorGUILayout.ObjectField("ObjectManager", objRegenerator.objectManager, typeof(ObjectManager), true) as ObjectManager;
				objRegenerator.overwriteGenerateAllObjectsSettings = EditorGUILayout.Toggle("Overwrite Generating Settings", objRegenerator.overwriteGenerateAllObjectsSettings);

				EditorGUI.BeginDisabledGroup(!objRegenerator.overwriteGenerateAllObjectsSettings);
				objRegenerator.generateAllObjectsAsStaticObjects  = EditorGUILayout.Toggle("Generate All Objects as Static Objects",  objRegenerator.generateAllObjectsAsStaticObjects);
				objRegenerator.generateAllObjectsAsDynamicObjects = EditorGUILayout.Toggle("Generate All Objects as Dynamic Objects", objRegenerator.generateAllObjectsAsDynamicObjects);
				EditorGUI.EndDisabledGroup();

				GUILayout.Space(10);

				EditorGUILayout.BeginHorizontal();
				{
					GUILayout.FlexibleSpace();

					if(objRegenerator.enabled)
					{
						if (GUILayout.Button("Regenerate all goods", GUILayout.Width(150), GUILayout.Height(30)))
						{
							Undo.RecordObject(target, "Regenerate all goods");

							this.Regenerate(objRegenerator);

							EditorCoroutineUtility.StartCoroutine(UpdateGraspableScripts(), this);
						}
					}
					else
					{
						GUILayout.Label("If you want to regenerate, please activate this script first.");
					}
					GUILayout.FlexibleSpace();
				}
				EditorGUILayout.EndHorizontal();
			}

			private void Regenerate(ObjectRegenerator objRegenerator)
			{
				List<ObjectGenerator> objGenerators = objRegenerator.GetComponentsInChildren<ObjectGenerator>().ToList();

				Debug.Log("ObjectGeneratorEditor Num="+objGenerators.Count);

				foreach (ObjectGenerator objGenerator in objGenerators)
				{
					if(objRegenerator.overwriteGenerateAllObjectsSettings)
					{
						objGenerator.generateAllObjectsAsStaticObjects  = objRegenerator.generateAllObjectsAsStaticObjects;  // Overwrite
						objGenerator.generateAllObjectsAsDynamicObjects = objRegenerator.generateAllObjectsAsDynamicObjects; // Overwrite
					}

					ObjectGeneratorEditor.Generate(objGenerator);
				}

				Debug.Log("Finished Regenerate");
				objRegenerator.enabled = false;
			}

			private IEnumerator UpdateGraspableScripts()
			{
				Selection.activeGameObject = ((ObjectRegenerator)target).objectManager.gameObject;

				yield return null; // Wait for activating

				ObjectManagerEditor[] editors = (ObjectManagerEditor[])Resources.FindObjectsOfTypeAll(typeof(ObjectManagerEditor));

				if (editors.Length!=1)
				{
					Debug.LogError("Number of ObjectManagerEditor!=1. Can not UpdateGraspableScripts in ObjectManager.");
				}
				else
				{
					editors[0].UpdateObjectList();
				}
			}
		}
#endif
	}
}

