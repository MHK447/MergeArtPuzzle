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

                foodmergegroupdatas_Array[index++] = BanpoFri.Data.FoodMergeGroupData.CreateFoodMergeGroupData(
                    builder,
                    item.Foodmergeidx,
                    item_ingamefooddatas_Vector,
                    item.Foodcount.Value
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
                    Foodcount = new ReactiveProperty<int>(Foodmergegroupdatas_item.Value.Foodcount)
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
                Foodmergegroupdatas.Add(foodmergegroupdata);
            }
        }
    }

}

public class FoodMergeGroupData
{
    public IReactiveProperty<int> Foodcount { get; set; } = new ReactiveProperty<int>(0);

    public int Foodmergeidx { get; set; } = 0;
    public List<InGameFoodData> Ingamefooddatas = new List<InGameFoodData>();

}
