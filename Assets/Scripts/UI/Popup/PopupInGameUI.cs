using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using TMPro;    
using UniRx;
using System.Linq;


[UIPath("UI/Popup/PopupInGameUI")]
public class PopupInGameUI : UIBase
{
    [SerializeField]
    private Button BackBtn;

    [SerializeField]
    private List<InGameFoodSlotComponent> FoodComponentList = new List<InGameFoodSlotComponent>();

    [SerializeField]
    private HudTopCurrency TopCurrency;


    public void Set(int stageidx)
    {   
        var tdlist = Tables.Instance.GetTable<FoodMergeGroupInfo>().DataList.ToList().FindAll(x => x.stageidx == stageidx);

        for(int i = 0; i < tdlist.Count; i++)
        {
            var td = tdlist[i];

            FoodComponentList[i].Set(td.mergeidx);
        }

        TopCurrency.Init();
    }

    public InGameFoodSlotComponent GetInGameFoodSlotComponent(int foodgroupidx)
    {
        var finddata = FoodComponentList.Find(x => x.GetFoodGroupIdx == foodgroupidx);

        if(finddata != null)
        {
            return finddata;
        }

        return null;
    }

}
