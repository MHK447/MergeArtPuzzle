using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using TMPro;

[UIPath("UI/Popup/PopupCollectionInfo")]
public class PopupCollectionInfo : UIBase
{
    [SerializeField]
    private Button RewardAdBtn;

    [SerializeField]
    private Button CashBtn;

    [SerializeField]
    private TextMeshProUGUI RewardAdText;

    [SerializeField]
    private TextMeshProUGUI TimeText;

    [SerializeField]
    private TextMeshProUGUI NameText;

    [SerializeField]
    private TextMeshProUGUI DescText;

    [SerializeField]
    private GameObject FreeObj;

    [SerializeField]
    private GameObject AdObj;


    private int AdCostValue = 25;

    private int EnergyRewardValue = 0;

    private InGameEnergyAd EnergyAd;

    private int EnergyIdx = 0;


    protected override void Awake()
    {
        base.Awake();

        RewardAdBtn.onClick.AddListener(OnClickRewardAdBtn);
        CashBtn.onClick.AddListener(OnClickCashBtn);
    }

    public void Set(int energyidx, InGameEnergyAd energyad)
    {
        var td = Tables.Instance.GetTable<InGameEnergyInfo>().GetData(energyidx);
        if (td != null)
        {

            EnergyIdx = energyidx;
            EnergyRewardValue = td.value;
            RewardAdText.text = EnergyRewardValue.ToString();
            NameText.text = Tables.Instance.GetTable<Localize>().GetString(td.name);
            DescText.text = Tables.Instance.GetTable<Localize>().GetString(td.desc);

            ProjectUtility.SetActiveCheck(AdObj, energyidx > 1);
            ProjectUtility.SetActiveCheck(FreeObj, energyidx == 1);


            EnergyAd = energyad;
        }
    }

    public override void Hide()
    {
        base.Hide();
        EnergyAd.SelectActiveCheck(false);
    }


    void Update()
    {
        if (EnergyAd == null) return;

        TimeText.text = Utility.GetTimeStringFormattingShort((int)EnergyAd.StartTime);

        if (EnergyAd.StartTime <= 0f)
        {
            Hide();
        }
    }



    public void OnClickRewardAdBtn()
    {
        if (EnergyIdx == 1)
        {
            ClickReward();
        }
        else
        {
            GameRoot.Instance.GetAdManager.ShowRewardedAd(() =>
            {
                ClickReward();
            });
        }
    }

    public void ClickReward()
    {
        GameRoot.Instance.UserData.Energycoin.Value += EnergyRewardValue;
        ProjectUtility.SetActiveCheck(EnergyAd.gameObject, false);
        Hide();
    }

    public void OnClickCashBtn()
    {
        if (AdCostValue <= GameRoot.Instance.UserData.Cash.Value)
        {
            GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Currency, (int)Config.CurrencyID.Cash, -AdCostValue);
            GameRoot.Instance.UserData.Energycoin.Value += EnergyRewardValue;
            ProjectUtility.SetActiveCheck(EnergyAd.gameObject, false);

            Hide();
        }
    }
}