using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Local stand-in for the shut-down GameSpy backend.
//
// All multiplayer record traffic used to hit gamespy.net (auth, the "sake"
// record store, stats). Those servers no longer exist, so GripNetwork routes
// every call here instead.
//
// The player's own records (their raid collection, defense loadout) are kept in
// a local store that is persisted to disk, so captured artifacts survive app
// restarts. Cross-player searches return empty, which makes the game fall back
// to its built-in AI opponents (MultiplayerAIOpponentData). Match rewards are
// applied to the normal Profile save as usual.
public static class OfflineBackend
{
	// The official GameSpy servers are gone, so this fork serves multiplayer
	// locally by default. Flip to false to restore the (now dead) online path.
	public static bool Enabled = true;

	private const string kStoreFileName = "offline_mp.dat";

	private static int mLocalProfileId;

	private static bool mLoaded;

	// Stable, non-zero owner id for the local player. 0 is reserved for AI
	// opponents, so we derive a positive id from the device user id and never
	// allow 0. Cached so it stays stable across a session.
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
		EnsureLoaded();
		Table table;
		if (!Tables.TryGetValue(name, out table))
		{
			table = new Table();
			Tables[name] = table;
		}
		return table;
	}

	private static GripField[] CloneFields(GripField[] fields)
	{
		if (fields == null)
		{
			return new GripField[0];
		}
		GripField[] copy = new GripField[fields.Length];
		for (int i = 0; i < fields.Length; i++)
		{
			copy[i] = (fields[i] != null) ? (GripField)fields[i].Clone() : null;
		}
		return copy;
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
		table.Records[id] = CloneFields(fields);
		Save();
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
		Save();
		return Dispatch("OfflineBackend_RemoveRecord", delegate
		{
			if (callback != null)
			{
				callback(GripNetwork.Result.Success, recordID);
			}
		});
	}

	// Updates carry only the changed fields (identified by name), so merge them
	// into the existing record instead of replacing the whole thing. Online,
	// UpdateRecord only ever touches records made by CreateRecord; creating a
	// record here for an unknown id produced malformed partial rows that crashed
	// the collection load, so unknown ids are ignored.
	public static DisposableMonoBehaviour UpdateRecord(string tableID, int recordID, GripField[] fields, Action<GripNetwork.Result> callback)
	{
		Table table = GetTable(tableID);
		GripField[] existing;
		if (table.Records.TryGetValue(recordID, out existing))
		{
			table.Records[recordID] = MergeFields(existing, fields);
			Save();
		}
		return Dispatch("OfflineBackend_UpdateRecord", delegate
		{
			if (callback != null)
			{
				callback(GripNetwork.Result.Success);
			}
		});
	}

	private static GripField[] MergeFields(GripField[] existing, GripField[] updates)
	{
		List<GripField> merged = new List<GripField>(CloneFields(existing));
		if (updates != null)
		{
			foreach (GripField update in updates)
			{
				if (update == null)
				{
					continue;
				}
				GripField clone = (GripField)update.Clone();
				int found = -1;
				for (int i = 0; i < merged.Count; i++)
				{
					if (merged[i] != null && string.Equals(merged[i].mName, clone.mName, StringComparison.OrdinalIgnoreCase))
					{
						found = i;
						break;
					}
				}
				if (found >= 0)
				{
					merged[found] = clone;
				}
				else
				{
					merged.Add(clone);
				}
			}
		}
		return merged.ToArray();
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

	// Return the player's own stored records, rebuilt into the column layout the
	// caller asked for. "recordid" and "ownerid" are synthesised (the real server
	// assigned these); all other columns are matched by field name.
	public static DisposableMonoBehaviour GetMyRecords(string tableID, string[] fieldNames, Action<GripNetwork.Result, GripField[,]> callback)
	{
		Table table = GetTable(tableID);
		int cols = (fieldNames != null && fieldNames.Length > 0) ? fieldNames.Length : 1;

		// Build only rows where every requested data column is present. Partial /
		// malformed records (e.g. a stray username-only row) are skipped so the
		// caller's index-based FromFields never hits a missing field.
		List<GripField[]> rows = new List<GripField[]>();
		foreach (KeyValuePair<int, GripField[]> record in table.Records)
		{
			GripField[] row = new GripField[cols];
			bool complete = true;
			for (int c = 0; c < cols; c++)
			{
				string name = (fieldNames != null && c < fieldNames.Length) ? fieldNames[c] : string.Empty;
				GripField field = BuildColumn(name, record.Key, record.Value);
				if (field == null)
				{
					complete = false;
					break;
				}
				row[c] = field;
			}
			if (complete)
			{
				rows.Add(row);
			}
		}

		GripField[,] result = new GripField[rows.Count, cols];
		for (int r = 0; r < rows.Count; r++)
		{
			for (int c = 0; c < cols; c++)
			{
				result[r, c] = rows[r][c];
			}
		}
		return Dispatch("OfflineBackend_GetMyRecords", delegate
		{
			if (callback != null)
			{
				callback(GripNetwork.Result.Success, result);
			}
		});
	}

	// Returns the column value for a record, or null if the record does not
	// carry that field (so the caller can skip the incomplete record). "recordid"
	// and "ownerid" are synthesised because the real server assigned them.
	private static GripField BuildColumn(string name, int recordId, GripField[] stored)
	{
		if (string.Equals(name, "recordid", StringComparison.OrdinalIgnoreCase))
		{
			GripField f = new GripField(name, GripField.GripFieldType.Int);
			f.mInt = recordId;
			return f;
		}
		if (string.Equals(name, "ownerid", StringComparison.OrdinalIgnoreCase))
		{
			GripField f = new GripField(name, GripField.GripFieldType.Int);
			f.mInt = LocalProfileId;
			return f;
		}
		if (stored != null)
		{
			foreach (GripField sf in stored)
			{
				if (sf != null && string.Equals(sf.mName, name, StringComparison.OrdinalIgnoreCase))
				{
					return sf;
				}
			}
		}
		return null;
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

	// ---- persistence ----------------------------------------------------------

	private static string StorePath
	{
		get { return Path.Combine(Application.persistentDataPath, kStoreFileName); }
	}

	private static void EnsureLoaded()
	{
		if (mLoaded)
		{
			return;
		}
		mLoaded = true;
		try
		{
			if (!File.Exists(StorePath))
			{
				return;
			}
			using (BinaryReader reader = new BinaryReader(File.Open(StorePath, FileMode.Open, FileAccess.Read)))
			{
				int tableCount = reader.ReadInt32();
				for (int t = 0; t < tableCount; t++)
				{
					string tableName = reader.ReadString();
					Table table = new Table();
					table.NextId = reader.ReadInt32();
					int recordCount = reader.ReadInt32();
					for (int r = 0; r < recordCount; r++)
					{
						int id = reader.ReadInt32();
						int fieldCount = reader.ReadInt32();
						GripField[] fields = new GripField[fieldCount];
						for (int f = 0; f < fieldCount; f++)
						{
							fields[f] = ReadField(reader);
						}
						table.Records[id] = fields;
					}
					Tables[tableName] = table;
				}
			}
		}
		catch (Exception ex)
		{
			// Corrupt or partial store: start clean rather than blocking multiplayer.
			Tables.Clear();
			Debug.LogWarning("OfflineBackend: could not load store: " + ex.Message);
		}
	}

	private static void Save()
	{
		try
		{
			using (BinaryWriter writer = new BinaryWriter(File.Open(StorePath, FileMode.Create, FileAccess.Write)))
			{
				writer.Write(Tables.Count);
				foreach (KeyValuePair<string, Table> kv in Tables)
				{
					writer.Write(kv.Key);
					writer.Write(kv.Value.NextId);
					writer.Write(kv.Value.Records.Count);
					foreach (KeyValuePair<int, GripField[]> rec in kv.Value.Records)
					{
						writer.Write(rec.Key);
						GripField[] fields = rec.Value ?? new GripField[0];
						writer.Write(fields.Length);
						foreach (GripField field in fields)
						{
							WriteField(writer, field);
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("OfflineBackend: could not save store: " + ex.Message);
		}
	}

	private static void WriteField(BinaryWriter w, GripField field)
	{
		if (field == null)
		{
			field = new GripField(string.Empty, GripField.GripFieldType.Null);
		}
		w.Write(field.mName ?? string.Empty);
		w.Write((int)field.mType);
		WriteNullableSByte(w, field.mByte);
		WriteNullableShort(w, field.mShort);
		WriteNullableInt(w, field.mInt);
		WriteNullableFloat(w, field.mFloat);
		bool hasString = field.mString != null;
		w.Write(hasString);
		if (hasString)
		{
			w.Write(field.mString);
		}
		WriteNullableBool(w, field.mBoolean);
		bool hasDate = field.mDateAndTime.HasValue;
		w.Write(hasDate);
		if (hasDate)
		{
			w.Write(field.mDateAndTime.Value.ToBinary());
		}
		byte[] bin = field.mBinaryData;
		w.Write(bin != null ? bin.Length : -1);
		if (bin != null)
		{
			w.Write(bin);
		}
		WriteNullableLong(w, field.mInt64);
	}

	private static GripField ReadField(BinaryReader r)
	{
		string name = r.ReadString();
		int type = r.ReadInt32();
		GripField field = new GripField(name, (GripField.GripFieldType)type);
		field.mByte = ReadNullableSByte(r);
		field.mShort = ReadNullableShort(r);
		field.mInt = ReadNullableInt(r);
		field.mFloat = ReadNullableFloat(r);
		if (r.ReadBoolean())
		{
			field.mString = r.ReadString();
		}
		field.mBoolean = ReadNullableBool(r);
		if (r.ReadBoolean())
		{
			field.mDateAndTime = DateTime.FromBinary(r.ReadInt64());
		}
		int binLen = r.ReadInt32();
		if (binLen >= 0)
		{
			field.mBinaryData = r.ReadBytes(binLen);
		}
		field.mInt64 = ReadNullableLong(r);
		return field;
	}

	private static void WriteNullableSByte(BinaryWriter w, sbyte? v) { w.Write(v.HasValue); if (v.HasValue) w.Write(v.Value); }
	private static void WriteNullableShort(BinaryWriter w, short? v) { w.Write(v.HasValue); if (v.HasValue) w.Write(v.Value); }
	private static void WriteNullableInt(BinaryWriter w, int? v) { w.Write(v.HasValue); if (v.HasValue) w.Write(v.Value); }
	private static void WriteNullableFloat(BinaryWriter w, float? v) { w.Write(v.HasValue); if (v.HasValue) w.Write(v.Value); }
	private static void WriteNullableBool(BinaryWriter w, bool? v) { w.Write(v.HasValue); if (v.HasValue) w.Write(v.Value); }
	private static void WriteNullableLong(BinaryWriter w, long? v) { w.Write(v.HasValue); if (v.HasValue) w.Write(v.Value); }

	private static sbyte? ReadNullableSByte(BinaryReader r) { return r.ReadBoolean() ? r.ReadSByte() : (sbyte?)null; }
	private static short? ReadNullableShort(BinaryReader r) { return r.ReadBoolean() ? r.ReadInt16() : (short?)null; }
	private static int? ReadNullableInt(BinaryReader r) { return r.ReadBoolean() ? r.ReadInt32() : (int?)null; }
	private static float? ReadNullableFloat(BinaryReader r) { return r.ReadBoolean() ? r.ReadSingle() : (float?)null; }
	private static bool? ReadNullableBool(BinaryReader r) { return r.ReadBoolean() ? r.ReadBoolean() : (bool?)null; }
	private static long? ReadNullableLong(BinaryReader r) { return r.ReadBoolean() ? r.ReadInt64() : (long?)null; }
}
