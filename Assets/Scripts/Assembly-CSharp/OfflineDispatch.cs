using System;
using System.Collections;
using UnityEngine;

// Runs a callback one frame later, mirroring the async contract the old HTTP
// GripNetwork_* behaviours had, then disposes itself. Returned from
// OfflineBackend so callers keep treating the result as a DisposableMonoBehaviour.
public class OfflineDispatch : DisposableMonoBehaviour
{
	private Action mBody;

	public void Run(Action body)
	{
		mBody = body;
		StartCoroutine(Invoke());
	}

	private IEnumerator Invoke()
	{
		yield return null;
		if (mBody != null)
		{
			mBody();
		}
		Dispose();
	}
}
