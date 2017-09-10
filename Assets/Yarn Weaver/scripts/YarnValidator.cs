using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Yarn;
using Yarn.Unity;
using Yarn.Analysis;

using TextAsset = YarnWeaver.RuntimeTextAsset;


// this "tests" / compiles Yarn scripts, and displays any compile errors or anything
// this is basically a port of YarnSpinnerEditorWindow, but that's in the Editor namespace (with Editor GUI stuff) so we can't just put it in YarnWeaver

namespace YarnWeaver
{
	// in Unity, TextAssets cannot be created at runtime, so we have to override it with our version, that CAN be generated at runtime
	// that's because all of YarnSpinner's analysis stuff relies on TextAsset, and we should try not to refactor it all the time
	// (the override is set at the top: "using TextAsset...")
	[System.Serializable]
	public class RuntimeTextAsset {
		public string name, text;
		public byte[] bytes;

		public RuntimeTextAsset ( string name, string text ) {
			this.name = name;
			this.text = text;
		}
	}

	public class YarnValidator : MonoBehaviour {

		[SerializeField] Text compileLogTextDisplay; // displays warnings and errors; assigned in Inspector
		List<string> compileExceptions = new List<string>();

		// called by YarnWeaverMain
		public void StartMain( Dictionary<string, string> yarnScriptData, List<string> compileExceptions ) {
			// grab compile exception stuff from YarnWeaverMain
			this.compileExceptions = compileExceptions;

			// convert script data into RuntimeTextAssets
			var yarnTextAssets = new List<TextAsset>();
			foreach( var kvp in yarnScriptData ) {
				yarnTextAssets.Add( new TextAsset( kvp.Key, kvp.Value ) );
			}

			// grab all files
			UpdateYarnScriptList( yarnTextAssets );

			// begin processing
			StartCoroutine( GoValidate() );
		}

		IEnumerator GoValidate() {
			Debug.Log( "beginning compile and validation..." );

			var noCompileErrors = compileExceptions.Count == 0;

			// analyze files
			if( noCompileErrors ) {
				yield return StartCoroutine( CheckAllFiles() );

				// assuming the files are valid, then we compile them
				yield return StartCoroutine( CompileAllScripts() );
			}

			// output all results back to user

			// based on various bits ripped from YarnSpinnerEditorWindow.OnGUI()
			var canCompile = checkResults.TrueForAll(item => item.state == CheckerResult.State.Passed) && noCompileErrors;

			compileLogTextDisplay.text = "===== Yarn Script File Checker =====\n";
			compileLogTextDisplay.text += canCompile ? "<color=green><b>PASSED!</b></color>\nJust press the Play button at the top.\n\n" : "<color=red><b>ERROR: CANNOT PLAY! PLEASE FIX!</b></color>\nSorry, I had trouble reading your Yarn script. Please fix and try again. Here's where I was confused:\n\n";

			foreach( var exception in compileExceptions ) {
				compileLogTextDisplay.text += "<color=red>" + exception + "</color>\n";
			}

			// end early if we cannot compile the rest of it
			if( !noCompileErrors ) {
				yield break;
			}

			// do tests
			foreach (var result in checkResults) {
				// TODO: change display from text readout into nice boxes with icons
				string newResult = "";

				// type of result?
				switch (result.state) {
				case CheckerResult.State.NotTested:
				case CheckerResult.State.Passed:
				case CheckerResult.State.Failed:
					newResult += "> " + result.state.ToString() + "\n";
					break;
				default:
					throw new System.ArgumentOutOfRangeException ();
				}

				// ok actually print the message now
				foreach (var message in result.messages) {
					newResult += message.type.ToString() + ": " + message.message + "\n";
				}

				// add finished result entry to textDisplay
				compileLogTextDisplay.text += newResult + "\n\n";
			}

			compileLogTextDisplay.text += "===== Analysis =====\n\n";

			// Draw any diagnoses that resulted
			foreach (var diagnosis in diagnoses) {

//				MessageType messageType;
//
//				switch (diagnosis.severity) {
//				case Yarn.Analysis.Diagnosis.Severity.Error:
//					messageType = MessageType.Error;
//					break;
//				case Yarn.Analysis.Diagnosis.Severity.Warning:
//					messageType = MessageType.Warning;
//					break;
//				case Yarn.Analysis.Diagnosis.Severity.Note:
//					messageType = MessageType.Info;
//					break;
//				default:
//					throw new System.ArgumentOutOfRangeException ();
//				}

				compileLogTextDisplay.text += diagnosis.ToString(showSeverity:true) + "\n";
			}
		}

