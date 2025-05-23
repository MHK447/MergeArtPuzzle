using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using System.Linq;
using UniRx;
public class FoodSystem
{
    public IReactiveProperty<int> EnergyCoinTimeProperty = new ReactiveProperty<int>(0);

    private float deltatime = 0f;

    public int energy_add_time = 0;

    public int start_energy_coin = 0;

    public void Create()
    {
        GameRoot.Instance.UserData.Foodcreateenergy.Value = 100;
        EnergyCoinTimeProperty.Value = 0;

        energy_add_time = Tables.Instance.GetTable<Define>().GetData("energy_add_time").value;
        start_energy_coin = Tables.Instance.GetTable<Define>().GetData("start_energy_coin").value;
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

    public void OneSecondUpdate()
    {
        EnergyCoinTimeProperty.Value += 1;

        if (EnergyCoinTimeProperty.Value >= energy_add_time)
        {
            GameRoot.Instance.UserData.Energycoin.Value += 1;
            EnergyCoinTimeProperty.Value = 0;
        }
    }

}
