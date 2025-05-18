using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class FoodMergeGroupInfoData
    {
        [SerializeField]
		private int _stageidx;
		public int stageidx
		{
			get { return _stageidx;}
			set { _stageidx = value;}
		}
		[SerializeField]
		private int _mergeidx;
		public int mergeidx
		{
			get { return _mergeidx;}
			set { _mergeidx = value;}
		}
		[SerializeField]
		private List<int> _food_idx;
		public List<int> food_idx
		{
			get { return _food_idx;}
			set { _food_idx = value;}
		}
		[SerializeField]
		private int _goal_count;
		public int goal_count
		{
			get { return _goal_count;}
			set { _goal_count = value;}
		}

    }

    [System.Serializable]
    public class FoodMergeGroupInfo : Table<FoodMergeGroupInfoData, KeyValuePair<int,int>>
    {
    }
}

