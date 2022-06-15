using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameStateController : MonoBehaviour
{
    [Header("Referencias Titulo de barra")]
    public Image playerXIcon;
    public Image playerOIcon;    

    [Header("Referencias Assets")]
    public Sprite tilePlayerO;
    public Sprite tilePlayerX;
    public Sprite tileEmpty;
    public Text[] tileList;

    [Header("Configuracion del juego")]
    public Color inactivePlayerColor;
    public Color activePlayerColor;
    public string whoPlaysFirst;

    [Header("Variables Privadas")] 
    private string playerTurn;
    private int moveCount;

    [Header("Paneles")]
    [SerializeField] GameObject PanelPausa;
    [SerializeField] GameObject PanelVictoria1;
    [SerializeField] GameObject PanelVictoria2;
    [SerializeField] GameObject PanelEmpate;

    private void Start() {
        //Establece un rastreador del primer turno del jugador y establece el icono de la interfaz de usuario para saber de quién es el turno
        playerTurn = whoPlaysFirst;
        if (playerTurn == "X") playerOIcon.color = inactivePlayerColor;
        else playerXIcon.color = inactivePlayerColor;
    }

    // Llama al final de cada turno para ver la condición de victoria
    public void EndTurn() {
        moveCount++;
        if (tileList[0].text == playerTurn && tileList[1].text == playerTurn && tileList[2].text == playerTurn) GameOver(playerTurn);
        else if (tileList[3].text == playerTurn && tileList[4].text == playerTurn && tileList[5].text == playerTurn) GameOver(playerTurn);
        else if (tileList[6].text == playerTurn && tileList[7].text == playerTurn && tileList[8].text == playerTurn) GameOver(playerTurn);
        else if (tileList[0].text == playerTurn && tileList[3].text == playerTurn && tileList[6].text == playerTurn) GameOver(playerTurn);
        else if (tileList[1].text == playerTurn && tileList[4].text == playerTurn && tileList[7].text == playerTurn) GameOver(playerTurn);
        else if (tileList[2].text == playerTurn && tileList[5].text == playerTurn && tileList[8].text == playerTurn) GameOver(playerTurn);
        else if (tileList[0].text == playerTurn && tileList[4].text == playerTurn && tileList[8].text == playerTurn) GameOver(playerTurn);
        else if (tileList[2].text == playerTurn && tileList[4].text == playerTurn && tileList[6].text == playerTurn) GameOver(playerTurn);
        else if (moveCount >= 9) GameOver("D");
        else
            ChangeTurn();
    }

    public void ChangeTurn() {
        playerTurn = (playerTurn == "X") ? "O" : "X";
        if (playerTurn == "X") {
            playerXIcon.color = activePlayerColor;
            playerOIcon.color = inactivePlayerColor;
        } else {
            playerXIcon.color = inactivePlayerColor;
            playerOIcon.color = activePlayerColor;
        }
    }

    // Se llama a la función cuando un jugador gana o cuando hay empate
    private void GameOver(string winningPlayer) {
        switch (winningPlayer) {
            case "D": PanelEmpate.SetActive(true); break;
            case "X": PanelVictoria1.SetActive(true); break;
            case "O": PanelVictoria2.SetActive(true); break;
        }        
        ToggleButtonState(false);
    }

 
    private void ToggleButtonState(bool state) {
        for (int i = 0; i < tileList.Length; i++)        
            tileList[i].GetComponentInParent<Button>().interactable = state;        
    }

    public string GetPlayersTurn() {
        return playerTurn;
    }

    public Sprite GetPlayerSprite() {
        if (playerTurn == "X") return tilePlayerX;
        else return tilePlayerO;
    }
    

    public void MostrarMenuPausa(bool estado){
        PanelPausa.SetActive(estado);
    }

    public void ReiniciarNivel(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void IrAMenuPrincipal(){
        SceneManager.LoadScene("MainMenu");
    }

    public void Exit(){
        Application.Quit();
    }
}
