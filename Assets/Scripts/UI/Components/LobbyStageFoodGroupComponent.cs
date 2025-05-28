using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using TMPro;
using UniRx;

public class LobbyStageFoodGroupComponent : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI StagePercentText;

    [SerializeField]
    private Image ProgressStoreImage;


    [SerializeField]
    private Image StoreBgImage;

    [SerializeField]
    private Slider SliderValue;


    private int FoodMergeGroupIdx = 0;

    private int ClearGoalCount = 0;


    private CompositeDisposable disposables = new CompositeDisposable();


    public void Set(int foodmergegroupidx)
    {
        FoodMergeGroupIdx = foodmergegroupidx;

        var stageidx = GameRoot.Instance.UserData.Stagedata.Stageidx.Value;

        var td = Tables.Instance.GetTable<FoodMergeGroupInfo>().GetData(new KeyValuePair<int, int>(stageidx , FoodMergeGroupIdx));

        if(td != null)
        {
            ClearGoalCount = td.goal_count;

            var finddata = GameRoot.Instance.FoodSystem.FindFoodMergeGroupData(foodmergegroupidx);

            //ProgressStoreImage.sprite = StoreBgImage.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_InGame_Food , )

            if(finddata != null)
            {
                disposables.Clear();

                finddata.Stageclearstarcount.Subscribe(x=> {SetStageClearCheck(x);}).AddTo(disposables);

                SetStageClearCheck(finddata.Stageclearstarcount.Value);
            }

        }
    }

    public void SetStageClearCheck(int curcount)
    {
        SliderValue.value = (float)curcount / (float)ClearGoalCount;

        var percentvalue = ProgressStoreImage.fillAmount * 100;

        StagePercentText.text = $"{percentvalue.ToString("F0")}%";
    }

    void OnDestroy()
    {
        disposables.Clear();
    }

}
