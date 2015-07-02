using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;

using NUnit;
using NUnit.Framework;

using NUnitLite;

using UnityEngine;
using Couchbase.Lite.Unity;
using System.Threading.Tasks;

using NUnit.Framework.Api;
using NUnitLite.Runner;
using NUnit.Framework.Internal;
using Couchbase.Lite;
using NUnit.Framework.Internal.WorkItems;
using System.Text.RegularExpressions;
using Couchbase.Lite.Util;

namespace NUnitLiteForUnity
{
	public class UnityListener : ITestListener
	{
		private const string TAG = "UnityListener";

		public void TestStarted (ITest test)
		{
			UnityMainThreadScheduler.TaskFactory.StartNew (() => {
				Log.I (TAG, "Starting test {0}", test.Name);
			});
		}

		public void TestFinished (ITestResult result)
		{
			UnityMainThreadScheduler.TaskFactory.StartNew (() => {
				if(result.ResultState.Status == TestStatus.Failed) {
					string format = "Test failed: {0}\n{1} ({2})\n{3}\n{4}";
					Log.E (TAG, format, result.ResultState, result.Name, result.FullName, result.Message, result.StackTrace);
				} else if(result.ResultState.Status == TestStatus.Inconclusive) {
					string format = "Test inconclusive: {0}\n{1} ({2})\n{3}\n{4}";
					Log.W (TAG, format, result.ResultState, result.Name, result.FullName, result.Message, result.StackTrace);
				} else {
					Log.I (TAG, "Test passed: {0}", result.Name);
				}
			});

		}

		public void TestOutput (TestOutput testOutput)
		{

		}
	}

	public class UnityTestFilter : ITestFilter
	{
		private readonly string[] _testNames;

		public UnityTestFilter(string[] testNames)
		{
			_testNames = testNames;
		}
		#region ITestFilter implementation

		public bool Pass (ITest test)
		{
			if (test is TestFixture) {
				return true;
			}

			foreach (var name in _testNames) {
				if(test.FullName.Contains(name)) {
					return true;
				}
			}
			return false;
		}

		public bool IsEmpty {
			get {
				return false;
			}
		}

		#endregion


	}

	public static class NUnitLiteForUnityTestRunner
	{
		public static void RunTests (ITestFilter filter)
		{
			var testRunner = new NUnit.Framework.Internal.NUnitLiteTestAssemblyRunner (new NUnit.Framework.Internal.NUnitLiteTestAssemblyBuilder ());

			var settings = new Dictionary<string, IList> {
				//{ "LOAD", new List<string> { "Couchbase.Lite.ReplicationTest" } }
			};


			Assembly assembly = Assembly.Load ("Couchbase.Lite.Unity.Tests");
			bool hasTestLoaded = testRunner.Load (assembly, settings);
			if (!hasTestLoaded) {
				Debug.Log ("No tests found in assembly");
				return;
			}

			Task.Factory.StartNew (() => {
				testRunner.Run(new UnityListener(), filter);
			}, TaskCreationOptions.LongRunning);
		}

		static string ToSummryString (ResultSummary resultSummry)
		{
			using (StringWriter stringWriter = new StringWriter ()) {
				stringWriter.Write ("{0} Tests :", resultSummry.TestCount);
				stringWriter.Write (" {0} Pass", resultSummry.PassCount);

				if (resultSummry.FailureCount > 0) {
					stringWriter.Write (", {0} Failure", resultSummry.FailureCount);
				}

				if (resultSummry.ErrorCount > 0) {
					stringWriter.Write (", {0} Error", resultSummry.ErrorCount);
				}

				if (resultSummry.NotRunCount > 0) {
					stringWriter.Write (", {0} NotRun", resultSummry.NotRunCount);
				}

				if (resultSummry.InconclusiveCount > 0) {
					stringWriter.Write (", {0} Inconclusive", resultSummry.InconclusiveCount);
				}

				return stringWriter.GetStringBuilder ().ToString ();
			}
		}

		static IEnumerable<ITestResult> EnumerateTestResult (ITestResult result)
		{
			if (result.HasChildren) {
				foreach (ITestResult child in result.Children) {
					foreach (ITestResult c in EnumerateTestResult (child)) {
						yield return c;
					}
				}
			}
			yield return result;
		}

		static void DebugLogErrorResults (IEnumerable<ITestResult> testResults)
		{
			foreach (ITestResult testResult in testResults) {
				if (testResult.ResultState == ResultState.Error || testResult.ResultState == ResultState.Failure) {
					string foramt = "{0}\n{1} ({2})\n{3}\n{4}";
					string log = string.Format (foramt, testResult.ResultState, testResult.Name, testResult.FullName, testResult.Message, testResult.StackTrace);
					Debug.Log (log);
				}
			}
		}

		static void DebugLogNotRunResults (IEnumerable<ITestResult> testResults)
		{
			foreach (ITestResult testResult in testResults) {
				if (testResult.ResultState == ResultState.Ignored || testResult.ResultState == ResultState.NotRunnable || testResult.ResultState == ResultState.Skipped) {
					string foramt = "{0}\n{1} ({2})\n{3}";
					string log = string.Format (foramt, testResult.ResultState, testResult.Name, testResult.FullName, testResult.Message);
					Debug.Log (log);
				}
			}
		}
	}
}
