using System;
using System.Collections;

using Couchbase.Lite;
using Couchbase.Lite.Util;
using UnityEngine;
using UnityEngine.UI;
using Couchbase.Lite.Unity;
using System.Diagnostics;
using System.Collections.Generic;

public class TestLogger : ILogger {
	private readonly Text _loggerText;
	private readonly SourceLevels _level;
	private readonly LinkedList<string> _rollover = new LinkedList<string> ();
	private LinkedListNode<string> _currentNode;

	public TestLogger(Text loggerText, SourceLevels filterLevel)
	{
		_loggerText = loggerText;
		_level = filterLevel;
	}

	public bool GoBack() {
		if (_currentNode != null && _currentNode.Previous == null) {
			return false;
		}

		if (_currentNode == null) {
			_rollover.AddLast (_loggerText.text);
			_currentNode = _rollover.Last;
			if (_currentNode.Previous != null) {
				_currentNode = _currentNode.Previous;
			}

		} else {
			_currentNode = _currentNode.Previous;
		}

		_loggerText.text = _currentNode.Value;
		return _currentNode.Previous != null;
	}

	public bool GoForward() {
		if (_currentNode == null || _currentNode.Next == null) {
			return false;
		}

		_currentNode = _currentNode.Next;
		_loggerText.text = _currentNode.Value;
		return _currentNode.Next != null;
	}

	static Exception Flatten (Exception tr)
	{
		if (!(tr is AggregateException))
			return tr;
		var err = ((AggregateException)tr).Flatten().InnerException;
		return err;
	}

	private void WriteToLog(string message) {
		UnityMainThreadScheduler.TaskFactory.StartNew (() => {
			_loggerText.text += message;
			if(_loggerText.preferredHeight > 3000.0f) {
				_rollover.AddLast(_loggerText.text);
				_loggerText.text = string.Empty;
			}
		});
	}
	
	#region ILogger
	
	public void V (string tag, string msg)
	{

		if (!(_level.HasFlag(SourceLevels.Verbose)))
			return;

		WriteToLog(string.Format ("[VERBOSE] {0}: {1}\n", tag, msg));
	}

	public void V (string tag, string msg, Exception tr)
	{
		if (!(_level.HasFlag(SourceLevels.Verbose)))
			return;

		WriteToLog(string.Format ("[VERBOSE] {0}: {1}\n{2}\n", tag, msg, Flatten (tr)));
	}

	public void V (string tag, string format, params object[] args)
	{
		V (tag, String.Format (format, args));
	}

	public void D (string tag, string msg)
	{
		if (!(_level.HasFlag(SourceLevels.ActivityTracing)))
			return;

		WriteToLog(string.Format ("[DEBUG] {0}: {1}\n", tag, msg));
	}
	public void D (string tag, string msg, Exception tr)
	{
		if (!(_level.HasFlag(SourceLevels.ActivityTracing)))
			return;

		WriteToLog(string.Format ("[DEBUG] {0}: {1}\n{2}\n", tag, msg, Flatten (tr)));
	}
	public void D (string tag, string format, params object[] args)
	{
		D (tag, String.Format (format, args));
	}
	public void I (string tag, string msg)
	{
		if (!(_level.HasFlag(SourceLevels.Information)))
			return;

		WriteToLog(string.Format ("[INFO] {0}: {1}\n", tag, msg));
	}
	public void I (string tag, string msg, System.Exception tr)
	{
		if (!(_level.HasFlag(SourceLevels.Information)))
			return;

		WriteToLog(string.Format ("[INFO] {0}: {1}\n{2}\n", tag, msg, Flatten (tr)));
	}
	public void I (string tag, string format, params object[] args)
	{
		I (tag, String.Format (format, args));
	}
	public void W (string tag, string msg)
	{
		if (!(_level.HasFlag(SourceLevels.Warning)))
			return;

		WriteToLog(string.Format ("[WARN] {0}: {1}\n", tag, msg));
	}
	public void W (string tag, System.Exception tr)
	{
		if (!(_level.HasFlag(SourceLevels.Warning)))
			return;

		WriteToLog(string.Format ("[WARN] {0}: {1}\n{2}\n", tag, Flatten (tr)));
	}
	public void W (string tag, string msg, System.Exception tr)
	{
		if (!(_level.HasFlag(SourceLevels.Warning)))
			return;
		
		WriteToLog(string.Format ("[WARN] {0}: {1}\n{2}\n", tag, msg, Flatten (tr)));
	}
	public void W (string tag, string format, params object[] args)
	{
		W (tag, String.Format (format, args));
	}
	public void E (string tag, string msg)
	{
		if (!(_level.HasFlag(SourceLevels.Error)))
			return;

		WriteToLog(string.Format ("[ERROR] {0}: {1}\n", tag, msg));
	}
	public void E (string tag, string msg, System.Exception tr)
	{
		if (!(_level.HasFlag(SourceLevels.Error)))
			return;

		WriteToLog(string.Format ("[ERROR] {0}: {1}\n{2}\n", tag, msg, Flatten (tr)));
	}
	public void E (string tag, string format, params object[] args)
	{
		E (tag, String.Format (format, args));
	}
	#endregion
	
}
