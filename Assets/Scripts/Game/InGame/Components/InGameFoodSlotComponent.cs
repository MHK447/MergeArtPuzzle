using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using TMPro;
using UniRx;

public class InGameFoodSlotComponent : MonoBehaviour
{
    [SerializeField]
    private Button FoodBtn;

    [SerializeField]
    private Image FoodImg;

    [SerializeField]
    private TextMeshProUGUI GoalText;

    [SerializeField]
    private Slider GoalSlider;

    [SerializeField]
    private GameObject ClearObj;

    private int FoodGroupIdx = 0;

    public int GetFoodGroupIdx { get { return FoodGroupIdx; } }

    private FoodMergeGroupData FoodMergeGroupData;

    private List<int> FoodList = new List<int>();
    
    private CompositeDisposable disposables = new CompositeDisposable();

    void Awake()
    {
        FoodBtn.onClick.AddListener(OnClickFoodBtn);
    }

    public void Set(int foodgroupidx)
    {
        FoodGroupIdx = foodgroupidx;

        var stageidx = GameRoot.Instance.UserData.CurMode.StageData.Stageidx.Value;

        var td = Tables.Instance.GetTable<FoodMergeGroupInfo>().GetData(new KeyValuePair<int, int>(stageidx, foodgroupidx));

        if (td != null)
        {
            FoodList = td.food_idx;
            FoodMergeGroupData = GameRoot.Instance.FoodSystem.FindFoodMergeData(FoodGroupIdx);

            if (FoodMergeGroupData != null)
            {
                GoalText.text = $"{FoodMergeGroupData.Foodcount.Value}/{td.goal_count}";
                GoalSlider.value = (float)FoodMergeGroupData.Foodcount.Value / (float)td.goal_count;
                
                ProjectUtility.SetActiveCheck(ClearObj , FoodMergeGroupData.Foodcount.Value >= td.goal_count);  

                disposables.Clear();

                FoodMergeGroupData.Foodcount.Subscribe(count => {
                    GoalText.text = $"{count}/{td.goal_count}";
                    GoalSlider.value = (float)count / (float)td.goal_count;
                }).AddTo(disposables);
            }
        }
    }

    public void CreateFood()
    {
        if (GameRoot.Instance.UserData.Foodcreateenergy.Value > 0)
        {
            var stageidx = GameRoot.Instance.UserData.CurMode.StageData.Stageidx.Value;

            var td = Tables.Instance.GetTable<FoodMergeGroupInfo>().GetData(new KeyValuePair<int, int>(stageidx, FoodGroupIdx));

            if(td != null)
            {
                GameRoot.Instance.UserData.Foodcreateenergy.Value -= 1;

                var randvalue = Random.Range(0, td.food_idx.Count);

                GameRoot.Instance.InGameSystem.GetInGame<InGameTycoon>().InGameChapterMap.CreateFood(td.food_idx[randvalue], 1, FoodGroupIdx);
            }
        }   
    }

    void OnDestroy()
    {
        disposables.Clear();
    }

    void OnDisable()
    {
        disposables.Clear();
    }


    public void OnClickFoodBtn()
    {
        if(GameRoot.Instance.UserData.Energycoin.Value > 0 && !ClearObj.activeSelf)
        {
            CreateFood();
            GameRoot.Instance.UserData.Energycoin.Value -= 1;
        }

    }
}
