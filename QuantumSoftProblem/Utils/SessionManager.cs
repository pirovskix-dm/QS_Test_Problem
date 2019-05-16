using System.Web.SessionState;

namespace QuantumSoftProblem.Utils
{
	public static class SessionManager
	{
		public static T GetSessionData<T>(string name, T def) where T : class
		{
			HttpSessionState currentSession = System.Web.HttpContext.Current.Session;
			var sessionData = currentSession[name] as T;
			if (sessionData == null)
			{
				sessionData = def;
				currentSession[name] = sessionData;
				return sessionData;
			}
			return sessionData;
		}

		public static void ClearSession()
		{
			System.Web.HttpContext.Current.Session.Clear();
		}
	}
}