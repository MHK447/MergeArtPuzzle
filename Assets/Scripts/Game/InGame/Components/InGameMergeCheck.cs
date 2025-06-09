using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BanpoFri;

public class InGameMergeCheck : MonoBehaviour
{
    [SerializeField]
    private InGameFood CurSelectFood;

    private InGameFood SelectFood;

    private Camera mainCamera;
    private bool isDragging = false;

    private Collider2D col;
    private int fingerId = -1;

    private InGameEnergyAd SelectEnergyAd;

    void Start()
    {
        mainCamera = Camera.main;
        col = GetComponent<Collider2D>();

        if (CurSelectFood != null)
        {
            CurSelectFood.gameObject.SetActive(false);
        }
    }

    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseInput();
#elif UNITY_ANDROID || UNITY_IOS
        HandleTouchInput();
#endif
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = GetMouseWorldPosition();
            Collider2D hit = Physics2D.OverlapPoint(worldPos);
            if (hit != null && hit.CompareTag("Food"))
            {
                isDragging = true;

                if (CurSelectFood != null)
                {
                    SelectFood = hit.GetComponent<InGameFood>();
                    CurSelectFood.gameObject.SetActive(true); 
                    CurSelectFood.transform.position = worldPos;
                    SelectFood.SelectOn();
                    CurSelectFood.SetSprite(SelectFood.GetMergeGroupIdx, SelectFood.GetFoodIdx, SelectFood.GetGrade);
                }
            }
            else if (hit != null && hit.CompareTag("EnergyAd"))
            {

                if (SelectEnergyAd != null)
                {
                    SelectEnergyAd.SelectActiveCheck(false);
                }

                SelectEnergyAd = hit.GetComponent<InGameEnergyAd>();

                var getui = GameRoot.Instance.UISystem.GetUI<PopupCollectionInfo>();

                if (SelectEnergyAd != null)
                {
                    SelectEnergyAd.SelectActiveCheck(true);
                    GameRoot.Instance.UISystem.OpenUI<PopupCollectionInfo>(popup => popup.Set(SelectEnergyAd.GetEnergyIdx, SelectEnergyAd));
                }
            }
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            Vector3 pos = GetMouseWorldPosition();
            if (CurSelectFood != null)
            {
                CurSelectFood.transform.position = pos;
            }
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;

            if (CurSelectFood != null)
            {
                SelectFood.SelectOff();
                CurSelectFood.gameObject.SetActive(false);
            }

