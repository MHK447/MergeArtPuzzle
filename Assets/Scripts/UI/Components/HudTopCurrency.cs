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
    private TextMeshProUGUI CashText;

    

    private CompositeDisposable disposables = new CompositeDisposable();
    
    



    public void Init()
    {
        disposables.Clear();

        GameRoot.Instance.UserData.Energycoin.Subscribe(count => {
            EnergycoinText.text = $"{count}";
        }).AddTo(disposables);

        GameRoot.Instance.UserData.Cash.Subscribe(x=> {
            
            CashText.text = $"{x}";
        }).AddTo(disposables);


        GameRoot.Instance.FoodSystem.EnergyCoinTimeProperty.Subscribe(x=> {
            var timevalue = GameRoot.Instance.FoodSystem.energy_add_time - x;
            EnergyCoinTimeText.text = ProjectUtility.GetTimeStringFormattingShort(timevalue);
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
