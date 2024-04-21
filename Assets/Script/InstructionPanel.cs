using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class InstructionPanel : MonoBehaviour
{
    
    [System.Serializable]
    public class Instruction
    {
        public GameObject instructionPanel;
        public string instructionText;
    }
    
    [SerializeField] private int _index;
    [SerializeField] private List<Instruction> instructionList;
    [SerializeField] private TMP_Text instructionText;

    public void OnEnable()
    {
        _index = 0;
        if (instructionList != null)
        {
            foreach (Instruction instruction in instructionList)
            {
                instruction.instructionPanel.SetActive(false);
            }
            
            instructionList[_index].instructionPanel.SetActive(true);
        }
    }

    public void ShowLeft()
    {
        if(instructionList == null) return;
        
        instructionList[GetMod(_index, instructionList.Count)].instructionPanel.SetActive(false);
        _index--;
        instructionList[GetMod(_index, instructionList.Count)].instructionPanel.SetActive(true);
        instructionText.text = instructionList[GetMod(_index, instructionList.Count)].instructionText;
    }

    public void ShowRight()
    {
        instructionList[GetMod(_index, instructionList.Count)].instructionPanel.SetActive(false);
        _index++;
        instructionList[GetMod(_index, instructionList.Count)].instructionPanel.SetActive(true);
        instructionText.text = instructionList[GetMod(_index, instructionList.Count)].instructionText;
    }

    private int GetMod(int n, int m)
    {
        return (n%m + m)%m;
    }
}
