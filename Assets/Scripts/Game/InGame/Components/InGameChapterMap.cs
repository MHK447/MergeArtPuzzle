using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using System.Linq;

public class InGameChapterMap : MonoBehaviour
{

    [SerializeField]
    private List<Transform> CreateTrList = new List<Transform>();
    public List<InGameFood> FoodList = new List<InGameFood>();


    public List<InGameFood> GetFoodList { get { return FoodList; } }

    private FoodMergeGroupData MergeGroupData;

    [SerializeField]
    private Transform FoodParent;
    private int FoodCreateOrder = 0;

    public void Init()
    {
        ProjectUtility.SetActiveCheck(this.gameObject, true);
        FoodCreateOrder = 0;

        var stageidx = GameRoot.Instance.UserData.CurMode.StageData.Stageidx.Value;

        StartFoodCreation();
    }
    public void StartFoodCreation()
    {
        StartCoroutine(CreateFoodsCoroutine());
    }

    private IEnumerator CreateFoodsCoroutine()
    {
        var stageidx = GameRoot.Instance.UserData.CurMode.StageData.Stageidx.Value;

        var foodmergelist = Tables.Instance.GetTable<FoodMergeGroupInfo>().DataList.FindAll(x => x.stageidx == stageidx).ToList();

        foreach (var foodmerge in foodmergelist)
        {
            MergeGroupData = GameRoot.Instance.FoodSystem.FindFoodMergeGroupData(foodmerge.mergeidx);

            if (MergeGroupData != null && MergeGroupData.Ingamefooddatas != null && MergeGroupData.Ingamefooddatas.Count > 0)
            {
                List<InGameFoodData> foodDataCopy = new List<InGameFoodData>(MergeGroupData.Ingamefooddatas);
                foreach (var fooddata in foodDataCopy)
                {
                    CreateFood(fooddata.Foodidx, fooddata.Mergegrade, MergeGroupData.Foodmergeidx, true);
                    yield return new WaitForSeconds(0.1f); // 0.05초 대기
                }
            }
        }
    }


    public void CreateFood(int foodidx, int grade, int foodgroupidx, bool isinit = false)
    {
        FoodCreateOrder++;

        if (FoodCreateOrder >= CreateTrList.Count)
        {
            FoodCreateOrder = 0;
        }

        int currentCreateOrder = FoodCreateOrder;

        var findfood = FoodList.Find(x => x.gameObject.activeSelf == false);

        if (findfood == null)
        {
            Addressables.InstantiateAsync("InGameFood").Completed += (handle) =>
            {
                var ingamefood = handle.Result.GetComponent<InGameFood>();
                ingamefood.Set(foodidx, grade, foodgroupidx);
                FoodList.Add(ingamefood);
                ingamefood.transform.position = CreateTrList[currentCreateOrder].position;
                ProjectUtility.SetActiveCheck(ingamefood.gameObject, true);
                ingamefood.transform.SetParent(FoodParent);
            };
        }
        else
        {
            findfood.Set(foodidx, grade, foodgroupidx);
            ProjectUtility.SetActiveCheck(findfood.gameObject, true);
            findfood.transform.SetParent(FoodParent);
            findfood.transform.position = CreateTrList[currentCreateOrder].position;
        }
    }

    public void EndGame()
    {
        foreach (var food in FoodList)
        {
            food.gameObject.SetActive(false);
            Destroy(food.gameObject);
        }

        FoodList.Clear();
    }

    public void RemoveFood(InGameFood food)
    {
        if (food != null)
        {
            FoodList.Remove(food);
        }
    }
}
