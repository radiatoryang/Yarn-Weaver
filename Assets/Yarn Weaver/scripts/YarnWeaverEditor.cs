using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Yarn.Unity;

namespace YarnWeaver {

	public class YarnWeaverEditor : MonoBehaviour {

		public static YarnWeaverEditor instance;

		DialogueRunner dialogueRunner;
		YarnValidator validator;

		Rect screenRect { get { return new Rect( Screen.width / 2 + 16, 48, Screen.width / 2 - 32, Screen.height - 136 ); } }
		Vector2 scrollPos;
		[SerializeField] GUISkin guiSkin;

		[SerializeField] YarnWeaverNode nodePrefab;
		List<YarnWeaverNode> allNodes = new List<YarnWeaverNode>();
		Dictionary<string, YarnWeaverNode> titleToNode = new Dictionary<string, YarnWeaverNode>();

		[SerializeField] RectTransform nodeFrameReference, nodeMapBG;

		YarnWeaverNode currentNode;
		bool loadedText = false;
		string textContent = "hello hello\nhello hellso\nhello hello\nhello hello\nhsello hello\nhello hello\nhello hello\nhello hello\nhellof hello\nhello hello\nhello hello\nhello helalo\nhello hello\nhello hello242\nh2424ello hello\nhello hello\nhello hello\nhello hello\nhello hello\nhello hello\nhello hello\nhello hello\n\"hello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\n\"hello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\n\"hello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\nhello hello\\n";

		void Awake () {
			instance = this;

			dialogueRunner = FindObjectOfType<DialogueRunner>();
			nodePrefab.gameObject.SetActive( false );
		}
		
		// Update is called once per frame
		void Update () {
		//	if (dialogueRunner.is
			if( dialogueRunner.isDialogueRunning && dialogueRunner.dialogue != null && !loadedText ) {
				loadedText = true;
				LoadAllData();

				//textContent = YarnWeaverMain.currentText;
				//textContent = dialogueRunner.

//				var nodeInfo = YarnWeaverLoader.GetNodesFromText( YarnWeaverMain.currentText, YarnWeaverLoader.GetFormatFromFileName( YarnWeaverMain.currentFilePath ) );
//				foreach( var info in nodeInfo ) {
//					Debug.Log( info.title );
//					Debug.Log( info.body );
//					textContent = info.body;
//					Debug.Log( new Vector2( info.position.x, info.position.y ).ToString() );
//				}


//				var strings = dialogueRunner.dialogue.GetStringTable();
//				foreach( var kvp in strings ) {
//					Debug.Log( kvp.Key + ": " + kvp.Value );
//				}
					
			}
		}

		public void SetCurrentNodePlaying( ) {
			SetCurrentNode( titleToNode[dialogueRunner.currentNodeName] );
		}

		public void SetCurrentNode( YarnWeaverNode newCurrentNode ) {
			currentNode = newCurrentNode;
			textContent = currentNode.nodeBody;
			nodeMapBG.anchoredPosition = new Vector2( nodeFrameReference.rect.width / 2 - currentNode.nodePos.x, nodeFrameReference.rect.height / 2 - currentNode.nodePos.y );
		}

		void LoadAllData () {
			var nodeInfos = YarnWeaverLoader.GetNodesFromText( YarnWeaverMain.currentText, YarnWeaverLoader.GetFormatFromFileName( YarnWeaverMain.currentFilePath ) );
//			foreach( var info in nodeInfo ) {
//				Debug.Log( info.title );
//				Debug.Log( info.body );
//				textContent = info.body;
//				Debug.Log( new Vector2( info.position.x, info.position.y ).ToString() );
//			}
			foreach( var node in nodeInfos ) {
				var newNodePanel = (YarnWeaverNode)Instantiate( nodePrefab, new Vector3( node.position.x, node.position.y, 0f ), Quaternion.identity, nodePrefab.transform.parent );
				newNodePanel.SetData( allNodes.Count, node.title, node.body, Color.blue, new Vector2( node.position.x, node.position.y ) );
				newNodePanel.gameObject.SetActive( true );
				allNodes.Add( newNodePanel );
				titleToNode.Add( node.title, newNodePanel );
			}
		}

		void OnGUI () {
			GUI.skin = guiSkin;
			GUILayout.BeginArea( screenRect );
			scrollPos = GUILayout.BeginScrollView( scrollPos );
			GUILayout.BeginHorizontal();
			GUILayout.Label( "00\n01\n02\n03\n04\n05" );
			textContent = GUILayout.TextArea( textContent );
			GUILayout.EndHorizontal();
			GUILayout.EndScrollView();
			GUILayout.EndArea();
		}
	}
}
