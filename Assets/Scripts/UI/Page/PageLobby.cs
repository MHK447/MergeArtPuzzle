using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using UnityEngine.AddressableAssets;
using UniRx;

[UIPath("UI/Page/PageLobby")]
public class PageLobby : UIBase
{
    [SerializeField]
    private Image StageImg;

    [SerializeField]
    private Image StageClearProgress;


    [SerializeField]
    private TextMeshProUGUI PercentText;

    [SerializeField]
    private TextMeshProUGUI LevelText;

    [SerializeField]
    private TextMeshProUGUI StarCountText;

    [SerializeField]
    private ButtonPressed PressedBtn;

    [SerializeField]
    private LobbyStageFoodGroupComponent LobbyStageFoodGroupComponent;
    


    [SerializeField]
    private Button StartBtn;



    private int StageIdx = 0;

    private int GoalClearCount = 0;

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

        if(stagetd != null)
        {
            GoalClearCount = stagetd.next_stage_count;

            disposables.Clear();

            CheckClearPercent();
            
            GameRoot.Instance.UserData.Nextstagecount.Subscribe(x=> {CheckClearPercent();}).AddTo(disposables);

            LevelText.text = $"Lv.{stageidx}";

            var count = GameRoot.Instance.UserData.Stageenergycount.Value;

            var starcount = GameRoot.Instance.UserData.Starcoinvalue.Value;

            StarCountText.text = starcount.ToString();

            StarCountCheck();

            disposables.Clear();

            GameRoot.Instance.UserData.Starcoinvalue.Subscribe(x=> {StarCountCheck();}).AddTo(disposables);

            LobbyStageFoodGroupComponent.Set(selectfoodgroupidx);
        }
    } 

    public void StarCountCheck()
    {
        var starcount = GameRoot.Instance.UserData.Starcoinvalue.Value;

        StarCountText.text = starcount.ToString();


        ProjectUtility.SetActiveCheck(PressedBtn.gameObject , starcount > 0);
        ProjectUtility.SetActiveCheck(StartBtn.gameObject , starcount <= 0);
    }


    public void CheckClearPercent()
    {
        var clearpercent = GameRoot.Instance.UserData.Nextstagecount.Value;

        var fillvalue = (float)clearpercent / (float)GoalClearCount;;

        StageClearProgress.fillAmount = fillvalue;

        PercentText.text = $"{fillvalue.ToString("F0")}%";
    }

    public void OnClickPressed()
    {
        if(GameRoot.Instance.UserData.Starcoinvalue.Value > 0)
        {
            GameRoot.Instance.UserData.Starcoinvalue.Value -= 1;

            //GameRoot.Instance.UserData.Stageclearstarcount.Value += 1;
        }
    }


    public void OnClickStart()
    {
       // GameRoot.Instance.InGameSystem.GetInGame<InGameTycoon>().StartGame();
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