		// adapted from YarnSpinnerEditorWindow:
		// Updates the list of all scripts that should be checked.
		void UpdateYarnScriptList( List<TextAsset> yarnScriptFiles ) {

			// Clear the list of files
			checkResults.Clear();

			// Clear the list of diagnoses
			diagnoses = new List<Yarn.Analysis.Diagnosis>();

			// Find all TextAssets
			// 9 September 2017 -- we aren't using the Asset Database in YarnWeaver, instead YarnWeaverMain passes in "yarnScriptFiles"
			// var list = AssetDatabase.FindAssets("t:textasset");

			foreach (var guid in yarnScriptFiles) {

				// Filter the list to only include .json files
				// 9 September 2017 -- actually, we don't want to do this anymore?
				/*
				var path = AssetDatabase.GUIDToAssetPath(guid);
				if (path.EndsWith(".json")) {
					var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);

					var newResult = new CheckerResult(asset);

					checkResults.Add(newResult);
				}
				*/
				checkResults.Add( new CheckerResult( guid ) );
			}
		}

		// adapted from YarnSpinnerEditorWindow.CompileAllScripts(), converted into coroutine
		IEnumerator CompileAllScripts() {
			foreach( var entry in checkResults ) {
				var variableStorage = new Yarn.MemoryVariableStore();

				var dialog = new Dialogue( variableStorage );

				bool failed = false;

				dialog.LogErrorMessage = delegate (string message ) {
					Debug.LogWarningFormat( "Error when compiling: {0}", message );
					failed = true;
				};

				dialog.LogDebugMessage = delegate (string message ) {
					Debug.LogFormat( "{0}", message );
				};

				try {
					dialog.LoadString( entry.script.text, entry.script.name );
				} catch( System.Exception e ) {
					dialog.LogErrorMessage( e.Message );
					break;
				}

				if( failed ) {
					Debug.LogErrorFormat( "Failed to compile script {0}; stopping", entry.script.name );
					break;
				}

				yield return 0;
			}

		}

		// adapted from YarnSpinnerEditorWindow.CheckAllFiles(), converted into coroutine
		IEnumerator CheckAllFiles() {
			// The shared context for all script analysis.
			var analysisContext = new Yarn.Analysis.Context();

			// We shouldn't try to perform program analysis if
			// any of the files fails to compile, because that
			// analysis would be performed on incomplete data.
			bool shouldPerformAnalysis = true;

			// 9 Sept 2017, commented out from YarnSpinnerEditorWindow
			/*
			// How many files have we finished checking?
			int complete = 0;

			// Let's get started!

			// First, ensure that we're looking at all of the scripts.
			UpdateYarnScriptList();
			*/

			// Next, compile each one.
			foreach( var result in checkResults ) {

				// Attempt to compile the file. Record any compiler messages.
				CheckerResult.State state;

				var messages = ValidateFile( result.script, analysisContext, out state );

				result.state = state;
				result.messages = messages;

				// Don't perform whole-program analysis if any file failed to compile
				if( result.state != CheckerResult.State.Passed ) {
					shouldPerformAnalysis = false;
				}

				// 9 Sept 2017, commented out from YarnSpinnerEditorWindow
				/*
				// We're done with it; if we have a callback to call after
				// each file is validated, do so.
				complete++;

				if (callback != null)
					callback(complete, checkResults.Count);
				*/

				yield return 0;
			}

			var results = new List<Yarn.Analysis.Diagnosis>();

			if( shouldPerformAnalysis ) {
				var scriptAnalyses = analysisContext.FinishAnalysis();
				results.AddRange( scriptAnalyses );
			}
				
			var environmentAnalyses = AnalyseEnvironment();
			results.AddRange( environmentAnalyses );

			diagnoses = results;
		}

