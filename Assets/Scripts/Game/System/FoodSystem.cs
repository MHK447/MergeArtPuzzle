using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using System.Linq;
using UniRx;
public class FoodSystem
{

    public void Create()
    {
        GameRoot.Instance.UserData.Foodcreateenergy.Value = 100;
    }


    public FoodMergeGroupData FindFoodMergeData(int foodgroupidx)
    {
        var stageidx = GameRoot.Instance.UserData.CurMode.StageData.Stageidx.Value;

        var finddata = GameRoot.Instance.UserData.Foodmergegroupdatas.Find(x => x.Foodmergeidx == foodgroupidx);

        if (finddata != null)
        {
            return finddata;
        }
        else
        {
            var newdata = new FoodMergeGroupData
            {
                Foodmergeidx = foodgroupidx,    
                Foodcount = new ReactiveProperty<int>(0),
                Ingamefooddatas = new List<InGameFoodData>()
            };
            GameRoot.Instance.UserData.Foodmergegroupdatas.Add(newdata);
            return newdata;
        }

    }

}
