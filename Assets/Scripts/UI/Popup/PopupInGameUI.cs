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
    private TextMeshProUGUI StarGoalValueText;

    public Transform GetStarImgTr;

    [SerializeField]
    private Slider SliderValue;

    [SerializeField]
    private Button TrashBtn;

    [SerializeField]
    private Button ShopBtn;

    [SerializeField]
    private Button NoAdsBtn;

    [SerializeField]
    private Button HomeBtn;

    [SerializeField]
    private HudTopCurrency HudTopCurrency;

    private int GoalValue = 0;

    private int MergeGroupFoodCount = 0;

    private CompositeDisposable disposables = new CompositeDisposable();


    protected override void Awake()
    {
        base.Awake();

        TrashBtn.onClick.AddListener(OnClickTrashBtn);

        ShopBtn.onClick.AddListener(OnClickShopBtn);

        NoAdsBtn.onClick.AddListener(OnClickNoAds);

        HomeBtn.onClick.AddListener(OnClickHomeBtn);
    }

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

        GameRoot.Instance.UserData.Starcoinvalue.Subscribe(x =>
        {
            CheckStarGoalValue();
        }).AddTo(disposables);

        GameRoot.Instance.ShopSystem.IsVipProperty.Subscribe(x =>
        {
            ProjectUtility.SetActiveCheck(NoAdsBtn.gameObject, !x);
        }).AddTo(disposables);

        CheckStarGoalValue();

        HudTopCurrency.SetStatus();
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

    public void OnClickTrashBtn()
    {
        var toastmesg = Tables.Instance.GetTable<Localize>().GetString("str_toastmsg_3");

        GameRoot.Instance.UISystem.OpenUI<PopupToastmessage>(popup => popup.Show("", toastmesg));
    }

    public void OnClickShopBtn()
    {
        GameRoot.Instance.UISystem.OpenUI<PageShop>(page =>
        {
            page.Init();
        });
    }

    void OnDestroy()
    {
        disposables.Clear();
    }

    public void OnClickHomeBtn()
    {

        var stageidx = GameRoot.Instance.UserData.Stagedata.Stageidx.Value;
        GameRoot.Instance.InGameSystem.GetInGame<InGameTycoon>().GoToLobby(stageidx);
    }

    void OnDisable()
    {
        disposables.Clear();
    }

    public void OnClickNoAds()
    {
        GameRoot.Instance.UISystem.OpenUI<PopupAdRemove>(popup =>
        {
            popup.Init();
        });
    }

}