		// everything after this is literally ripped verbatim from YarnSpinnerEditorWindow
		// ideally, in the future, YarnSpinner uncouples some of this stuff from UnityEditor, and puts it in Yarn.Analysis?
		// but who has the time to refactor that? it'd probably be a waste of time

		#region RIPPED_FROM_YARNSPINNER

		// The list of files that we know about, and their status.
		private List<CheckerResult> checkResults = new List<CheckerResult>();

		// The list of analysis results that were made as a result of checking
		// all scripts
		private List<Yarn.Analysis.Diagnosis> diagnoses = new List<Yarn.Analysis.Diagnosis>();

		public enum MessageType {
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

			public override bool Equals( object obj ) {
				if( obj is CheckerResult && ((CheckerResult)obj).script == this.script )
					return true;
				else
					return false;
			}

			public override int GetHashCode() {
				return this.script.GetHashCode();
			}

			public CheckerResult( TextAsset script ) {
				this.script = script;
				this.state = State.NotTested;
			}
		}

		// Validates a single script.
		ValidationMessage[] ValidateFile( TextAsset script, Context analysisContext, out CheckerResult.State result ) {

			// The list of messages we got from the compiler.
			var messageList = new List<ValidationMessage>();

			// A dummy variable storage; it won't be used, but Dialogue
			// needs it.
			var variableStorage = new Yarn.MemoryVariableStore();

			// The Dialog object is the compiler.
			var dialog = new Dialogue( variableStorage );

			// Whether compilation failed or not; this will be
			// set to true if any error messages are returned.
			bool failed = false;

			// Called when we get an error message. Convert this
			// message into a ValidationMessage and store it;
			// additionally, mark that this file failed compilation
			dialog.LogErrorMessage = delegate (string message ) {
				var msg = new ValidationMessage();
				msg.type = MessageType.Error;
				msg.message = message;
				messageList.Add( msg );

				// any errors means this validation failed
				failed = true;
			};

			// Called when we get an error message. Convert this
			// message into a ValidationMessage and store it
			dialog.LogDebugMessage = delegate (string message ) {
				var msg = new ValidationMessage();
				msg.type = MessageType.Info;
				msg.message = message;
				messageList.Add( msg );
			};

			// Attempt to compile this script. Any exceptions will result
			// in an error message
			try {
				dialog.LoadString( script.text, script.name );
			} catch( System.Exception e ) {
				dialog.LogErrorMessage( e.Message );
			}

			// Once compilation has finished, run the analysis on it
			dialog.Analyse( analysisContext );

			// Did it succeed or not?
			if( failed ) {
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

			public Deprecation( System.Type type, string methodName, string usageNotes ) {
				this.type = type;
				this.methodName = methodName;
				this.usageNotes = usageNotes;
			}

		}

		IEnumerable<Yarn.Analysis.Diagnosis> AnalyseEnvironment() {

			var deprecations = new List<Deprecation>();

			deprecations.Add( new Deprecation(
				typeof(Yarn.Unity.VariableStorageBehaviour),
				"SetNumber",
				"This method is obsolete, and will not be called in future " +
				"versions of Yarn Spinner. Use SetValue instead."
			) );

			deprecations.Add( new Deprecation(
				typeof(Yarn.Unity.VariableStorageBehaviour),
				"GetNumber",
				"This method is obsolete, and will not be called in future " +
				"versions of Yarn Spinner. Use GetValue instead."
			) );

			var results = new List<Yarn.Analysis.Diagnosis>();

			var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();


			foreach( var assembly in assemblies ) {
				foreach( var type in assembly.GetTypes() ) {

					foreach( var deprecation in deprecations ) {
						if( !type.IsSubclassOf( deprecation.type ) )
							continue;

						foreach( var method in type.GetMethods () ) {
							if( method.Name == deprecation.methodName && method.DeclaringType == type ) {
								var message = "{0} implements the {1} method. {2}";
								message = string.Format( message, type.Name, deprecation.methodName, deprecation.usageNotes );
								var diagnosis = new Yarn.Analysis.Diagnosis( message, Yarn.Analysis.Diagnosis.Severity.Warning );
								results.Add( diagnosis );
							}
						}

					}
				}
			}
			return results;
		}

		#endregion
		
	}
}
