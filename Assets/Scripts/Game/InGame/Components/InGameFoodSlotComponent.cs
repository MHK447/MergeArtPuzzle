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

    private CompositeDisposable disposables = new CompositeDisposable();

    private int FoodGoalCount = 0;

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
            FoodGoalCount = td.goal_count;
            FoodMergeGroupData = GameRoot.Instance.FoodSystem.FindFoodMergeGroupData(FoodGroupIdx);

            if (FoodMergeGroupData != null)
            {
                GoalText.text = $"{FoodMergeGroupData.Foodcount.Value}/{td.goal_count}";
                GoalSlider.value = (float)FoodMergeGroupData.Foodcount.Value / (float)td.goal_count;

                ProjectUtility.SetActiveCheck(ClearObj, FoodMergeGroupData.Foodcount.Value >= td.goal_count);

                disposables.Clear();

                FoodMergeGroupData.Foodcount.Subscribe(count =>
                {
                    GoalText.text = $"{count}/{td.goal_count}";
                    GoalSlider.value = (float)count / (float)td.goal_count;
                }).AddTo(disposables);
            }
        }
    }




    public int RandSelectChoiceFood()
    {
        //예를들면 나올수있는 풀이 5개 까지 한정해놓고 

        //거기서 랜덤 돌리다가 8개 다 뽑앗으면 또 다른애 등장 이런식으로

        var finddata = GameRoot.Instance.FoodSystem.FindFoodMergeGroupData(FoodGroupIdx);


        List<int> FoodPoolList = new List<int>();
        List<int> FoodRandList = new List<int>();

        for (int i = 0; i < FoodGoalCount; ++i)
        {
            FoodPoolList.Add(i + 1);
        }




        if (finddata != null)
        {
            foreach (var drawfooddata in finddata.Drawfooddatas)
            {
                if (drawfooddata.Drawfoodcount >= 8)
                {
                    if (FoodPoolList.Contains(drawfooddata.Foodidx))
                    {
                        FoodPoolList.Remove(drawfooddata.Foodidx);
                    }
                }
            }

            for (int i = 0; i < GameRoot.Instance.FoodSystem.FoodMaxPool; ++i)
            {
                FoodRandList.Add(FoodPoolList[i]);
            }

            var randvalue = Random.Range(0, FoodRandList.Count);

            return FoodRandList[randvalue];

        }

        return 0;
    }

    public void CreateFood()
    {
        if (GameRoot.Instance.UserData.Foodcreateenergy.Value > 0)
        {
            var stageidx = GameRoot.Instance.UserData.CurMode.StageData.Stageidx.Value;

            var td = Tables.Instance.GetTable<FoodMergeGroupInfo>().GetData(new KeyValuePair<int, int>(stageidx, FoodGroupIdx));

            if (td != null)
            {
                GameRoot.Instance.UserData.Foodcreateenergy.Value -= 1;

                var randvalue = RandSelectChoiceFood();

                var drawfooddata = FoodMergeGroupData.Drawfooddatas.Find(x => x.Foodidx == randvalue);

                if (drawfooddata != null)
                {
                    drawfooddata.Drawfoodcount += 1;
                }
                else
                {
                    FoodMergeGroupData.Drawfooddatas.Add(new DrawFoodData()
                    {
                        Foodidx = randvalue,
                        Drawfoodcount = 1
                    });
                }

                GameRoot.Instance.InGameSystem.GetInGame<InGameTycoon>().InGameChapterMap.CreateFood(randvalue, 1, FoodGroupIdx);
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
        if (GameRoot.Instance.UserData.Energycoin.Value > 0 && !ClearObj.activeSelf)
        {
            CreateFood();
            GameRoot.Instance.UserData.Energycoin.Value -= 1;
        }

    }
}
