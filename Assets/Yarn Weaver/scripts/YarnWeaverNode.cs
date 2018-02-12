using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace YarnWeaver {

	public class YarnWeaverNode : MonoBehaviour, IEndDragHandler {
		
		public YarnWeaverLoader.NodeInfo nodeInfo;
		public int nodeIndex;
		public string nodeTitle { get { return nodeInfo.title; } set { nodeInfo.title = value; } }
		public string nodeBody { get { return nodeInfo.body; } set { nodeInfo.body = value; } }
		public Color nodeColor; // TODO: ??? what is ColorID tho?
		public Vector2 nodePos { get { return new Vector2( nodeInfo.position.x, nodeInfo.position.y ); } set { var pos = new YarnWeaverLoader.NodeInfo.Position(); pos.x = Mathf.RoundToInt(value.x); pos.y = Mathf.RoundToInt(value.y); nodeInfo.position = pos; } }

		[SerializeField] Image headerColor, bodyColor;
		[SerializeField] Text textHeader, textBody;
		RectTransform trans;

		// Use this for initialization
		void Start () {
			trans = GetComponent<RectTransform>();
			Refresh();
		}

		// called by YarnWeaverEditor usually
		public void SetData( int nodeIndex, string nodeTitle, string nodeBody, Color nodeColor, Vector2 nodePos ) {
			this.nodeIndex = nodeIndex;
			this.nodeTitle = nodeTitle;
			this.nodeBody = nodeBody;
			this.nodeColor = nodeColor;
			this.nodePos = nodePos;
		}

		// this is separate from SetData because it needs to happen in Start(), not upon instantiation by YarnWeaverEditor
		public void Refresh () {
			textHeader.text = this.nodeTitle;
			textBody.text = this.nodeBody;
			headerColor.color = this.nodeColor;
			trans.anchoredPosition = this.nodePos;
		}
		
		// Update is called once per frame
		void Update () {
			
		}

		public void OnClickEdit () {
			YarnWeaverEditor.instance.SetCurrentNode( this );
		}

		// EndDrag hander to remember the new position
		public void OnEndDrag( PointerEventData eventData ) {
			nodePos = trans.anchoredPosition;
		}
	}
}
