using System;
using System.Collections.Generic;
using UnityEngine;

// Local stand-in for the shut-down GameSpy backend.
//
// All multiplayer record traffic used to hit gamespy.net (auth, the "sake"
// record store, stats). Those servers no longer exist, so GripNetwork routes
// every call here instead. Writes are kept in an in-memory record store so the
// player's own records read back consistently within a session; cross-player
// searches return empty, which makes MultiplayerCollectionStatus fall back to
// the game's built-in AI opponents (MultiplayerAIOpponentData). Match rewards
// are applied to the normal Profile save, so they persist as usual.
public static class OfflineBackend
{
	// The official GameSpy servers are gone, so this fork serves multiplayer
	// locally by default. Flip to false to restore the (now dead) online path.
	public static bool Enabled = true;

	// Stable, non-zero owner id for the local player. 0 is reserved for AI
	// opponents, so we derive a positive id from the device user id and never
	// allow 0. Cached so it stays stable across a session.
	private static int mLocalProfileId;

	public static int LocalProfileId
	{
		get
		{
			if (mLocalProfileId == 0)
			{
				int hash = 0;
				string userID = ApplicationUtilities.UserID;
				if (!string.IsNullOrEmpty(userID))
				{
					hash = userID.GetHashCode();
				}
				mLocalProfileId = hash & 0x7FFFFFFF;
				if (mLocalProfileId == 0)
				{
					mLocalProfileId = 1;
				}
			}
			return mLocalProfileId;
		}
	}

	private class Table
	{
		public int NextId = 1;

		public readonly Dictionary<int, GripField[]> Records = new Dictionary<int, GripField[]>();
	}

	private static readonly Dictionary<string, Table> Tables = new Dictionary<string, Table>();

	private static Table GetTable(string name)
	{
		Table table;
		if (!Tables.TryGetValue(name, out table))
		{
			table = new Table();
			Tables[name] = table;
		}
		return table;
	}

	private static DisposableMonoBehaviour Dispatch(string name, Action body)
	{
		GameObject gameObject = new GameObject(name);
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		OfflineDispatch offlineDispatch = gameObject.AddComponent<OfflineDispatch>();
		offlineDispatch.Run(body);
		return offlineDispatch;
	}

	public static void Login(Action<GripNetwork.Result> callback)
	{
		UnityThreadHelper.Activate();
		if (callback != null)
		{
			callback(GripNetwork.Result.Success);
		}
	}

	public static DisposableMonoBehaviour CreateRecord(string tableID, GripField[] fields, Action<GripNetwork.Result, int> callback)
	{
		Table table = GetTable(tableID);
		int id = table.NextId++;
		table.Records[id] = fields;
		return Dispatch("OfflineBackend_CreateRecord", delegate
		{
			if (callback != null)
			{
				callback(GripNetwork.Result.Success, id);
			}
		});
	}

	public static DisposableMonoBehaviour RemoveRecord(string tableID, int recordID, Action<GripNetwork.Result, int> callback)
	{
		GetTable(tableID).Records.Remove(recordID);
		return Dispatch("OfflineBackend_RemoveRecord", delegate
		{
			if (callback != null)
			{
				callback(GripNetwork.Result.Success, recordID);
			}
		});
	}

	public static DisposableMonoBehaviour UpdateRecord(string tableID, int recordID, GripField[] fields, Action<GripNetwork.Result> callback)
	{
		GetTable(tableID).Records[recordID] = fields;
		return Dispatch("OfflineBackend_UpdateRecord", delegate
		{
			if (callback != null)
			{
				callback(GripNetwork.Result.Success);
			}
		});
	}

	// Cross-player search: no human players exist offline, so return an empty
	// result set. Callers react by pulling in AI opponents instead.
	public static DisposableMonoBehaviour SearchRecords(int fieldCount, Action<GripNetwork.Result, GripField[,]> callback)
	{
		return Dispatch("OfflineBackend_SearchRecords", delegate
		{
			if (callback != null)
			{
				callback(GripNetwork.Result.Success, new GripField[0, Mathf.Max(1, fieldCount)]);
			}
		});
	}

	public static DisposableMonoBehaviour GetMyRecords(int fieldCount, Action<GripNetwork.Result, GripField[,]> callback)
	{
		return Dispatch("OfflineBackend_GetMyRecords", delegate
		{
			if (callback != null)
			{
				callback(GripNetwork.Result.Success, new GripField[0, Mathf.Max(1, fieldCount)]);
			}
		});
	}

	// "Load my single record" path: no stored record offline for a fresh player.
	// Return RecordNotFound so callers take their null/default branch rather than
	// indexing row 0 of an empty result set.
	public static DisposableMonoBehaviour FirstRecordNotFound(Action<GripNetwork.Result, GripField[,]> callback)
	{
		return Dispatch("OfflineBackend_FirstRecordNotFound", delegate
		{
			if (callback != null)
			{
				callback(GripNetwork.Result.RecordNotFound, new GripField[0, 1]);
			}
		});
	}

	public static DisposableMonoBehaviour CountRecords(Action<GripNetwork.Result, int> callback)
	{
		return Dispatch("OfflineBackend_CountRecords", delegate
		{
			if (callback != null)
			{
				callback(GripNetwork.Result.Success, 0);
			}
		});
	}

	public static DisposableMonoBehaviour ReadAndLockRecord(Action<GripNetwork.Result, GripField[]> callback)
	{
		return Dispatch("OfflineBackend_ReadAndLockRecord", delegate
		{
			if (callback != null)
			{
				callback(GripNetwork.Result.RecordNotFound, new GripField[0]);
			}
		});
	}

	public static DisposableMonoBehaviour Success(string name, Action<GripNetwork.Result> callback)
	{
		return Dispatch(name, delegate
		{
			if (callback != null)
			{
				callback(GripNetwork.Result.Success);
			}
		});
	}

	public static DisposableMonoBehaviour UploadFile(Action<GripNetwork.Result, string> callback)
	{
		return Dispatch("OfflineBackend_UploadFile", delegate
		{
			if (callback != null)
			{
				callback(GripNetwork.Result.Success, "offline");
			}
		});
	}

	public static DisposableMonoBehaviour SearchHosts(Action<GripNetwork.Result, List<Gamespy.Matchmaking.GameHost>> callback)
	{
		return Dispatch("OfflineBackend_SearchHosts", delegate
		{
			if (callback != null)
			{
				callback(GripNetwork.Result.Success, new List<Gamespy.Matchmaking.GameHost>());
			}
		});
	}
}
