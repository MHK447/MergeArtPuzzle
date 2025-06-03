using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using TMPro;
public class InGameEnergyAd : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer EnergyImg;

    [SerializeField]
    private GameObject SelectObj;

    [SerializeField]
    private TextMeshProUGUI TimeText;

    private int EnergyIdx = 0;

    public int GetEnergyIdx { get { return EnergyIdx; } }

    public float StartTime = 20;

    private float deltime = 0f;

    public void Set(int energyidx)
    {
        EnergyIdx = energyidx;

        var td = Tables.Instance.GetTable<InGameEnergyInfo>().GetData(energyidx);
        if (td != null)
        {
            EnergyImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, td.image);
        }

        ProjectUtility.SetActiveCheck(SelectObj, false);

        StartTime = 20f;
    }


    void Update()
    {
        deltime += Time.deltaTime;

        if (deltime >= 1f)
        {
            deltime = 0f;
            StartTime -= 1f;

            if (StartTime <= 0f)
            {
                StartTime = 0f;
                ProjectUtility.SetActiveCheck(gameObject, false);
            }
        }

    }


    public void SelectActiveCheck(bool isactive)
    {
        ProjectUtility.SetActiveCheck(SelectObj, isactive);
    }

}
