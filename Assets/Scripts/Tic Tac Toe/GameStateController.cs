using TMPro;
using System;
using System.Linq;
using UnityEngine.Windows.Speech;
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
    //Reconocimiento de voz
    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, Action> actions = new Dictionary<string, Action>();
    private string TextoDicho;

    [Header("Paneles")]
    [SerializeField] GameObject PanelPausa;
    [SerializeField] GameObject PanelVictoria1;
    [SerializeField] GameObject PanelVictoria2;
    [SerializeField] GameObject PanelEmpate;

    void Start() {
        //Establece un rastreador del primer turno del jugador y establece el icono de la interfaz de usuario para saber de quién es el turno
        playerTurn = whoPlaysFirst;
        if (playerTurn == "X") playerOIcon.color = inactivePlayerColor;
        else playerXIcon.color = inactivePlayerColor;

        //Reconocimiento de voz
        actions.Add("pausa", MostrarMenuPausa);
        actions.Add("reanudar", CerrarMenuPausa);
        actions.Add("reiniciar", ReiniciarNivel);
        actions.Add("cerrar", IrAMenuPrincipal);  

        //Comandos de voz       
        actions.Add("a uno", VozMarcarCasilla);
        actions.Add("b uno", VozMarcarCasilla);
        actions.Add("c uno", VozMarcarCasilla);
        actions.Add("a dos", VozMarcarCasilla);
        actions.Add("b dos", VozMarcarCasilla);
        actions.Add("c dos", VozMarcarCasilla);
        actions.Add("a tres", VozMarcarCasilla);
        actions.Add("b tres", VozMarcarCasilla);
        actions.Add("c tres", VozMarcarCasilla);

        keywordRecognizer = new KeywordRecognizer(actions.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += RecognizedSpeech;
        keywordRecognizer.Start();    
    }

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

    private void GameOver(string winningPlayer) {
        switch (winningPlayer) {
            case "D": PanelEmpate.SetActive(true); break;
            case "X": PanelVictoria1.SetActive(true); break;
            case "O": PanelVictoria2.SetActive(true); break;
        }        
        ToggleButtonState(false);
    }
    //editar a voz
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

    public void MostrarMenuPausa(){
        PanelPausa.SetActive(true);
    }
    public void CerrarMenuPausa(){
        PanelPausa.SetActive(false);
    }
    public void ReiniciarNivel(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void IrAMenuPrincipal(){
        SceneManager.LoadScene("MainMenu");
    }

    //Reconocimiento de voz
    private void RecognizedSpeech(PhraseRecognizedEventArgs speech)
    {
        //Debug.Log(speech.text);
        TextoDicho = speech.text;
        actions[speech.text].Invoke();
    }

    public void VozMarcarCasilla(){
        switch (TextoDicho)
        {
            case "a uno": 
                tileList[0].gameObject.GetComponentInParent<TileController>().UpdateTile();
                Debug.Log(TextoDicho);
                break;
            case "b uno":
                tileList[1].gameObject.GetComponentInParent<TileController>().UpdateTile();
                Debug.Log(TextoDicho);
                break;
            case "c uno":
                tileList[2].gameObject.GetComponentInParent<TileController>().UpdateTile();
                Debug.Log(TextoDicho);
                break;
            case "a dos":
                tileList[3].gameObject.GetComponentInParent<TileController>().UpdateTile();
                Debug.Log(TextoDicho);
                break;
            case "b dos":
                tileList[4].gameObject.GetComponentInParent<TileController>().UpdateTile();
                Debug.Log(TextoDicho);
                break;
            case "c dos":
                tileList[5].gameObject.GetComponentInParent<TileController>().UpdateTile();
                Debug.Log(TextoDicho);
                break;
            case "a tres":
                tileList[6].gameObject.GetComponentInParent<TileController>().UpdateTile();
                Debug.Log(TextoDicho);
                break;  
            case "b tres":
                tileList[7].gameObject.GetComponentInParent<TileController>().UpdateTile();
                Debug.Log(TextoDicho);
                break;
            case "c tres":
                tileList[8].gameObject.GetComponentInParent<TileController>().UpdateTile();
                Debug.Log(TextoDicho);
                break;  
            default:
                break;
        }        

    }
}
