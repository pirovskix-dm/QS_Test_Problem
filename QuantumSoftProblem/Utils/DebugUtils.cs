using System;
using System.Diagnostics;

namespace QuantumSoftProblem.Utils
{
	public static class DebugUtils
	{
		[Conditional("DEBUG")]
		public static void WriteLine(string message)
		{
			var stackFrame = new StackTrace(2, true).GetFrame(0);
			string method = new StackTrace(1, true).GetFrame(0).GetMethod().Name;
			string file = new StackTrace(1, true).GetFrame(0).GetMethod().DeclaringType.Name;
			string callingFile = new StackTrace(2, true).GetFrame(0).GetMethod().DeclaringType.Name;
			int line = stackFrame.GetFileLineNumber();
			Debug.WriteLine(message, $"{line} {callingFile} -> {file}.{method}");
		}

		[Conditional("DEBUG")]
		public static void WriteException(Exception ex)
		{
			WriteLine("EXCEPTION");
			Debug.WriteLine(ex.Message, "EXCEPTION");
			Debug.WriteLine(ex.StackTrace, "EXCEPTION");
		}
	}
}