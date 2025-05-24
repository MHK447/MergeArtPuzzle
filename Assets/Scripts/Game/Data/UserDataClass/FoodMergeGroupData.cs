using System;
using System.Collections.Generic;
using UniRx;
using Google.FlatBuffers;

public partial class UserDataSystem
{
    public List<FoodMergeGroupData> Foodmergegroupdatas { get; private set; } = new List<FoodMergeGroupData>();
    private void SaveData_FoodMergeGroupData(FlatBufferBuilder builder)
    {
        // 선언된 변수들은 모두 저장되어야함

        // Foodmergegroupdatas Array 저장
        Offset<BanpoFri.Data.FoodMergeGroupData>[] foodmergegroupdatas_Array = null;
        VectorOffset foodmergegroupdatas_Vector = default;

        if(Foodmergegroupdatas.Count > 0){
            foodmergegroupdatas_Array = new Offset<BanpoFri.Data.FoodMergeGroupData>[Foodmergegroupdatas.Count];
            int index = 0;
            foreach(var pair in Foodmergegroupdatas){
                var item = pair;
                // item.Ingamefooddatas 처리 GenerateItemSaveCode IsCustom
                Offset<BanpoFri.Data.InGameFoodData>[] item_ingamefooddatas_Array = null;
                VectorOffset item_ingamefooddatas_Vector = default;

                if(item.Ingamefooddatas.Count > 0){
                    item_ingamefooddatas_Array = new Offset<BanpoFri.Data.InGameFoodData>[item.Ingamefooddatas.Count];
                    int item_ingamefooddatas_idx = 0;
                    foreach(var item_ingamefooddatas_pair in item.Ingamefooddatas){
                        var item_ingamefooddatas_item = item.Ingamefooddatas[item_ingamefooddatas_idx];
                        item_ingamefooddatas_Array[item_ingamefooddatas_idx++] = BanpoFri.Data.InGameFoodData.CreateInGameFoodData(
                            builder,
                            item_ingamefooddatas_item.Foodidx,
                            item_ingamefooddatas_item.Mergegrade
                        );
                    }
                    item_ingamefooddatas_Vector = BanpoFri.Data.FoodMergeGroupData.CreateIngamefooddatasVector(builder, item_ingamefooddatas_Array);
                }

                // item.Drawfooddatas 처리 GenerateItemSaveCode IsCustom
                Offset<BanpoFri.Data.DrawFoodData>[] item_drawfooddatas_Array = null;
                VectorOffset item_drawfooddatas_Vector = default;

                if(item.Drawfooddatas.Count > 0){
                    item_drawfooddatas_Array = new Offset<BanpoFri.Data.DrawFoodData>[item.Drawfooddatas.Count];
                    int item_drawfooddatas_idx = 0;
                    foreach(var item_drawfooddatas_pair in item.Drawfooddatas){
                        var item_drawfooddatas_item = item.Drawfooddatas[item_drawfooddatas_idx];
                        item_drawfooddatas_Array[item_drawfooddatas_idx++] = BanpoFri.Data.DrawFoodData.CreateDrawFoodData(
                            builder,
                            item_drawfooddatas_item.Foodidx,
                            item_drawfooddatas_item.Drawfoodcount
                        );
                    }
                    item_drawfooddatas_Vector = BanpoFri.Data.FoodMergeGroupData.CreateDrawfooddatasVector(builder, item_drawfooddatas_Array);
                }

                foodmergegroupdatas_Array[index++] = BanpoFri.Data.FoodMergeGroupData.CreateFoodMergeGroupData(
                    builder,
                    item.Foodmergeidx,
                    item_ingamefooddatas_Vector,
                    item.Foodcount.Value,
                    item.Stageclearstarcount.Value,
                    item_drawfooddatas_Vector
                );
            }
            foodmergegroupdatas_Vector = BanpoFri.Data.UserData.CreateFoodmergegroupdatasVector(builder, foodmergegroupdatas_Array);
        }



        Action cbAddDatas = () => {
            BanpoFri.Data.UserData.AddFoodmergegroupdatas(builder, foodmergegroupdatas_Vector);
        };

        cb_SaveAddDatas += cbAddDatas;

    }
    private void LoadData_FoodMergeGroupData()
    {
        // 로드 함수 내용

        // Foodmergegroupdatas 로드
        Foodmergegroupdatas.Clear();
        int Foodmergegroupdatas_length = flatBufferUserData.FoodmergegroupdatasLength;
        for (int i = 0; i < Foodmergegroupdatas_length; i++)
        {
            var Foodmergegroupdatas_item = flatBufferUserData.Foodmergegroupdatas(i);
            if (Foodmergegroupdatas_item.HasValue)
            {
                var foodmergegroupdata = new FoodMergeGroupData
                {
                    Foodmergeidx = Foodmergegroupdatas_item.Value.Foodmergeidx,
                    Foodcount = new ReactiveProperty<int>(Foodmergegroupdatas_item.Value.Foodcount),
                    Stageclearstarcount = new ReactiveProperty<int>(Foodmergegroupdatas_item.Value.Stageclearstarcount),
                };

                // Ingamefooddatas 로드
                foodmergegroupdata.Ingamefooddatas.Clear();
                int ingamefooddatasLength = Foodmergegroupdatas_item.Value.IngamefooddatasLength;
                for (int j = 0; j < ingamefooddatasLength; j++)
                {
                    var fbIngamefooddatasItem = Foodmergegroupdatas_item.Value.Ingamefooddatas(j);
                    if (fbIngamefooddatasItem.HasValue)
                    {
                        var nested_item = new InGameFoodData
                        {
                            Foodidx = fbIngamefooddatasItem.Value.Foodidx,
                            Mergegrade = fbIngamefooddatasItem.Value.Mergegrade
                        };
                        foodmergegroupdata.Ingamefooddatas.Add(nested_item);
                    }
                }

                // Drawfooddatas 로드
                foodmergegroupdata.Drawfooddatas.Clear();
                int drawfooddatasLength = Foodmergegroupdatas_item.Value.DrawfooddatasLength;
                for (int j = 0; j < drawfooddatasLength; j++)
                {
                    var fbDrawfooddatasItem = Foodmergegroupdatas_item.Value.Drawfooddatas(j);
                    if (fbDrawfooddatasItem.HasValue)
                    {
                        var nested_item = new DrawFoodData
                        {
                            Foodidx = fbDrawfooddatasItem.Value.Foodidx,
                            Drawfoodcount = fbDrawfooddatasItem.Value.Drawfoodcount
                        };
                        foodmergegroupdata.Drawfooddatas.Add(nested_item);
                    }
                }
                Foodmergegroupdatas.Add(foodmergegroupdata);
            }
        }
    }

}

public class FoodMergeGroupData
{
    public List<DrawFoodData> Drawfooddatas = new List<DrawFoodData>();

    public IReactiveProperty<int> Stageclearstarcount { get; set; } = new ReactiveProperty<int>(0);

    public IReactiveProperty<int> Foodcount { get; set; } = new ReactiveProperty<int>(0);

    public int Foodmergeidx { get; set; } = 0;
    public List<InGameFoodData> Ingamefooddatas = new List<InGameFoodData>();

}
