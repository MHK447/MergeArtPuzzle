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

    [SerializeField]
    private TextMeshProUGUI StarGoalValueText;

    public Transform GetStarImgTr;

    [SerializeField]
    private Slider SliderValue;

    private int GoalValue = 0;

    private int MergeGroupFoodCount = 0;

    private CompositeDisposable disposables = new CompositeDisposable();

    public void Set(int stageidx)
    {
        MergeGroupFoodCount = 0;
        GoalValue = 0;

        foreach (var food in FoodComponentList)
        {
            ProjectUtility.SetActiveCheck(food.gameObject, false);
        }

        var tdlist = Tables.Instance.GetTable<FoodMergeGroupInfo>().DataList.ToList().FindAll(x => x.stageidx == stageidx);

        for (int i = 0; i < tdlist.Count; i++)
        {
            var td = tdlist[i];

            GoalValue += td.goal_count;

            ProjectUtility.SetActiveCheck(FoodComponentList[i].gameObject, true);

            FoodComponentList[i].Set(td.mergeidx);
        }
        
        disposables.Clear();

        GameRoot.Instance.UserData.Starcoinvalue.Subscribe(x=> {
            CheckStarGoalValue();
        }).AddTo(disposables);

        CheckStarGoalValue();

        TopCurrency.Init();
    }

    public void CheckStarGoalValue()
    {
        MergeGroupFoodCount = 0;

        foreach (var groupdata in GameRoot.Instance.UserData.Foodmergegroupdatas)
        {
            MergeGroupFoodCount += groupdata.Foodcount.Value;
        }

        StarGoalValueText.text = $"{MergeGroupFoodCount}/{GoalValue}";

        SliderValue.value = (float)MergeGroupFoodCount / (float)GoalValue;
    }

    public InGameFoodSlotComponent GetInGameFoodSlotComponent(int foodgroupidx)
    {
        var finddata = FoodComponentList.Find(x => x.GetFoodGroupIdx == foodgroupidx);

        if (finddata != null)
        {
            return finddata;
        }

        return null;
    }

    void OnDestroy()
    {
        disposables.Clear();
    }

    void OnDisable()
    {
        disposables.Clear();
    }

}
