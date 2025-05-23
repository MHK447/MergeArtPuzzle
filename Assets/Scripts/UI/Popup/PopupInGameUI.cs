using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using TMPro;
using UniRx;

[UIPath("UI/Popup/PopupInGameUI")]
public class PopupInGameUI : UIBase
{
    [SerializeField]
    private Button BackBtn;

    [SerializeField]
    private List<InGameFoodSlotComponent> FoodComponentList = new List<InGameFoodSlotComponent>();


    public void Set(int stageidx)
    {   
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
