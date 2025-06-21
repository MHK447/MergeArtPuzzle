using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using TMPro;
using UniRx;

public class HudTopCurrency : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI EnergycoinText;

    [SerializeField]
    private TextMeshProUGUI EnergyCoinTimeText;

    [SerializeField]
    private GameObject EnergyRoot;

    [SerializeField]
    private TextMeshProUGUI CashText;

    [SerializeField]
    private Button CashBtn;

    [SerializeField]
    private Button EnergyBtn;



    private CompositeDisposable disposables = new CompositeDisposable();


    void Awake()
    {
        CashBtn.onClick.AddListener(OnClickCash);
        EnergyBtn.onClick.AddListener(OnClickEnergy);
    }

    public void OnClickCash()
    {
        GameRoot.Instance.UISystem.OpenUI<PageShop>(page => page.Init());
    }

    public void OnClickEnergy()
    {
        GameRoot.Instance.UISystem.OpenUI<PopupPurchaseLightning>(popup => popup.Init());
    }

    public void SetStatus()
    {
        var isenergyad = GameRoot.Instance.ShopSystem.IsEnerrgyFree();

        EnergycoinText.text = !isenergyad ? Tables.Instance.GetTable<Localize>().GetString("str_max") : $"{GameRoot.Instance.UserData.Energycoin.Value}";
    }


    void OnEnable()
    {
        disposables.Clear();


        var isenergyad = GameRoot.Instance.ShopSystem.IsEnerrgyFree();

        var stageidx = GameRoot.Instance.UserData.Stagedata.Stageidx.Value;

        ProjectUtility.SetActiveCheck(EnergyRoot, isenergyad);


        GameRoot.Instance.UserData.Stagedata.Stageidx.Subscribe(x =>
        {
            var isenergyad = GameRoot.Instance.ShopSystem.IsEnerrgyFree();
            EnergycoinText.text = !isenergyad ? Tables.Instance.GetTable<Localize>().GetString("str_max") : $"{GameRoot.Instance.UserData.Energycoin.Value}";
        }).AddTo(disposables);


        GameRoot.Instance.UserData.Energycoin.Subscribe(count =>
        {
            var isenergyad = GameRoot.Instance.ShopSystem.IsEnerrgyFree();
            EnergycoinText.text = !isenergyad ? Tables.Instance.GetTable<Localize>().GetString("str_max") : $"{count}";
        }).AddTo(disposables);

        GameRoot.Instance.UserData.Cash.Subscribe(x =>
        {

            CashText.text = $"{x}";
        }).AddTo(disposables);


        GameRoot.Instance.FoodSystem.EnergyCoinTimeProperty.Subscribe(x =>
        {
            var timevalue = GameRoot.Instance.FoodSystem.energy_add_time - x;
            EnergyCoinTimeText.text = ProjectUtility.GetTimeStringFormattingShort(timevalue);
        }).AddTo(disposables);

        GameRoot.Instance.UserData.Energycoin.Subscribe(x =>
        {
            ProjectUtility.SetActiveCheck(EnergyRoot, x < GameRoot.Instance.FoodSystem.MaxEnergyCoin);
        }).AddTo(disposables);

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
