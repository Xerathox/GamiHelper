using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MostrarTurno : MonoBehaviour
{
    public CheckersBoard checkersBoard;
    public TextMeshProUGUI text;
    
    private void Awake() {        
        text = GetComponent<TextMeshProUGUI>();        
    }
    
    void Update()
    {        
        switch (checkersBoard.isWhite) {
            case true:
                text.text = "Turno blancos";
                break;
            case false:
                text.text = "Turno negros";
                break;
        }        
    }
}
