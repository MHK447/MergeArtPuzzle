using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using UnityEngine.AddressableAssets;
using UniRx;
using Unity.VisualScripting;

[UIPath("UI/Page/PageLobby")]
public class PageLobby : UIBase
{
    [SerializeField]
    private Transform ContentsRoot;


    [SerializeField]
    private TextMeshProUGUI LevelText;

    [SerializeField]
    private TextMeshProUGUI StarCountText;

    [SerializeField]
    private ButtonPressed PressedBtn;

    [SerializeField]
    private Transform PressedBtnRoot;

    [SerializeField]
    private Slider SliderValue;

    [SerializeField]
    private TextMeshProUGUI StarGoalValueText;

    private LobbyFoodComponentGroup LobbyStageFoodGroupComponent;



    [SerializeField]
    private Button StartBtn;



    private int StageIdx = 0;

    private int GoalClearCount = 0;

    private int SelectFoodGroupIdx = 0;

    private CompositeDisposable disposables = new CompositeDisposable();


    protected override void Awake()
    {
        base.Awake();

        StartBtn.onClick.AddListener(OnClickStart);
        PressedBtn.OnPressed = OnClickPressed;


    }

    public void Set(int stageidx, int selectfoodgroupidx)
    {
        StageIdx = stageidx;

        var stagetd = Tables.Instance.GetTable<StageInfo>().GetData(stageidx);

        if (stagetd != null)
        {
            GoalClearCount = stagetd.next_stage_count;

            disposables.Clear();

            //CheckClearPercent();

            //GameRoot.Instance.UserData.Nextstagecount.Subscribe(x => { CheckClearPercent(); }).AddTo(disposables);

            LevelText.text = $"Lv.{stageidx}";

            var count = GameRoot.Instance.UserData.Stageenergycount.Value;

            var starcount = GameRoot.Instance.UserData.Starcoinvalue.Value;

            StarCountText.text = starcount.ToString();

            StarCountCheck();

            disposables.Clear();

            GameRoot.Instance.UserData.Starcoinvalue.Subscribe(x => { StarCountCheck(); }).AddTo(disposables);

            if (LobbyStageFoodGroupComponent != null)
            {
                Destroy(LobbyStageFoodGroupComponent.gameObject);
                LobbyStageFoodGroupComponent = null;
            }
            var MergeGroupFoodCount = 0;

            foreach (var groupdata in GameRoot.Instance.UserData.Foodmergegroupdatas)
            {
                MergeGroupFoodCount += groupdata.Foodcount.Value;
            }

            var GoalValue = 0;

            var tdlist = Tables.Instance.GetTable<FoodMergeGroupInfo>().DataList.ToList().FindAll(x => x.stageidx == stageidx);

            for (int i = 0; i < tdlist.Count; i++)
            {
                var td = tdlist[i];

                GoalValue += td.goal_count;
            }


            StarGoalValueText.text = $"{MergeGroupFoodCount}/{GoalValue}";

            SliderValue.value = (float)MergeGroupFoodCount / (float)GoalValue;

            Addressables.InstantiateAsync($"Stage_Map_{stageidx.ToString("D2")}").Completed += (handle) =>
            {
                LobbyStageFoodGroupComponent = handle.Result.GetComponent<LobbyFoodComponentGroup>();

                if (LobbyStageFoodGroupComponent != null)
                {
                    LobbyStageFoodGroupComponent.Set(GameRoot.Instance.UserData.Stagedata.Stageidx.Value);

                    LobbyStageFoodGroupComponent.transform.SetParent(ContentsRoot, false);

                    LobbyStageFoodGroupComponent.transform.localPosition = Vector3.zero;
                }
            };


            //LobbyStageFoodGroupComponent.Set(selectfoodgroupidx);
        }
    }

    public void StarCountCheck()
    {
        var starcount = GameRoot.Instance.UserData.Starcoinvalue.Value;

        StarCountText.text = starcount.ToString();


        ProjectUtility.SetActiveCheck(PressedBtnRoot.gameObject, starcount > 0);
        ProjectUtility.SetActiveCheck(StartBtn.gameObject, starcount <= 0);
    }


    // public void CheckClearPercent()
    // {
    //     var clearpercent = GameRoot.Instance.UserData.Nextstagecount.Value;

    //     var fillvalue = (float)clearpercent / (float)GoalClearCount; ;

    //     StageClearProgress.fillAmount = fillvalue;

    //     PercentText.text = $"{fillvalue.ToString("F0")}%";
    // }

    public void OnClickPressed()
    {
        var stageidx = GameRoot.Instance.UserData.Stagedata.Stageidx.Value;

        var tdlist = Tables.Instance.GetTable<FoodMergeGroupInfo>().DataList.FindAll(x => x.stageidx == stageidx).ToList();

        foreach (var td in tdlist)
        {
            var findgroupdata = GameRoot.Instance.FoodSystem.FindFoodMergeGroupData(td.mergeidx);

            if (findgroupdata != null)
            {
                if (findgroupdata.Stageclearstarcount.Value < td.goal_count)
                {
                    findgroupdata.Stageclearstarcount.Value += 1;

                    GameRoot.Instance.UserData.Starcoinvalue.Value -= 1;

                    break;
                }

            }
        }

        CheckNextStage();
    }


    public void OnClickStart()
    {
        GameRoot.Instance.InGameSystem.GetInGame<InGameTycoon>().StartGame();
    }


    public void CheckNextStage()
    {
        bool isclear = true;

        var tdlist = Tables.Instance.GetTable<FoodMergeGroupInfo>().DataList.FindAll(x => x.stageidx == StageIdx).ToList();

        foreach (var td in tdlist)
        {
            var findgroupdata = GameRoot.Instance.FoodSystem.FindFoodMergeGroupData(td.mergeidx);

            if (findgroupdata.Stageclearstarcount.Value < td.goal_count)
            {
                isclear = false;
                break;
            }
        }

        if (isclear)
        {
            GameRoot.Instance.UserData.Stagedata.Stageidx.Value += 1;

            LevelText.text = $"Lv.{GameRoot.Instance.UserData.Stagedata.Stageidx.Value}";

            GameRoot.Instance.UserData.Foodmergegroupdatas.Clear();

            GameRoot.Instance.UserData.Save();
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
}
