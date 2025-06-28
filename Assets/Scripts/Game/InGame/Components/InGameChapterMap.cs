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

    public List<InGameEnergyAd> EnergyAdList = new List<InGameEnergyAd>();


    public List<InGameFood> GetFoodList { get { return FoodList; } }

    private FoodMergeGroupData MergeGroupData;

    [SerializeField]
    private Transform FoodParent;


    [SerializeField]
    private Transform LeftTr;

    [SerializeField]
    private Transform RightTr;

    private int FoodCreateOrder = 0;

    public void Init()
    {
        ProjectUtility.SetActiveCheck(this.gameObject, true);
        FoodCreateOrder = 0;

        // 기존 음식들 완전히 정리
        ClearAllFoods();

        var stageidx = GameRoot.Instance.UserData.Stagedata.Stageidx.Value;

        AdjustPositionsForScreenResolution();
        
        // 물리 엔진 안정화를 위한 약간의 지연 후 음식 생성 시작
        GameRoot.Instance.WaitTimeAndCallback(0.2f, () =>
        {
            StartFoodCreation();
        });
    }

    float screenLeftX = 0f; // 화면의 가장 왼쪽 X 위치

  private void AdjustPositionsForScreenResolution()
{
    if (LeftTr == null || RightTr == null)
        return;

    float screenY = Screen.height * 0.5f; // 중앙 높이 정도 (원하는 값으로 수정)
    float distanceFromCamera = 5f; // 카메라로부터의 거리

    // ----------------
    // 화면의 가장 왼쪽
    Vector3 screenPointLeft = new Vector3(0f, screenY, distanceFromCamera);
    Vector3 worldPosLeft = Camera.main.ScreenToWorldPoint(screenPointLeft);
    LeftTr.position = new Vector3(worldPosLeft.x, transform.position.y, transform.position.z);

    // ----------------
    // 화면의 가장 오른쪽
    Vector3 screenPointRight = new Vector3(Screen.width, screenY, distanceFromCamera);
    Vector3 worldPosRight = Camera.main.ScreenToWorldPoint(screenPointRight);
    RightTr.position = new Vector3(worldPosRight.x, transform.position.y, transform.position.z);
}

    void Update()
    {
        AdjustPositionsForScreenResolution();
    }

    public void StartFoodCreation()
    {
        StartCoroutine(CreateFoodsCoroutine());
    }

    private IEnumerator CreateFoodsCoroutine()
    {
        var stageidx = GameRoot.Instance.UserData.Stagedata.Stageidx.Value;

        var foodmergelist = Tables.Instance.GetTable<FoodMergeGroupInfo>().DataList.FindAll(x => x.stageidx == stageidx).ToList();

        // 게임 시작 시 더 분산된 배치를 위해 생성 순서를 랜덤화
        List<int> randomizedOrder = new List<int>();
        for (int i = 0; i < CreateTrList.Count; i++)
        {
            randomizedOrder.Add(i);
        }
        
        // 생성 순서 섞기
        for (int i = 0; i < randomizedOrder.Count; i++)
        {
            int temp = randomizedOrder[i];
            int randomIndex = Random.Range(i, randomizedOrder.Count);
            randomizedOrder[i] = randomizedOrder[randomIndex];
            randomizedOrder[randomIndex] = temp;
        }

        int currentFoodIndex = 0;

        foreach (var foodmerge in foodmergelist)
        {
            MergeGroupData = GameRoot.Instance.FoodSystem.FindFoodMergeGroupData(foodmerge.mergeidx);

            if (MergeGroupData != null && MergeGroupData.Ingamefooddatas != null && MergeGroupData.Ingamefooddatas.Count > 0)
            {
                List<InGameFoodData> foodDataCopy = new List<InGameFoodData>(MergeGroupData.Ingamefooddatas);
                foreach (var fooddata in foodDataCopy)
                {
                    // 더 분산된 위치에 생성하기 위한 개선된 로직
                    CreateFoodWithSafePosition(fooddata.Foodidx, fooddata.Mergegrade, MergeGroupData.Foodmergeidx, randomizedOrder, currentFoodIndex);
                    currentFoodIndex = (currentFoodIndex + 1) % randomizedOrder.Count;
                    
                    // 더 긴 간격으로 생성하여 안정성 확보
                    yield return new WaitForSeconds(0.3f);
                }
            }
        }
        
        // 모든 음식 생성 완료 후 물리 시뮬레이션 안정화 대기
        yield return new WaitForSeconds(1f);
    }

    // 안전한 위치에 음식을 생성하는 새로운 메서드
    private void CreateFoodWithSafePosition(int foodidx, int grade, int foodgroupidx, List<int> randomizedOrder, int currentIndex)
    {
        int createOrderIndex = randomizedOrder[currentIndex % randomizedOrder.Count];
        
        // 더 강력한 랜덤 오프셋 적용 (게임 시작 시)
        Vector3 basePosition = CreateTrList[createOrderIndex].position;
        Vector3 randomOffset = new Vector3(
            Random.Range(-1.2f, 1.2f), // 더 넓은 X축 분산
            Random.Range(-0.8f, 0.8f), // 더 넓은 Y축 분산
            0f
        );
        Vector3 spawnPosition = basePosition + randomOffset;

        var findfood = FoodList.Find(x => x.gameObject.activeSelf == false);

        if (findfood == null)
        {
            Addressables.InstantiateAsync("InGameFood").Completed += (handle) =>
            {
                var ingamefood = handle.Result.GetComponent<InGameFood>();
                ingamefood.Set(foodidx, grade, foodgroupidx);
                FoodList.Add(ingamefood);
                ingamefood.transform.position = spawnPosition;
                ProjectUtility.SetActiveCheck(ingamefood.gameObject, true);
                ingamefood.transform.SetParent(FoodParent);
                
                // 게임 시작 시에도 약간의 임펄스 적용 (겹침 방지)
                var rigidbody = ingamefood.GetComponent<Rigidbody2D>();
                if (rigidbody != null)
                {
                    // 약간 지연 후 임펄스 적용
                    GameRoot.Instance.WaitTimeAndCallback(0.1f, () =>
                    {
                        if (ingamefood != null && rigidbody != null)
                        {
                            rigidbody.AddForce(new Vector2(Random.Range(-1f, 1f), Random.Range(0.5f, 1.5f)), ForceMode2D.Impulse);
                        }
                    });
                }
            };
        }
        else
        {
            findfood.Set(foodidx, grade, foodgroupidx);
            ProjectUtility.SetActiveCheck(findfood.gameObject, true);
            findfood.transform.SetParent(FoodParent);
            findfood.transform.position = spawnPosition;
            
            // 게임 시작 시에도 임펄스 적용
            var rigidbody = findfood.GetComponent<Rigidbody2D>();
            if (rigidbody != null)
            {
                rigidbody.velocity = Vector2.zero;
                // 약간 지연 후 임펄스 적용
                GameRoot.Instance.WaitTimeAndCallback(0.1f, () =>
                {
                    if (findfood != null && rigidbody != null)
                    {
                        rigidbody.AddForce(new Vector2(Random.Range(-1f, 1f), Random.Range(0.5f, 1.5f)), ForceMode2D.Impulse);
                    }
                });
            }
        }
    }



    public bool IsFoodMaxCountCheck()
    {
        var foodlist = FoodList.FindAll(x => x.gameObject.activeSelf == true);

        return foodlist.Count >= GameRoot.Instance.FoodSystem.max_food_size;
    }


    public void CreateFood(int foodidx, int grade, int foodgroupidx, bool isinit = false)
    {
        FoodCreateOrder++;

        if (FoodCreateOrder >= CreateTrList.Count)
        {
            FoodCreateOrder = 0;
        }

        int currentCreateOrder = FoodCreateOrder;
        
        // 생성 위치에 랜덤 오프셋 추가 (리지드바디 겹침 방지)
        Vector3 basePosition = CreateTrList[currentCreateOrder].position;
        Vector3 randomOffset = new Vector3(
            Random.Range(-0.5f, 0.5f), // X축 랜덤 오프셋
            Random.Range(-0.3f, 0.3f), // Y축 랜덤 오프셋
            0f
        );
        Vector3 spawnPosition = basePosition + randomOffset;

        var findfood = FoodList.Find(x => x.gameObject.activeSelf == false);

        if (findfood == null)
        {
            Addressables.InstantiateAsync("InGameFood").Completed += (handle) =>
            {
                var ingamefood = handle.Result.GetComponent<InGameFood>();
                ingamefood.Set(foodidx, grade, foodgroupidx);
                FoodList.Add(ingamefood);
                ingamefood.transform.position = spawnPosition;
                ProjectUtility.SetActiveCheck(ingamefood.gameObject, true);
                ingamefood.transform.SetParent(FoodParent);
                
                // 생성 직후 약간의 임펄스 추가 (겹침 방지)
                var rigidbody = ingamefood.GetComponent<Rigidbody2D>();
                if (rigidbody != null && !isinit)
                {
                    rigidbody.AddForce(new Vector2(Random.Range(-2f, 2f), Random.Range(1f, 3f)), ForceMode2D.Impulse);
                }
            };
        }
        else
        {
            findfood.Set(foodidx, grade, foodgroupidx);
            ProjectUtility.SetActiveCheck(findfood.gameObject, true);
            findfood.transform.SetParent(FoodParent);
            findfood.transform.position = spawnPosition;
            
            // 재사용 시에도 약간의 임펄스 추가 (겹침 방지)
            var rigidbody = findfood.GetComponent<Rigidbody2D>();
            if (rigidbody != null && !isinit)
            {
                rigidbody.velocity = Vector2.zero; // 기존 속도 초기화
                rigidbody.AddForce(new Vector2(Random.Range(-2f, 2f), Random.Range(1f, 3f)), ForceMode2D.Impulse);
            }
        }
    }



    public void CreateEnergy(int energyidx)
    {
        FoodCreateOrder++;

        if (FoodCreateOrder >= CreateTrList.Count)
        {
            FoodCreateOrder = 0;
        }

        int currentCreateOrder = FoodCreateOrder;

        var findenergy = EnergyAdList.Find(x => x.gameObject.activeSelf == false);

        if (findenergy == null)
        {
            Addressables.InstantiateAsync("InGameEnergy").Completed += (handle) =>
            {
                var ingameenergy = handle.Result.GetComponent<InGameEnergyAd>();
                ingameenergy.Set(energyidx);
                EnergyAdList.Add(ingameenergy);
                ingameenergy.transform.position = CreateTrList[currentCreateOrder].position;
                ProjectUtility.SetActiveCheck(ingameenergy.gameObject, true);
                ingameenergy.transform.SetParent(FoodParent);
            };
        }
        else
        {
            findenergy.Set(energyidx);
            ProjectUtility.SetActiveCheck(findenergy.gameObject, true);
            findenergy.transform.SetParent(FoodParent);
            findenergy.transform.position = CreateTrList[currentCreateOrder].position;
        }
    }

    public void EndGame()
    {
        // 게임 종료 전 데이터 저장
        GameRoot.Instance.FoodSystem.CurCheckFoodData();
        
        // 모든 코루틴 정지
        StopAllCoroutines();
        
        // 안전한 정리 수행
        ClearAllFoods();
        
        // 추가 안전성을 위한 완전한 정리
        foreach (var food in FoodList)
        {
            if (food != null && food.gameObject != null)
            {
                food.gameObject.SetActive(false);
                Destroy(food.gameObject);
            }
        }

        foreach (var energy in EnergyAdList)
        {
            if (energy != null && energy.gameObject != null)
            {
                energy.gameObject.SetActive(false);
                Destroy(energy.gameObject);
            }
        }

        FoodList.Clear();
        EnergyAdList.Clear();
        
        // 생성 순서 초기화
        FoodCreateOrder = 0;
    }

    public void RemoveFood(InGameFood food)
    {
        if (food != null)
        {
            FoodList.Remove(food);
        }
    }

    // 모든 음식을 안전하게 정리하는 메서드
    private void ClearAllFoods()
    {
        // 활성화된 모든 음식들을 비활성화하고 리지드바디 초기화
        foreach (var food in FoodList)
        {
            if (food != null && food.gameObject != null)
            {
                // 리지드바디 상태 초기화
                var rigidbody = food.GetComponent<Rigidbody2D>();
                if (rigidbody != null)
                {
                    rigidbody.velocity = Vector2.zero;
                    rigidbody.angularVelocity = 0f;
                    rigidbody.rotation = 0f;
                }
                
                // 오브젝트 비활성화
                ProjectUtility.SetActiveCheck(food.gameObject, false);
            }
        }
        
        // 에너지 아이템들도 정리
        foreach (var energy in EnergyAdList)
        {
            if (energy != null && energy.gameObject != null)
            {
                ProjectUtility.SetActiveCheck(energy.gameObject, false);
            }
        }
    }

}
