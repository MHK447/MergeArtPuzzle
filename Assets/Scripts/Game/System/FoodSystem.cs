using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSystem
{

    public void Create()
    {
        GameRoot.Instance.UserData.Foodcreateenergy.Value = 100;
    }





    public FoodMergeGroupData FindFoodMergeData(int foodgroupidx)
    {
        var finddata = GameRoot.Instance.UserData.Foodmergegroupdatas.Find(x => x.Foodmergeidx == foodgroupidx);

        if (finddata != null)
        {
            return finddata;
        }

        return null;
    }


}
