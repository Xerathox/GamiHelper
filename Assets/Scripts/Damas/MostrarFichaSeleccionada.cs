using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MostrarFichaSeleccionada : MonoBehaviour
{
    public CheckersBoard checkersBoard;
    public TextMeshProUGUI text;

    private void Awake() {        
        text = GetComponent<TextMeshProUGUI>();        
    }

    void Update() {
        if (checkersBoard.selectedPiece == null)        
            text.text = "Ninguna ficha seleccionada";        
        else        
            text.text = "Ficha " + checkersBoard.origen_columna + ' ' + checkersBoard.origen_fila + " seleccionada";
    }
}
