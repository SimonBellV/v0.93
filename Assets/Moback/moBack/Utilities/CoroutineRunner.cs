using UnityEngine;
using System.Collections;

namespace MoBackInternal {
	/// <summary>
	/// Convenience for running coroutines from a static context.
	/// </summary>
	public class CoroutineRunner : MonoBehaviour {

		static CoroutineRunner _instance;

		bool quitting;

		public static Coroutine RunCoroutine(IEnumerator routine) {
			if (_instance == null) {
				_instance = new GameObject("_moBackCoroutineHolder").AddComponent<CoroutineRunner>();
			}
			return _instance.StartCoroutine(routine);
		}

		void Awake() {
			DontDestroyOnLoad (this);
		}

		void OnDisable() {
			if (MoBack.MoBackAppSettings.loggingLevel >= MoBack.MoBackAppSettings.LoggingLevel.WARNINGS && !quitting) {
				Debug.Log("Disabling MoBack's CoroutineRunner object instance will stop any network requests currently running");
			}
		}

		//De-facto, OnApplicationQuit is called before OnDisable, though this is not in any official documentation and may change (though this script will just throw spurrious warnings in that case)
		void OnApplicationQuit() {
			quitting = true;
		}
	}
}