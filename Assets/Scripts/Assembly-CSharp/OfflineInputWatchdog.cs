using UnityEngine;

// Safety net for the offline port.
//
// Several menu flows (especially the multiplayer raid/attack path) disable global
// input via InputManager.InputEnabled and used to re-enable it from a GameSpy
// server callback. Offline, some of those callbacks never complete the same way,
// so input can stay disabled and every button appears dead.
//
// This watchdog auto-spawns at startup (no wiring needed) and:
//   * re-enables input if it has been stuck disabled far longer than any legit
//     async UI flow would hold it (6 s), and
//   * offers F9 as a manual override.
//
// It lives in its own file/type because that compiles reliably; editing existing
// methods for this proved unreliable in the CI build.
public class OfflineInputWatchdog : MonoBehaviour
{
	private const float kStuckThresholdSeconds = 6f;

	private float mDisabledSince = -1f;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void Bootstrap()
	{
		GameObject gameObject = new GameObject("OfflineInputWatchdog");
		Object.DontDestroyOnLoad(gameObject);
		gameObject.AddComponent<OfflineInputWatchdog>();
	}

	private void Update()
	{
		if (!SingletonMonoBehaviour<InputManager>.Exists)
		{
			return;
		}
		InputManager instance = SingletonMonoBehaviour<InputManager>.Instance;

		if (Input.GetKeyDown(KeyCode.F9))
		{
			instance.InputEnabled = true;
			mDisabledSince = -1f;
			Debug.Log("OfflineInputWatchdog: F9 forced InputEnabled = true");
			return;
		}

		if (instance.InputEnabled)
		{
			mDisabledSince = -1f;
			return;
		}

		if (mDisabledSince < 0f)
		{
			mDisabledSince = Time.unscaledTime;
		}
		else if (Time.unscaledTime - mDisabledSince > kStuckThresholdSeconds)
		{
			instance.InputEnabled = true;
			mDisabledSince = -1f;
			Debug.Log("OfflineInputWatchdog: auto-recovered stuck InputEnabled after " + kStuckThresholdSeconds + "s");
		}
	}
}
