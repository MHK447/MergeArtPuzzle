using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;

public class InGameChapterMap : MonoBehaviour
{

    [SerializeField]
    private List<Transform> CreateTrList = new List<Transform>();
    public List<InGameFood> FoodList = new List<InGameFood>();


    [SerializeField]
    private Transform FoodParent;
    private int FoodCreateOrder = 0;

    public void Init()
    {
        ProjectUtility.SetActiveCheck(this.gameObject, true);
        FoodCreateOrder = 0;
    }



    public void CreateFood(int foodidx)
    {

        FoodCreateOrder++;

        if (FoodCreateOrder >= CreateTrList.Count)
        {
            FoodCreateOrder = 0;
        }

        var findfood = FoodList.Find(x => x.gameObject.activeSelf == false);

        if (findfood == null)
        {
            Addressables.InstantiateAsync("InGameFood").Completed += (handle) =>
         {
             var ingamefood = handle.Result.GetComponent<InGameFood>();
             ingamefood.Set(foodidx);
             FoodList.Add(ingamefood);
             ingamefood.transform.position = CreateTrList[FoodCreateOrder].position;
             ProjectUtility.SetActiveCheck(ingamefood.gameObject, true);
             ingamefood.transform.SetParent(FoodParent);
         };

        }
        else
        {
            findfood.Set(foodidx);
            ProjectUtility.SetActiveCheck(findfood.gameObject, true);
            findfood.transform.SetParent(FoodParent);

            findfood.transform.position = CreateTrList[FoodCreateOrder].position;
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
