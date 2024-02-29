using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Remoting;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SIGVerse.FCSC.Common
{
	public class ObjectGenerator : MonoBehaviour
	{
		private const string TagGraspable = "Graspable";

		public GameObject objPrefab;
		[Range(1, 10)] public int xNum = 1;
		[Range(1, 5)] public int yNum = 1;
		[Range(1, 10)] public int zNum = 1;
		[TooltipAttribute("[m]")]
		public float interval = 0.02f;
		public bool withDeletion = true;
		public bool generateAllObjectsAsStaticObjects = false;
		public bool generateAllObjectsAsDynamicObjects = false;

		[HeaderAttribute("Hierarchy")]
		public string parentStartWith = "Goods";
		public string grandparentStartWith = "MeshRack_";

	#if UNITY_EDITOR
		[CustomEditor(typeof(ObjectGenerator))]
		[CanEditMultipleObjects]
		public class ObjectGeneratorEditor : Editor
		{
			public override void OnInspectorGUI()
			{
				ObjectGenerator objGenerator = (ObjectGenerator)target;

				base.OnInspectorGUI();

				GUILayout.Space(10);

				EditorGUILayout.BeginHorizontal();
				{
					GUILayout.FlexibleSpace();

					if (GUILayout.Button("Generate", GUILayout.Width(150), GUILayout.Height(30)))
					{
						Undo.RecordObject(target, "Generate");

						Generate(objGenerator);
					}

					GUILayout.FlexibleSpace();
				}
				EditorGUILayout.EndHorizontal();
			}

			public static void Generate(ObjectGenerator objGenerator)
			{
				if(objGenerator.withDeletion){ DeleteChildObjects(objGenerator); }

				Vector3 colSize;

				if(objGenerator.objPrefab.GetComponent<MeshCollider>()!=null)
				{
					colSize = objGenerator.objPrefab.GetComponent<MeshCollider>().sharedMesh.bounds.size;
				}
				else
				{
					colSize = objGenerator.objPrefab.GetComponent<BoxCollider>().size;
					Debug.Log("BoxCollider.size="+colSize);
				}

				string objName = CreateName(objGenerator);

				for(int k=0; k<objGenerator.zNum; k++)
				{
					for(int j=0; j<objGenerator.yNum; j++)
					{
						for(int i=0; i<objGenerator.xNum; i++)
						{
							int index = i + j*objGenerator.xNum + k*objGenerator.xNum*objGenerator.yNum;

							GameObject obj = PrefabUtility.InstantiatePrefab(objGenerator.objPrefab) as GameObject;
							obj.transform.parent = objGenerator.transform;
							obj.transform.localPosition = new Vector3(i*(colSize.x+objGenerator.interval), j*colSize.y, k*(colSize.z+objGenerator.interval));
							obj.transform.localRotation = Quaternion.Euler(0, 180, 0);
							obj.name = objName+"_"+index.ToString("000");

							if(objGenerator.objPrefab.isStatic || objGenerator.generateAllObjectsAsStaticObjects)
							{
								obj.isStatic = true;
								DestroyImmediate(obj.GetComponent<Rigidbody>());
							}
							else if(objGenerator.generateAllObjectsAsDynamicObjects)
							{
								obj.tag = TagGraspable;
							}
							else
							{
								if (k != 0)
								{
									ChangeToStatic(obj);

									if(j == objGenerator.yNum - 1)
									{
										obj.GetComponent<MeshRenderer>().scaleInLightmap = 2;
									}
								}
								else if (j != objGenerator.yNum - 1)
								{
									ChangeToStatic(obj);

									obj.GetComponent<MeshRenderer>().scaleInLightmap = 1.5f;
								}
								else
								{
									obj.tag = TagGraspable;

									obj.name = objGenerator.objPrefab.name;
									obj.GetComponent<Rigidbody>().isKinematic = true;
								}
							}
						}
					}
				}
			}

			private static void ChangeToStatic(GameObject obj)
			{
				obj.isStatic = true;

				DestroyImmediate(obj.GetComponent<Rigidbody>());

				if(obj.GetComponent<MeshCollider>()!=null)
				{
					DestroyImmediate(obj.GetComponent<MeshCollider>());
					obj.AddComponent<BoxCollider>();
				}
			}

			private static void DeleteChildObjects(ObjectGenerator objGen)
			{
				Transform[] children = objGen.transform.GetComponentsInChildren<Transform>();

				foreach(Transform child in children)
				{
					if (child.gameObject == objGen.gameObject) continue;
					DestroyImmediate(child.gameObject);
				}
			}

			private static string CreateName(ObjectGenerator objGen)
			{
				Transform parent = objGen.transform.parent;
				Transform grandparent = parent.parent;

				return grandparent.name.TrimStart(objGen.grandparentStartWith.ToCharArray())
					+ parent.name.TrimStart(objGen.parentStartWith.ToCharArray())
					+ "-" + objGen.objPrefab.name;
			}
		}
	#endif
	}
}

