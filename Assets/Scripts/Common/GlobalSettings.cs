using UnityEngine;
using System.Collections;

public class GlobalSettings : MonoBehaviour
{
	// General
	public static GlobalString playerName = new GlobalString("playerName", "Anonymous");

	// Host
	public static GlobalString hostGameName = new GlobalString("hostGameName", "Anonymous's game");
	public static GlobalString hostNetworkAddress = new GlobalString("hostNetworkAddress", "Localhost");
	public static GlobalInt hostNetworkPort = new GlobalInt("hostNetworkPort", 7777);
	public static GlobalInt hostPlayerCount = new GlobalInt("hostPlayerCount", 4);

	// Join
	public static GlobalString joinNetworkAddress = new GlobalString("joinNetworkAddress", "Localhost");
	public static GlobalInt joinNetworkPort = new GlobalInt("joinNetworkPort", 7777);

	// Audio
	public static GlobalFloat mainAudioVolume = new GlobalFloat("mainAudioVolume", 1.0f);
	public static GlobalFloat musicAudioVolume = new GlobalFloat("musicAudioVolume", 1.0f);
	public static GlobalFloat effectsAudioVolume = new GlobalFloat("effectsAudioVolume", 1.0f);
	public static GlobalFloat uiAudioVolume = new GlobalFloat("uiAudioVolume", 1.0f);

	public static void SaveToDisk()
	{
		PlayerPrefs.Save();
	}

}
