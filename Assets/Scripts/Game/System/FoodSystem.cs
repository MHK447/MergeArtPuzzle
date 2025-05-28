using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using System.Linq;
using UniRx;
public class FoodSystem
{
    public IReactiveProperty<int> EnergyCoinTimeProperty = new ReactiveProperty<int>(0);

    private float deltatime = 0f;

    public int energy_add_time = 0;

    public int start_energy_coin = 0;

    public int FoodMaxPool = 4;

    public int max_food_size = 0;

    public void Create()
    {
        EnergyCoinTimeProperty.Value = 0;

        energy_add_time = Tables.Instance.GetTable<Define>().GetData("energy_add_time").value;
        start_energy_coin = Tables.Instance.GetTable<Define>().GetData("start_energy_coin").value;
        max_food_size = Tables.Instance.GetTable<Define>().GetData("max_food_size").value;
    }


    public FoodMergeGroupData FindFoodMergeGroupData(int foodgroupidx)
    {
        var stageidx = GameRoot.Instance.UserData.Stagedata.Stageidx.Value;

        var finddata = GameRoot.Instance.UserData.Foodmergegroupdatas.Find(x => x.Foodmergeidx == foodgroupidx);

        if (finddata != null)
        {
            return finddata;
        }
        else
        {
            var newdata = new FoodMergeGroupData
            {
                Foodmergeidx = foodgroupidx,
                Foodcount = new ReactiveProperty<int>(0),
                Ingamefooddatas = new List<InGameFoodData>()
            };
            GameRoot.Instance.UserData.Foodmergegroupdatas.Add(newdata);
            return newdata;
        }
    }


    public void CurCheckFoodData()
    {
        try
        {
            // InGameTycoon이나 InGameChapterMap이 없으면 데이터 저장만 하고 종료
            var inGameTycoon = GameRoot.Instance.InGameSystem.GetInGame<InGameTycoon>();
            if (inGameTycoon == null || inGameTycoon.InGameChapterMap == null)
            {
                Debug.Log("CurCheckFoodData: 인게임 참조가 없어 기존 데이터 유지");
                GameRoot.Instance.UserData.Save();
                return;
            }

            var foodlist = inGameTycoon.InGameChapterMap.GetFoodList.FindAll(x => x.gameObject.activeSelf).ToList();
            
            // 현재 상태를 데이터에 반영
            if (foodlist.Count > 0)
            {
                // 기존 데이터 초기화
                foreach (var mergegroupdata in GameRoot.Instance.UserData.Foodmergegroupdatas)
                {
                    mergegroupdata.Ingamefooddatas.Clear();
                }

                // 현재 활성화된 음식 데이터 추가
                foreach (var food in foodlist)
                {
                    var mergedata = FindFoodMergeGroupData(food.GetMergeGroupIdx);

                    if (mergedata != null)
                    {
                        mergedata.Ingamefooddatas.Add(new InGameFoodData()
                        {
                            Foodidx = food.GetFoodIdx,
                            Mergegrade = food.GetGrade
                        });
                    }
                }
                
                Debug.Log($"CurCheckFoodData: {foodlist.Count}개 음식 데이터 저장");
            }
            else
            {
                Debug.Log("CurCheckFoodData: 활성화된 음식이 없어 기존 데이터 유지");
            }

            // 데이터 저장
            GameRoot.Instance.UserData.Save(true); // 즉시 저장하도록 변경
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CurCheckFoodData 오류: {e.Message}");
        }
    }


    public void OneSecondUpdate()
    {
        EnergyCoinTimeProperty.Value += 1;

        if (EnergyCoinTimeProperty.Value >= energy_add_time)
        {
            GameRoot.Instance.UserData.Energycoin.Value += 1;
            EnergyCoinTimeProperty.Value = 0;
        }
    }

    // 게임 종료 시 데이터 저장
    public void SaveOnGameExit()
    {
        // 현재 활성화된 음식 데이터 동기화
        CurCheckFoodData();
    }
}