            CheckMerge();
        }
    }

    void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                Vector3 worldPos = GetTouchWorldPosition(touch.position);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        if (!IsPointerOverUIObject(touch.position))
                        {
                            Collider2D hit = Physics2D.OverlapPoint(worldPos);
                            if (hit != null && hit.CompareTag("Food"))
                            {
                                fingerId = touch.fingerId;
                                isDragging = true;

                                if (CurSelectFood != null)
                                {
                                    SelectFood = hit.GetComponent<InGameFood>();
                                    CurSelectFood.gameObject.SetActive(true);
                                    CurSelectFood.transform.position = worldPos;
                                    SelectFood.SelectOn();
                                    CurSelectFood.SetSprite(SelectFood.GetMergeGroupIdx, SelectFood.GetFoodIdx, SelectFood.GetGrade);
                                }
                            }
                            else if (hit != null && hit.CompareTag("EnergyAd"))
                            {
                                if (SelectEnergyAd != null)
                                {
                                    SelectEnergyAd.SelectActiveCheck(false);
                                }

                                SelectEnergyAd = hit.GetComponent<InGameEnergyAd>();

                                if (SelectEnergyAd != null)
                                {
                                    SelectEnergyAd.SelectActiveCheck(true);
                                    GameRoot.Instance.UISystem.OpenUI<PopupCollectionInfo>(popup => popup.Set(SelectEnergyAd.GetEnergyIdx, SelectEnergyAd));
                                    
                                    Debug.Log("Energy Ad touched: " + SelectEnergyAd.GetEnergyIdx);
                                }
                            }
                        }
                        break;

                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        if (isDragging && touch.fingerId == fingerId)
                        {
                            if (CurSelectFood != null)
                            {
                                CurSelectFood.transform.position = worldPos;
                            }
                        }
                        break;

                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        if (isDragging && touch.fingerId == fingerId)
                        {
                            isDragging = false;
                            fingerId = -1;

                            if (CurSelectFood != null && SelectFood != null)
                            {
                                SelectFood.SelectOff();
                                CurSelectFood.gameObject.SetActive(false);
                            }

                            CheckMerge();
                        }
                        break;
                }
            }
        }
    }

    private bool IsPointerOverUIObject(Vector2 touchPos)
    {
        if (UnityEngine.EventSystems.EventSystem.current == null)
            return false;
            
        UnityEngine.EventSystems.PointerEventData eventDataCurrentPosition = 
            new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
        
        eventDataCurrentPosition.position = touchPos;
        
        List<UnityEngine.EventSystems.RaycastResult> results = new List<UnityEngine.EventSystems.RaycastResult>();
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        
        // 결과 필터링 - 특정 UI 요소는 무시하고 게임 플레이에 영향을 주는 UI만 체크
        if (results.Count > 0)
        {
            // 무시할 UI 요소 목록 (예: 배경 이미지, 효과 등)
            string[] ignoreUITags = new string[] { "Background", "Effect" };
            
            foreach (var result in results)
            {
                // 무시할 UI가 아니라면 UI 위에 있다고 판단
                if (result.gameObject != null && !System.Array.Exists(ignoreUITags, tag => result.gameObject.CompareTag(tag)))
                {
                    // UI 요소 이름 로그로 출력하여 디버깅
                    Debug.Log("UI Blocking Touch: " + result.gameObject.name);
                    return true;
                }
            }
        }
        
        // 모든 UI 요소가 무시 가능하거나 UI가 없음
        return false;
    }

    void CheckMerge()
    {
        Vector3 checkPos = CurSelectFood != null ? CurSelectFood.transform.position : transform.position;
        float mergeRadius = 0.55f;

        Collider2D[] hits = Physics2D.OverlapCircleAll(checkPos, mergeRadius);

        InGameFood closestFood = null;
        float closestDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            if (hit.gameObject != SelectFood.gameObject && hit.CompareTag("Food"))
            {
                var foodObj = hit.GetComponent<InGameFood>();

                if (SelectFood != null && SelectFood.GetFoodIdx == foodObj.GetFoodIdx && SelectFood.GetGrade == foodObj.GetGrade)
                {
                    float distance = Vector2.Distance(checkPos, hit.transform.position);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestFood = foodObj;
                    }
                }
            }
        }

        if (closestFood != null)
        {
            TryMergeWith(closestFood);
        }
    }
    void TryMergeWith(InGameFood other)
    {
        if (SelectFood != null)
        {
            if (SelectFood.GetFoodIdx == other.GetFoodIdx && SelectFood.GetGrade == other.GetGrade)
            {
                var mergegroupdata = GameRoot.Instance.FoodSystem.FindFoodMergeGroupData(SelectFood.GetMergeGroupIdx);

                if (mergegroupdata != null)
                {
                    var finddata = mergegroupdata.Ingamefooddatas.Find(x => x.Foodidx == SelectFood.GetFoodIdx && x.Mergegrade == SelectFood.GetGrade);

                    if (finddata != null)
                    {
                        mergegroupdata.Ingamefooddatas.Remove(finddata);
                    }
                }


                ProjectUtility.SetActiveCheck(SelectFood.gameObject, false);
                other.SetGrade(SelectFood.GetGrade + 1);

                // AddMergeEffect(other.transform.position);
            }
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 screenPos = Input.mousePosition;
        screenPos.z = 10; // 카메라 거리 조정 필요 시 변경
        return mainCamera.ScreenToWorldPoint(screenPos);
    }

    Vector3 GetTouchWorldPosition(Vector3 screenPos)
    {
        screenPos.z = 10; // 카메라 거리 조정 필요 시 변경
        return mainCamera.ScreenToWorldPoint(screenPos);
    }
}
