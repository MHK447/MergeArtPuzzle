using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using BanpoFri;
using UnityEngine;

public class InGameFood : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer FoodImg;

    [SerializeField]
    private PolygonCollider2D Col;

    private int FoodIdx = 0;

    private int Grade = 0;

    public int GetFoodIdx { get { return FoodIdx; } }

    public int GetGrade { get { return Grade; } }

    private int MergeGroupIdx = 0;

    public void Set(int foodidx, int grade, int groupidx)
    {
        FoodIdx = foodidx;

        Grade = grade;

        MergeGroupIdx = groupidx;

        var td = Tables.Instance.GetTable<FoodInfo>().GetData(foodidx);

        SetSprite(foodidx, grade);

        UpdatePolygonCollider_OnlyOuter();

        Grade = 1;
    }


    private void UpdatePolygonCollider_OnlyOuter()
    {
        if (FoodImg == null || FoodImg.sprite == null)
        {
            Debug.LogWarning("FoodImg 또는 sprite가 null입니다.");
            return;
        }

        if (Col == null)
        {
            Col = gameObject.AddComponent<PolygonCollider2D>();
        }

        List<Vector2> shape = new List<Vector2>();
        FoodImg.sprite.GetPhysicsShape(0, shape); // 외곽 경로만

        Col.pathCount = 1;
        Col.SetPath(0, shape.ToArray());

        Debug.Log("PolygonCollider2D가 스프라이트 외곽(첫 번째 경로)에 맞춰 갱신되었습니다.");
    }


    public void SelectOn()
    {
        UnityEngine.Color color = FoodImg.color;
        color.a = 0.5f;
        FoodImg.color = color;
    }

    public void SelectOff()
    {

        UnityEngine.Color color = FoodImg.color;
        color.a = 1f;
        FoodImg.color = color;
    }

    public void SetSprite(int foodidx, int grade)
    {
        FoodIdx = foodidx;
        Grade = grade;

        var sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_InGame_Food, $"Food_0{FoodIdx}_{Grade}_03");

        if (sprite != null)
        {
            FoodImg.sprite = sprite;
        }
    }

    public void SetGrade(int grade)
    {

        GameRoot.Instance.EffectSystem.MultiPlay<MergeEffect>(this.transform.position, effect =>
                               {
                                   effect.SetAutoRemove(true, 3.5f);
                               });

        Grade = grade;

        var sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_InGame_Food, $"Food_0{FoodIdx}_{Grade}_03");

        if (sprite != null)
        {
            FoodImg.sprite = sprite;
        }

        if (grade == 4)
        {
            ProjectUtility.SetActiveCheck(this.gameObject, false);

            var startPos = Utility.worldToUISpace(GameRoot.Instance.UISystem.WorldCanvas, this.transform.position);

            var pos = GameRoot.Instance.UISystem.GetUI<PopupInGameUI>().GetInGameFoodSlotComponent(MergeGroupIdx).transform.position;

            ProjectUtility.PlayGoodsEffect(startPos, (int)Config.RewardType.Food, FoodIdx, grade, 1, false, null, 0, "", null, false
            , false, pos);
        }
    }

}
