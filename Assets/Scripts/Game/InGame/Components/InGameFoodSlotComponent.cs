using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using TMPro;
using UniRx;
using Unity.VisualScripting;

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
    private Button ClearBtn;

    private int FoodGroupIdx = 0;

    public int GetFoodGroupIdx { get { return FoodGroupIdx; } }

    private FoodMergeGroupData FoodMergeGroupData;

    private CompositeDisposable disposables = new CompositeDisposable();

    private int FoodGoalCount = 0;

    void Awake()
    {
        FoodBtn.onClick.AddListener(OnClickFoodBtn);
        ClearBtn.onClick.AddListener(OnClickClearBtn);
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

                ProjectUtility.SetActiveCheck(ClearBtn.gameObject, FoodMergeGroupData.Foodcount.Value >= td.goal_count);

                disposables.Clear();

                FoodMergeGroupData.Foodcount.Subscribe(count =>
                {
                    GoalText.text = $"{count}/{td.goal_count}";
                    GoalSlider.value = (float)count / (float)td.goal_count;
                    ProjectUtility.SetActiveCheck(ClearBtn.gameObject, count >= td.goal_count);
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
                if (FoodPoolList.Count <= i) break;

                FoodRandList.Add(FoodPoolList[i]);
            }

            var randvalue = Random.Range(0, FoodRandList.Count);

            if (FoodRandList.Count == 0)
            {
                var toastmesg = Tables.Instance.GetTable<Localize>().GetString("str_toastmsg_2");

                GameRoot.Instance.UISystem.OpenUI<PopupToastmessage>(popup => popup.Show("", toastmesg));

                return -1;
            }
            return FoodRandList[randvalue];

        }

        return -1;
    }

    public void OnClickClearBtn()
    {
        var stageidx = GameRoot.Instance.UserData.CurMode.StageData.Stageidx.Value;

        GameRoot.Instance.InGameSystem.GetInGame<InGameTycoon>().GoToLobby(stageidx, FoodGroupIdx);
    }

    public void CreateFood(int foodidx)
    {
        var stageidx = GameRoot.Instance.UserData.CurMode.StageData.Stageidx.Value;

        var td = Tables.Instance.GetTable<FoodMergeGroupInfo>().GetData(new KeyValuePair<int, int>(stageidx, FoodGroupIdx));

        if (td != null)
        {

            var randvalue = foodidx;

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
        if (GameRoot.Instance.UserData.Energycoin.Value <= 0)
        {
            GameRoot.Instance.UISystem.OpenUI<PopupPurchaseLightning>(popup => popup.Init());
            return;
        }

        if (GameRoot.Instance.InGameSystem.GetInGame<InGameTycoon>().InGameChapterMap.IsFoodMaxCountCheck())
        {

            var toastmesg = Tables.Instance.GetTable<Localize>().GetString("str_toastmsg_1");

            GameRoot.Instance.UISystem.OpenUI<PopupToastmessage>(popup => popup.Show("", toastmesg));
            return;
        }

        var randselectfoodidx = RandSelectChoiceFood();

        if (randselectfoodidx > 0)
        {
            CreateFood(randselectfoodidx);
            GameRoot.Instance.UserData.Energycoin.Value -= 1;
        }


    }
}
