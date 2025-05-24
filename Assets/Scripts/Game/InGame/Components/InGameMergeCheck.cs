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
                    CurSelectFood.SetSprite(SelectFood.GetMergeGroupIdx , SelectFood.GetFoodIdx, SelectFood.GetGrade);
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
                        Collider2D hit = Physics2D.OverlapPoint(worldPos);
                        if (hit != null && hit.CompareTag("Food"))
                        {
                            fingerId = touch.fingerId;
                            isDragging = true;

                            if (CurSelectFood != null)
                            {
                                CurSelectFood.gameObject.SetActive(true);
                                CurSelectFood.transform.position = worldPos;
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

                            if (CurSelectFood != null)
                            {
                                CurSelectFood.gameObject.SetActive(false);
                            }

                            CheckMerge();
                        }
                        break;
                }
            }
        }
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

                if(mergegroupdata != null)
                {
                    var finddata = mergegroupdata.Ingamefooddatas.Find(x => x.Foodidx == SelectFood.GetFoodIdx && x.Mergegrade == SelectFood.GetGrade);

                    if(finddata != null)
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
