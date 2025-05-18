using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class FoodInfoData
    {
        [SerializeField]
		private int _foodidx;
		public int foodidx
		{
			get { return _foodidx;}
			set { _foodidx = value;}
		}
		[SerializeField]
		private string _food_img;
		public string food_img
		{
			get { return _food_img;}
			set { _food_img = value;}
		}

    }

    [System.Serializable]
    public class FoodInfo : Table<FoodInfoData, int>
    {
    }
}

