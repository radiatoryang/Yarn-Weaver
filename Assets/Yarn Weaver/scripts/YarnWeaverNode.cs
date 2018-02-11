using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace YarnWeaver {

	public class YarnWeaverNode : MonoBehaviour, IEndDragHandler {

		public int nodeIndex;
		public string nodeTitle, nodeBody;
		public Color nodeColor;
		public Vector2 nodePos;

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
