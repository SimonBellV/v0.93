using UnityEngine;

namespace MoBack {
	public class MoBackAppSettings : MonoBehaviour {
		public enum LoggingLevel { ERRORS = 0, WARNINGS = 1, VERBOSE = 2 }

		private static string _applicationId, _envKey, _sessionToken;

		public static LoggingLevel loggingLevel = LoggingLevel.WARNINGS;

		public static bool doublePrecisionFloatingPoint { get; private set; }

		[SerializeField]
		string _appId, _environmentKey;
		[SerializeField]
		LoggingLevel _loggingLevel;
		[SerializeField]
		bool _doublePrecisionFloatingPoint;

		public static string ApplicationID {
			get { return _applicationId; }
		}

		public static string EnvironmentKey {
			get { return _envKey; }
		}

		public static string SessionToken {
			get { return _sessionToken; }
			set { _sessionToken = value; }
		}

		void Awake() {
			loggingLevel = _loggingLevel;
			doublePrecisionFloatingPoint = SimpleJSON.SimpleJSON.assumeDoublePrecisionFloatingPoint = _doublePrecisionFloatingPoint;
			SetApplicationIdAndEnvironmentKey (_appId, _environmentKey);
		}

		public void SetApplicationIdAndEnvironmentKey(string applicationId, string environmentKey) {
			_applicationId = applicationId;
			_envKey = environmentKey;

			if(MoBackAppSettings.loggingLevel >= MoBackAppSettings.LoggingLevel.VERBOSE)
			{
				Debug.Log("Application ID set as: " + _applicationId);
				Debug.Log("Environment Key set as: " + _envKey);
			}
		}

		/// <summary>
		/// Sets the float precision mode. It is not reccomended to make this change mid-app.
		/// </summary>
		/// <param name="doublePrecision">If set to <c>true</c> double precision.</param>
		public static void SetFloatPrecisionMode(bool useDoublePrecision) {
			doublePrecisionFloatingPoint = SimpleJSON.SimpleJSON.assumeDoublePrecisionFloatingPoint = useDoublePrecision;
		}
	}
}