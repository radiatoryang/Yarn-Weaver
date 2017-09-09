using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;
using Yarn.Analysis;

// this "tests" / compiles Yarn scripts, and displays any compile errors or anything

// this is basically a port of YarnSpinnerEditorWindow
// but that's in the Editor namespace (with Editor GUI stuff) so we can't just put it in YarnWeaver

public class YarnWeaverTests : MonoBehaviour {

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {

	}

	// everything after this is basically ripped from YarnSpinnerEditorWindow
	// ideally, in the future, YarnSpinner uncouples this stuff from UnityEditor, and puts it in Yarn.Analysis?

	public enum MessageType
	{
		None,
		Info,
		Warning,
		Error
	}

	class CheckerResult {
		public enum State {
			NotTested,
			Passed,
			Failed
		}

		public State state;
		public TextAsset script;

		public ValidationMessage[] messages = new ValidationMessage[0];

		public override bool Equals (object obj)
		{
			if (obj is CheckerResult && ((CheckerResult)obj).script == this.script)
				return true;
			else
				return false;
		}

		public override int GetHashCode ()
		{
			return this.script.GetHashCode();
		}

		public CheckerResult(TextAsset script) {
			this.script = script;
			this.state = State.NotTested;
		}
	}

	// Validates a single script.
	ValidationMessage[] ValidateFile(TextAsset script, Context analysisContext, out CheckerResult.State result) {

		// The list of messages we got from the compiler.
		var messageList = new List<ValidationMessage>();

		// A dummy variable storage; it won't be used, but Dialogue
		// needs it.
		var variableStorage = new Yarn.MemoryVariableStore();

		// The Dialog object is the compiler.
		var dialog = new Dialogue(variableStorage);

		// Whether compilation failed or not; this will be
		// set to true if any error messages are returned.
		bool failed = false;

		// Called when we get an error message. Convert this
		// message into a ValidationMessage and store it;
		// additionally, mark that this file failed compilation
		dialog.LogErrorMessage = delegate (string message) {
			var msg = new ValidationMessage();
			msg.type = MessageType.Error;
			msg.message = message;
			messageList.Add(msg);

			// any errors means this validation failed
			failed = true;
		};

		// Called when we get an error message. Convert this
		// message into a ValidationMessage and store it
		dialog.LogDebugMessage = delegate (string message) {
			var msg = new ValidationMessage();
			msg.type = MessageType.Info;
			msg.message = message;
			messageList.Add(msg);
		};

		// Attempt to compile this script. Any exceptions will result
		// in an error message
		try {
			dialog.LoadString(script.text,script.name);
		} catch (System.Exception e) {
			dialog.LogErrorMessage(e.Message);
		}

		// Once compilation has finished, run the analysis on it
		dialog.Analyse(analysisContext);

		// Did it succeed or not?
		if (failed) {
			result = CheckerResult.State.Failed;
		} else {
			result = CheckerResult.State.Passed;
		}

		// All done.
		return messageList.ToArray();

	}

	// A result from validation.
	struct ValidationMessage {
		public string message;

		public MessageType type;
	}

	struct Deprecation {
		public System.Type type;
		public string methodName;
		public string usageNotes;

		public Deprecation (System.Type type, string methodName, string usageNotes)
		{
			this.type = type;
			this.methodName = methodName;
			this.usageNotes = usageNotes;
		}

	}
		
}
