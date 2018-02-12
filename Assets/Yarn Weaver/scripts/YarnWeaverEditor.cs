using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Yarn.Unity;

namespace YarnWeaver {

	// for undo / redo
	public class YarnWeaverEditorHistoryState {
		public float stateChangeTimestamp;
		public string stateChangeDescription;
		public Dictionary<string, List<YarnWeaverLoader.NodeInfo>> fileToNodeInfo = new Dictionary<string, List<YarnWeaverLoader.NodeInfo>>();

		public static YarnWeaverEditorHistoryState CloneFrom( YarnWeaverEditorHistoryState sourceState ) {
			var newHistoryState = new YarnWeaverEditorHistoryState();

			// there's probably a better way to do this but I'm a shitty coder
			// goal: make new copies of all the state info, don't just pass references, so we can modify this newState without affecting the sourceState
			foreach( var kvp in sourceState.fileToNodeInfo ) {
				newHistoryState.fileToNodeInfo.Add( kvp.Key, kvp.Value.ToArray().ToList() );
			}

			return newHistoryState;
		}
	}

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
		Dictionary<string, List<YarnWeaverNode>> fileToNodeList = new Dictionary<string, List<YarnWeaverNode>>();

		float historyChangeTimestamp;
		string historyChangeDescription;
		int currentHistoryIndex = -1; // where in the history we are (in case we clicked undo)
		YarnWeaverEditorHistoryState currentHistoryState { get { return currentHistoryIndex > -1 && history.Count > 0 ? history[currentHistoryIndex] : null; } }
		List<YarnWeaverEditorHistoryState> history = new List<YarnWeaverEditorHistoryState>();

		[SerializeField] RectTransform nodeFrameReference, nodeMapBG;

		YarnWeaverNode currentNode;
		bool loadedText = false;
		string lineNumberText = "0000\n0001\n0002\n0003\n0004\n0005";
		Rect textEditorRect;
		string editTextHeader, editTextBody, startText = "Welcome to Yarn Weaver.\n- Click +NEW on the left to make a new Yarn node, and start writing!\n- When you want to test, click the PLAY button at the top.\nFor more help, click the (?) button in top-right.\n\ncurrently loaded files: ";
			
		void Awake () {
			instance = this;
			editTextBody = startText;

			dialogueRunner = FindObjectOfType<DialogueRunner>();
			nodePrefab.gameObject.SetActive( false );
		}
		
		// Update is called once per frame
		void Update () {
		//	if (dialogueRunner.is
//			if( dialogueRunner.isDialogueRunning && dialogueRunner.dialogue != null && !loadedText ) {
//				loadedText = true;
//				LoadAllData();

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
					
//			}

			if( loadedText ) {
				UpdateLineNumbers();
			}

			if( historyChangeTimestamp > 0f && Time.time - historyChangeTimestamp > 1f ) {
				CommitEditsToHistory();
			}
		}

		public void OnClickAddNewNode () {

		}

		public void SetCurrentNodePlaying( ) {
			SetCurrentNode( titleToNode[dialogueRunner.currentNodeName] );
		}

		public void SetCurrentNode( YarnWeaverNode newCurrentNode ) {
			currentNode = newCurrentNode;
			editTextBody = currentNode.nodeBody;
			nodeMapBG.anchoredPosition = new Vector2( nodeFrameReference.rect.width / 2 - currentNode.nodePos.x, nodeFrameReference.rect.height / 2 - currentNode.nodePos.y );

		}

		void UpdateLineNumbers () {
			// generate line numbers: to get good word wrap spacing for line numbers, the line numbers need an (invisible) copy of the entire node body text -- and then GUIStyle word wrap just takes care of it from there
			var lines = editTextBody.Split( new string[] {"\r\n","\n"}, System.StringSplitOptions.None );
			int charsPerLine = Mathf.FloorToInt( (screenRect.width - 78f) / (guiSkin.textArea.fontSize * 0.5f) );
			//			string startInvisible = " "; // "<color=#00000000>";
			//			string endInvisible = ""; //"</color>";
			Debug.Log( charsPerLine );
			for( int i = 0; i < lines.Length; i++ ) {
				//				lines[i] = i.ToString( "D4" ) + startInvisible + lines[i] + endInvisible;
				int lineBreaks = Mathf.FloorToInt( 1f * lines[i].Length / charsPerLine ); // calculate fixed-width line count
			//	Debug.Log( i.ToString() + ": " + lines[i].Length.ToString() + " / " + charsPerLine.ToString() );
				lines[i] = i.ToString().PadLeft(4, ' ') + "  ";
				for( int breaks = 0; breaks < lineBreaks; breaks++ ) {
					lines[i] += "\n";
				}
			}
			lineNumberText = string.Join( "\n", lines );
		}

