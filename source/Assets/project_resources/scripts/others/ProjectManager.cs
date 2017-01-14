using UnityEngine;
using System.Collections;

public static class ProjectManager 
{
	#region Project
	public static string bundleId 				= "com.VictorFisac.GalaxyBreak";
	#endregion

	#region Coins
	public static int defaultCoin				= 1;
	public static int bonusCoin					= 3;
	public static int extraCoin					= 5;
	#endregion

	#if !UNITY_EDITOR
	#region Play Services
	public static string leaderboardId				= "CgkI3K3ervwPEAIQAQ";

	public static string buyShipAchievement			= "CgkI3K3ervwPEAIQAg";

	public static string goodAchievement			= "CgkI3K3ervwPEAIQAw";
	public static int goodAchievementScore			= 10;

	public static string increibleAchievement		= "CgkI3K3ervwPEAIQBA";
	public static int increibleAchievementScore		= 25;

	public static string awesomeAchievement			= "CgkI3K3ervwPEAIQBQ";
	public static int awesomeAchievementScore		= 50;

	public static string bossAchievement			= "CgkI3K3ervwPEAIQBg";
	public static int bossAchievementScore			= 100;
	#endregion

	#region Billing
	public static string[] coinsPackIds 			= new string[3] { "coins_pack_1", "coins_pack_2", "coins_pack_3" };
	public static int[] coinsPackCoins				= new int[3] { 100, 300, 600 };
	public static string noAdsId 					= "no_ads";
	#endregion

	#region Advertising
	public static int rewardCoins					= 15;
	public static string[] adKeywords				= new string[3] { "game", "galaxy", "break" };
	public static string bannerId					= "ca-app-pub-4291494329942604/2948129572";
	public static string intersticialId				= "ca-app-pub-4291494329942604/5901595973";
	#if VIDEO_ADMOB
	public static string rewardedId					= "ca-app-pub-4291494329942604/7378329179";
	#elif VIDEO_ADCOLONY
	public static string adVersion					= "version:1.0,store:google";
	public static string adZoneId					= "vzf2e6f9ec94724ef78c";
	public static string adAppId					= "app2f4054dda44a4532b7";
	#endif
	#endregion

	#region Messages
	public static string playServicesTitle 			= "Google Play Services";
	public static string playServicesError 			= "Error when trying to connect to Google Play Services. Please, check your Internet connection and try again.";

	public static string shopTitle 					= "Shop";
	public static string shopError 					= "You don't have enought coins to buy this product.";

	public static string billingTitle 				= "In-App Purchasing";
	public static string billingComplete 			= "Product purchased successfully.";
	public static string billingError 				= "Error when trying to start purchase process. Please, check your Internet connection and try again.";

	public static string shareMessage 				= "Can you break my record in Galaxy Break? Download: https://goo.gl/FnAid5";

	public static string rateTitle					= "Do you like the game?";
	public static string rateMessage				= "If you are enjoying it, rate the game application in Google Play Store.";

	public static string rewardTitle				= "Rewarded Video";
	public static string rewardMessageError			= "Cannot load and display any rewarded advertisement. Check your Internet connection and try again later.";
	public static string rewardMessageSuccess		= "Congratulations! You have earned " + rewardCoins.ToString() + " coins by watching a rewarded video.";
	#endregion
	#endif
}