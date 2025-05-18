using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using TMPro;


public class InGameFoodSlotComponent : MonoBehaviour
{
    [SerializeField]
    private Button FoodBtn;

    [SerializeField]
    private Image FoodImg;

    [SerializeField]
    private TextMeshProUGUI GoalText;

    private int FoodGroupIdx = 0;

    private FoodMergeGroupData FoodMergeGroupData;

    private List<int> FoodList = new List<int>();

    void Awake()
    {
        FoodBtn.onClick.AddListener(OnClickFoodBtn);
    }

    public void Set(int foodgroupidx)
    {
        FoodGroupIdx = foodgroupidx;

        var stageidx = GameRoot.Instance.UserData.CurMode.StageData.Stageidx.Value;

        var td = Tables.Instance.GetTable<FoodMergeGroupInfo>().GetData(new KeyValuePair<int, int>(stageidx , foodgroupidx));
        
        if(td != null)
        {
            FoodList = td.food_idx;
            FoodMergeGroupData = GameRoot.Instance.FoodSystem.FindFoodMergeData(FoodGroupIdx);
        }
    }
    
    public void CreateFood()
    {
        if(GameRoot.Instance.UserData.Foodcreateenergy.Value > 0)
        {
            GameRoot.Instance.UserData.Foodcreateenergy.Value--;
            GameRoot.Instance.InGameSystem.GetInGame<InGameTycoon>().InGameChapterMap.CreateFood(1);
        }
    }


    public void OnClickFoodBtn()
    {
        CreateFood();
    }
}
