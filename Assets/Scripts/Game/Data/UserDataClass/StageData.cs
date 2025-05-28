using System;
using System.Collections.Generic;
using UniRx;
using Google.FlatBuffers;

public partial class UserDataSystem
{
    public StageData Stagedata { get; private set; } = new StageData();
    private void SaveData_StageData(FlatBufferBuilder builder)
    {
        // 선언된 변수들은 모두 저장되어야함

        // Stagedata 단일 저장
        // Stagedata 최종 생성 및 추가
        var stagedata_Offset = BanpoFri.Data.StageData.CreateStageData(
            builder,
            Stagedata.Stageidx.Value
        );


        Action cbAddDatas = () => {
            BanpoFri.Data.UserData.AddStagedata(builder, stagedata_Offset);
        };

        cb_SaveAddDatas += cbAddDatas;

    }
    private void LoadData_StageData()
    {
        // 로드 함수 내용

        // Stagedata 로드
        var fb_Stagedata = flatBufferUserData.Stagedata;
        if (fb_Stagedata.HasValue)
        {
            Stagedata.Stageidx.Value = fb_Stagedata.Value.Stageidx;
            
            // mainData의 StageData와 동기화
            if (mainData != null && mainData.StageData != null)
            {
                mainData.StageData.Stageidx.Value = Stagedata.Stageidx.Value;
            }
        }
    }

    // StageData 값이 변경될 때 두 속성 간 동기화를 위한 메소드 추가
    public void UpdateStageIdx(int stageIdx)
    {
        Stagedata.Stageidx.Value = stageIdx;
        
        if (mainData != null && mainData.StageData != null)
        {
            mainData.StageData.Stageidx.Value = stageIdx;
        }
        
        Save();
    }
}

public class StageData
{
    public IReactiveProperty<int> Stageidx { get; set; } = new ReactiveProperty<int>(1);

}
