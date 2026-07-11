using System.Runtime.InteropServices;
using UnityEngine;

public class GameCenterBinding
{
#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern bool _gameCenterIsGameCenterAvailable();
#else
	private static bool _gameCenterIsGameCenterAvailable() { return false; }
#endif

	public static bool isGameCenterAvailable()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			return _gameCenterIsGameCenterAvailable();
		}
		return false;
	}

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _gameCenterAuthenticateLocalPlayer();
#else
	private static void _gameCenterAuthenticateLocalPlayer() { }
#endif

	public static void authenticateLocalPlayer()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterAuthenticateLocalPlayer();
		}
	}

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern bool _gameCenterIsPlayerAuthenticated();
#else
	private static bool _gameCenterIsPlayerAuthenticated() { return false; }
#endif

	public static bool isPlayerAuthenticated()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			return _gameCenterIsPlayerAuthenticated();
		}
		return false;
	}

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern string _gameCenterPlayerAlias();
#else
	private static string _gameCenterPlayerAlias() { return string.Empty; }
#endif

	public static string playerAlias()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			return _gameCenterPlayerAlias();
		}
		return string.Empty;
	}

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern string _gameCenterPlayerIdentifier();
#else
	private static string _gameCenterPlayerIdentifier() { return string.Empty; }
#endif

	public static string playerIdentifier()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			return _gameCenterPlayerIdentifier();
		}
		return string.Empty;
	}

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern bool _gameCenterIsUnderage();
#else
	private static bool _gameCenterIsUnderage() { return false; }
#endif

	public static bool isUnderage()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			return _gameCenterIsUnderage();
		}
		return false;
	}

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _gameCenterRetrieveFriends();
#else
	private static void _gameCenterRetrieveFriends() { }
#endif

	public static void retrieveFriends()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterRetrieveFriends();
		}
	}

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _gameCenterLoadPlayerData(string playerIds);
#else
	private static void _gameCenterLoadPlayerData(string playerIds) { }
#endif

	public static void loadPlayerData(string[] playerIdArray)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterLoadPlayerData(string.Join(",", playerIdArray));
		}
	}

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _gameCenterLoadLeaderboardLeaderboardTitles();
#else
	private static void _gameCenterLoadLeaderboardLeaderboardTitles() { }
#endif

	public static void loadLeaderboardTitles()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterLoadLeaderboardLeaderboardTitles();
		}
	}

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _gameCenterReportScore(long score, string leaderboardId);
#else
	private static void _gameCenterReportScore(long score, string leaderboardId) { }
#endif

	public static void reportScore(long score, string leaderboardId)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterReportScore(score, leaderboardId);
		}
	}

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _gameCenterShowLeaderboardWithTimeScope(int timeScope);
#else
	private static void _gameCenterShowLeaderboardWithTimeScope(int timeScope) { }
#endif

	public static void showLeaderboardWithTimeScope(GameCenterLeaderboardTimeScope timeScope)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterShowLeaderboardWithTimeScope((int)timeScope);
		}
	}

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _gameCenterShowLeaderboardWithTimeScopeAndLeaderboardId(int timeScope, string leaderboardId);
#else
	private static void _gameCenterShowLeaderboardWithTimeScopeAndLeaderboardId(int timeScope, string leaderboardId) { }
#endif

	public static void showLeaderboardWithTimeScopeAndLeaderboard(GameCenterLeaderboardTimeScope timeScope, string leaderboardId)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterShowLeaderboardWithTimeScopeAndLeaderboardId((int)timeScope, leaderboardId);
		}
	}

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _gameCenterRetrieveScores(bool friendsOnly, int timeScope, int start, int end);
#else
	private static void _gameCenterRetrieveScores(bool friendsOnly, int timeScope, int start, int end) { }
#endif

	public static void retrieveScores(bool friendsOnly, GameCenterLeaderboardTimeScope timeScope, int start, int end)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterRetrieveScores(friendsOnly, (int)timeScope, start, end);
		}
	}

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _gameCenterRetrieveScoresForLeaderboard(bool friendsOnly, int timeScope, int start, int end, string leaderboardId);
#else
	private static void _gameCenterRetrieveScoresForLeaderboard(bool friendsOnly, int timeScope, int start, int end, string leaderboardId) { }
#endif

	public static void retrieveScores(bool friendsOnly, GameCenterLeaderboardTimeScope timeScope, int start, int end, string leaderboardId)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterRetrieveScoresForLeaderboard(friendsOnly, (int)timeScope, start, end, leaderboardId);
		}
	}

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _gameCenterRetrieveScoresForPlayerId(string playerId);
#else
	private static void _gameCenterRetrieveScoresForPlayerId(string playerId) { }
#endif

	public static void retrieveScoresForPlayerId(string playerId)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterRetrieveScoresForPlayerId(playerId);
		}
	}

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _gameCenterRetrieveScoresForPlayerIdAndLeaderboard(string playerId, string leaderboardId);
#else
	private static void _gameCenterRetrieveScoresForPlayerIdAndLeaderboard(string playerId, string leaderboardId) { }
#endif

	public static void retrieveScoresForPlayerId(string playerId, string leaderboardId)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterRetrieveScoresForPlayerIdAndLeaderboard(playerId, leaderboardId);
		}
	}

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _gameCenterReportAchievement(string identifier, float percent);
#else
	private static void _gameCenterReportAchievement(string identifier, float percent) { }
#endif

	public static void reportAchievement(string identifier, float percent)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterReportAchievement(identifier, percent);
		}
	}

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _gameCenterGetAchievements();
#else
	private static void _gameCenterGetAchievements() { }
#endif

	public static void getAchievements()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterGetAchievements();
		}
	}

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _gameCenterResetAchievements();
#else
	private static void _gameCenterResetAchievements() { }
#endif

	public static void resetAchievements()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterResetAchievements();
		}
	}

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _gameCenterShowAchievements();
#else
	private static void _gameCenterShowAchievements() { }
#endif

	public static void showAchievements()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterShowAchievements();
		}
	}

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _gameCenterRetrieveAchievementMetadata();
#else
	private static void _gameCenterRetrieveAchievementMetadata() { }
#endif

	public static void retrieveAchievementMetadata()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterRetrieveAchievementMetadata();
		}
	}

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void _gameCenterShowCompletionBannerForAchievements();
#else
	private static void _gameCenterShowCompletionBannerForAchievements() { }
#endif

	public static void showCompletionBannerForAchievements()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterShowCompletionBannerForAchievements();
		}
	}
}
