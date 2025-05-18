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

        var stageidx = GameRoot.Instance.UserData.CurMode.StageData.Stageidx.Value;

        var td = Tables.Instance.GetTable<StageInfo>().GetData(stageidx);

        if (td != null)
        {
            GameRoot.Instance.UISystem.OpenUI<PageLobby>();
        }

    }


    public void StartGame()
    {
        var stageidx = GameRoot.Instance.UserData.CurMode.StageData.Stageidx.Value;

        GameRoot.Instance.UISystem.OpenUI<PopupInGameUI>();

        GameRoot.Instance.UISystem.GetUI<PageLobby>().Hide();

        if (InGameChapterMap != null)
        {
            Destroy(InGameChapterMap.gameObject);

            InGameChapterMap = null;
        }



        Addressables.InstantiateAsync($"InGameChapter_{stageidx.ToString("D2")}").Completed += (handle) =>
        {
            InGameChapterMap = handle.Result.GetComponent<InGameChapterMap>();

            if(InGameChapterMap != null)
            {
                InGameChapterMap.Init();
            }
        };
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
