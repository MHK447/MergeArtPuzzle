using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BanpoFri;
using UnityEngine;

public class LobbyStageMapComponent : MonoBehaviour
{
    [SerializeField]
    private List<LobbyStageFoodGroupComponent> FoodGroupList = new List<LobbyStageFoodGroupComponent>();


    public  void Set(int stageidx)
    {
        foreach(var foodgroup in FoodGroupList)
        {
            ProjectUtility.SetActiveCheck(foodgroup.gameObject , false);
        }

        var tdlist = Tables.Instance.GetTable<FoodMergeGroupInfo>().DataList.FindAll(x=> x.stageidx == stageidx).ToList();

        for(int i = 0; i < tdlist.Count; ++i)
        {
             ProjectUtility.SetActiveCheck(FoodGroupList[i].gameObject , true);
             FoodGroupList[i].Set(tdlist[i].mergeidx);
        }

    }
}
