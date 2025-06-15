using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using BanpoFri;
using UniRx;
using System.Linq;
using UnityEngine.AI;
using NavMeshPlus.Components;
using Unity.VisualScripting;

public class InGameTycoon : InGameMode
{
    public IReactiveProperty<bool> MaxMode = new ReactiveProperty<bool>(true);

    public InGameChapterMap InGameChapterMap;

    private int ProductHeroIdxs = 0;
    public override void Load()
    {
        base.Load();

        var stageidx = GameRoot.Instance.UserData.Stagedata.Stageidx.Value;

        var td = Tables.Instance.GetTable<StageInfo>().GetData(stageidx);

        if (td != null)
        {
            GameRoot.Instance.InGameSystem.GetInGame<InGameTycoon>().StartGame();
            //GameRoot.Instance.UISystem.OpenUI<PageLobby>(pagelobby => pagelobby.Set(stageidx, 1));
        }

    }


    public void StartGame()
    {
        var stageidx = GameRoot.Instance.UserData.Stagedata.Stageidx.Value;

        var clearpercent = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.StartStage, stageidx);

        if (clearpercent <= 0)
        {
            GameRoot.Instance.UserData.Energycoin.Value = GameRoot.Instance.FoodSystem.start_energy_coin;
            GameRoot.Instance.UserData.AddRecordCount(Config.RecordCountKeys.StartStage, 1, stageidx);
        }

        GameRoot.Instance.UISystem.OpenUI<PopupInGameUI>(popup => popup.Set(stageidx));

        GameRoot.Instance.UISystem.GetUI<PageLobby>()?.Hide();

        if (InGameChapterMap != null)
        {
            Destroy(InGameChapterMap.gameObject);

            InGameChapterMap = null;
        }



        Addressables.InstantiateAsync("InGameChapter_Map").Completed += (handle) =>
        {
            InGameChapterMap = handle.Result.GetComponent<InGameChapterMap>();

            if (InGameChapterMap != null)
            {
                InGameChapterMap.Init();
            }
        };
    }


    public void GoToLobby(int stageidx)
    {
        GameRoot.Instance.UISystem.OpenUI<PageLobby>(pagelobby => pagelobby.Set(stageidx));


        ProjectUtility.SetActiveCheck(InGameChapterMap.gameObject, false);
    }


    protected override void LoadUI()
    {
        base.LoadUI();
        GameRoot.Instance.InGameSystem.InitPopups();
    }


    public override void UnLoad()
    {
        base.UnLoad();

        if (InGameChapterMap != null)
        {
            InGameChapterMap.EndGame();
        }
    }
}
