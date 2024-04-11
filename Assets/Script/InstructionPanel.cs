using System;
using System.Collections.Generic;
using UnityEngine;

public class InstructionPanel : MonoBehaviour
{
    [SerializeField] private int _index;
    [SerializeField] private List<GameObject> instructionList;

    public void OnEnable()
    {
        _index = 0;
        if (instructionList != null)
        {
            foreach (GameObject obj in instructionList)
            {
                obj.SetActive(false);
            }
            
            instructionList[_index].SetActive(true);
        }
    }

    public void ShowLeft()
    {
        if(instructionList == null) return;
        
        instructionList[GetMod(_index, instructionList.Count)].SetActive(false);
        _index--;
        instructionList[GetMod(_index, instructionList.Count)].SetActive(true);
    }

    public void ShowRight()
    {
        instructionList[GetMod(_index, instructionList.Count)].SetActive(false);
        _index++;
        instructionList[GetMod(_index, instructionList.Count)].SetActive(true);
    }

    public int GetMod(int n, int m)
    {
        return (n%m + m)%m;
    }
}