		public void LoadAllDataInFile (string filepath, string filetext) {
//			var nodeInfos = YarnWeaverLoader.GetNodesFromText( YarnWeaverMain.currentText, YarnWeaverLoader.GetFormatFromFileName( YarnWeaverMain.currentFilePath ) );
			var nodeInfos = YarnWeaverLoader.GetNodesFromText( filetext, YarnWeaverLoader.GetFormatFromFileName( filepath ) );
//			foreach( var info in nodeInfo ) {
//				Debug.Log( info.title );
//				Debug.Log( info.body );
//				textContent = info.body;
//				Debug.Log( new Vector2( info.position.x, info.position.y ).ToString() );
//			}
			var nodesForThisFile = new List<YarnWeaverNode>();
			foreach( var node in nodeInfos ) {
				var newNodePanel = (YarnWeaverNode)Instantiate( nodePrefab, new Vector3( node.position.x, node.position.y, 0f ), Quaternion.identity, nodePrefab.transform.parent );
				newNodePanel.SetData( allNodes.Count, node.title, node.body, Color.blue, new Vector2( node.position.x, node.position.y ) );
				newNodePanel.gameObject.SetActive( true );
				allNodes.Add( newNodePanel );
				titleToNode.Add( node.title, newNodePanel );
				nodesForThisFile.Add( newNodePanel );
			}

			if( fileToNodeList.ContainsKey( filepath ) == false ) {
				fileToNodeList.Add( filepath, nodesForThisFile );
			} else {
				fileToNodeList[filepath] = nodesForThisFile;
			}

			loadedText = true;
		}

		public void SaveAllFiles () {
			foreach( var kvp in fileToNodeList ) {
				SaveFile( kvp.Key );
			}
		}

		public void SaveFile( string filepath ) {

			if( currentHistoryState.fileToNodeInfo.ContainsKey( filepath ) == false ) {
				// nothing to save!
				// TODO: save as an empty file?
				Debug.Log("no nodes to save for " + filepath); // TODO: move to player console
			} else {
				// ripped from YarnSpinnerConsole/LineAdder.cs https://github.com/thesecretlab/YarnSpinner/blob/master/YarnSpinnerConsole/LineAdder.cs
				var format = YarnWeaverLoader.GetFormatFromFileName(filepath);

				switch (format)
				{
				case NodeFormat.JSON:
					break;
				case NodeFormat.Text:
					break;
				default:
					Debug.LogError( "SaveFile(): I don't know this format for " + filepath ); // TODO: move to player console
					break;
				}

				// Convert the nodes into the correct format
				var savedData = YarnWeaverFileFormatConverter.ConvertNodes( currentHistoryState.fileToNodeInfo[filepath], format);

				// Write the file!
				using (var writer = new System.IO.StreamWriter(filepath))
				{
					writer.Write(savedData);
				}
			}
		}

		public void CommitEditsToHistory () {
			// write new state, save to history stack
			var newState = YarnWeaverEditorHistoryState.CloneFrom( currentHistoryState );
			newState.stateChangeTimestamp = historyChangeTimestamp;
			newState.stateChangeDescription = historyChangeDescription;
			history.Add( newState );

			// reset vars
			historyChangeTimestamp = -1f;
			historyChangeDescription = "";

			// REFRESH NODE DISPLAY
			RefreshAllDisplay();
		}

		void RefreshAllDisplay () {

			// TODO: refresh node map with new NodeInfo?
			// TODO: how to preserve currentNode reference?
			// TODO: rely on internal nodeID instead of nodeNames?
			1. don't disable?

			// TODO: refresh lines in nodemap for connections

			// refresh text editor
			editTextHeader = currentNode.nodeTitle;
			editTextBody = currentNode.nodeBody;
		}

		void OnGUI () {
			if( !loadedText ) {
				return;
			}

			GUI.skin = guiSkin;
			GUI.changed = false;

			GUILayout.BeginArea( screenRect );
			GUILayout.BeginVertical();
			editTextHeader = GUILayout.TextField( editTextHeader );
			if( GUI.changed ) {
				historyChangeTimestamp = Time.time;
				historyChangeDescription = "edit Node Title on " + editTextHeader;
			}
			scrollPos = GUILayout.BeginScrollView( scrollPos );
			GUI.changed = false; // change this to false because we want to ignore scrollPos effect on GUI.changed
			GUILayout.BeginHorizontal();
			GUILayout.Label( lineNumberText, GUILayout.Width(48) ); // line numbers
			Rect lineNumberRect = GUILayoutUtility.GetLastRect();
			editTextBody = GUILayout.TextArea( editTextBody );

			textEditorRect = GUILayoutUtility.GetLastRect();
			GUILayout.EndHorizontal();

			if( GUI.changed ) {
				historyChangeTimestamp = Time.time;
				historyChangeDescription = "edit Node Body on " + editTextHeader;
			}

			// TODO: generate syntax highlighting here (overlay or underlay a manually-drawn GUI.Label with duplicate contents, except with rich text formatting)

//			var finalLineNumberRect = textEditorRect;
//			finalLineNumberRect.x = lineNumberRect.x;
//			finalLineNumberRect.width += lineNumberRect.width - 32;
//			GUI.Label( finalLineNumberRect, lineNumberText ); // actually draw line numbers now

			GUILayout.EndScrollView();

			GUILayout.EndVertical();
			GUILayout.EndArea();

		}
	}
}
