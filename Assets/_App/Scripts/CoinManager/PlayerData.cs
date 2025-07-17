using System;
using Assets.TJ.Scripts;
using UnityEngine;

public class Constant
{
	public static string DataKey_PlayerData="player_data";
	public static int    countSong         =19;
	public static int    priceUnlockSong   =1000;
}

public class PlayerData : BaseData
{
	public int    intDiamond => CoinsManager.Instance.GetTotalCoins();
	public int    currentSong;
	public bool[] listSongs;

	public Action<int> onChangeDiamond;

	public override void Init()
	{
		prefString=Constant.DataKey_PlayerData;
		if(PlayerPrefs.GetString(prefString).Equals(""))
		{
			ResetData();
		}

		base.Init();
	}


	public override void ResetData()
	{
		currentSong=0;
		listSongs  =new bool[Constant.countSong];

		for(int i=0;i<8;i++)
		{
			listSongs[i]=true;
		}

		Save();
	}

	public bool CheckLock(int id) { return this.listSongs[id]; }

	public void Unlock(int id)
	{
		if(!listSongs[id])
		{
			listSongs[id]=true;
		}

		Save();
	}

	public void AddDiamond(int a)
	{
		CoinsManager.Instance.AddCoins(a);

		onChangeDiamond?.Invoke(intDiamond);

		Save();
	}

	public bool CheckCanUnlock() { return intDiamond>=Constant.priceUnlockSong; }

	public void SubDiamond(int a)
	{
		CoinsManager.Instance.DeductCoins(a);

		onChangeDiamond?.Invoke(intDiamond);

		Save();
	}

	public void ChooseSong(int i)
	{
		currentSong=i;
		Save();
	}
}
