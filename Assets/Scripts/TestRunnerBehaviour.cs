using UnityEngine;
using System.Reflection;
using Couchbase.Lite.Util;
using Couchbase.Lite.Unity;
using Couchbase.Lite;
using System.IO;
using System;
using System.Diagnostics;
using UnityEngine.UI;
using NUnit.Framework.Internal;

namespace NUnitLiteForUnity
{
	public class TestRunnerBehaviour : MonoBehaviour
	{
		public InputField TestInputField;
		public Text LoggingArea;
		public Button PrevButton;
		public Button NextButton;

		private TestLogger _testLogger;

		public void ShowNextLog()
		{
			if (!_testLogger.GoForward ()) {
				NextButton.enabled = false;
			}

			PrevButton.enabled = true;
		}

		public void ShowPrevLog()
		{
			if (!_testLogger.GoBack ()) {
				PrevButton.enabled = false;
			}
			
			NextButton.enabled = true;
		}

		public void StartTests ()
		{
			Log.I ("TestRunnerBehaviour", "Start the tests!");
			LoggingArea.text = String.Empty;
			
			if (TestInputField == null) {
				throw new InvalidOperationException("Set the TestInputField property in the editor first!");
			}

			_testLogger = new TestLogger (LoggingArea, SourceLevels.Information);
			Log.SetLogger(_testLogger);


#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
			SQLitePCL.SQLite3Provider.SetDllDirectory(Application.persistentDataPath);
#endif

			Log.I ("TestRunnerBehaviour", Application.persistentDataPath);
			LiteTestCase.RootDirectory = new DirectoryInfo (Application.persistentDataPath);

			if (StringEx.IsNullOrWhiteSpace (TestInputField.text)) {
				NUnitLiteForUnityTestRunner.RunTests (TestFilter.Empty);
			} else {
				NUnitLiteForUnityTestRunner.RunTests (new UnityTestFilter(TestInputField.text.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)));
			}

		}
	}
}