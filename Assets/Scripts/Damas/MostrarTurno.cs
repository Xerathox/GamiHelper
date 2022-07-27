using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MostrarTurno : MonoBehaviour
{
    public CheckersBoard checkersBoard;
    public TextMeshProUGUI text;
    // Start is called before the first frame update
    private void Awake() {        
        text = GetComponent<TextMeshProUGUI>();        
    }

    // Update is called once per frame
    void Update()
    {        
        switch (checkersBoard.isWhite)
        {
            case true:
                text.text = "Turno blancos";
                break;
            case false:
                text.text = "Turno negros";
                break;
        }
        
    }
}
