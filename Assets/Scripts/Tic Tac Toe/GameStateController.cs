using TMPro;
using System;
using System.Linq;
using UnityEngine.Windows.Speech;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

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

    //Reconocimiento de voz
    public MiDiccionario[] columnas;
    public MiDiccionario[] filas;

    //Leer JSON
    public string textJSON;
    public string textJSONMENU;

    void Awake() {
        LlamadoApi();
    }

    void Empezar() {         
        //Leer JSON        
        Debug.Log(textJSON);
        JSONInitializer jsonInitializer = new JSONInitializer();    
        jsonInitializer = JsonUtility.FromJson<JSONInitializer>(textJSON);
        columnas = jsonInitializer.columna;
        filas = jsonInitializer.fila;

        JSONMenuInitializer jSONMenuInitializer = new JSONMenuInitializer();
        jSONMenuInitializer = JsonUtility.FromJson<JSONMenuInitializer>(textJSONMENU);
        
        //Establece un rastreador del primer turno del jugador y establece el icono de la interfaz de usuario para saber de quién es el turno
        playerTurn = whoPlaysFirst;
        if (playerTurn == "X") playerOIcon.color = inactivePlayerColor;
        else playerXIcon.color = inactivePlayerColor;

        //Reconocimiento de voz
        SpeechController.instance.actions.Add(jSONMenuInitializer.pausa, MostrarMenuPausa);
        SpeechController.instance.actions.Add(jSONMenuInitializer.reanudar, CerrarMenuPausa);
        SpeechController.instance.actions.Add(jSONMenuInitializer.reiniciar, ReiniciarNivel);
        SpeechController.instance.actions.Add(jSONMenuInitializer.cerrar, IrAMenuPrincipal);  

        //Comandos de voz               
        foreach (var columna in columnas) {
            foreach (var fila in filas) {
                SpeechController.instance.actions.Add(columna.key + ' ' +  fila.key , VozMarcarCasilla);
            }   
        }
        SpeechController.instance.IniciarSpeech();
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
        SceneManager.LoadScene(ScreenIndices.MAINMENU);
    }

    //Reconocimiento de voz
    private void RecognizedSpeech(PhraseRecognizedEventArgs speech) {
        SpeechController.instance.TextoDicho = speech.text;
        SpeechController.instance.actions[speech.text].Invoke();
    }

    public void VozMarcarCasilla() {
        string[] words = SpeechController.instance.TextoDicho.Split(' ');
        MiDiccionario columna = Array.Find(columnas, item => item.key == words[0]);
        MiDiccionario fila = Array.Find(filas, item => item.key == words[1]);
        int IdFicha = columna.value + fila.value;

        tileList[IdFicha].gameObject.GetComponentInParent<TileController>().UpdateTile();
        Debug.Log(SpeechController.instance.TextoDicho);
        
    }

    void LlamadoApi() {
        StartCoroutine(LlamadoApiCorrutina()); 
    }

    IEnumerator LlamadoApiCorrutina() {
        List<string> URLTICTACTOE = new List<string>();
        List<string> URLMENU = new List<string>();
        
        URLTICTACTOE.Add("https://raw.githubusercontent.com/Xerathox/JSONFiles/main/JSONTICTACTOE.json");
        URLTICTACTOE.Add("https://raw.githubusercontent.com/Xerathox/JSONFiles2/main/JSONTICTACTOE.json");
        URLMENU.Add("https://raw.githubusercontent.com/Xerathox/JSONFiles/main/JSONMENUS.json");
        URLMENU.Add("https://raw.githubusercontent.com/Xerathox/JSONFiles2/main/JSONMENUS.json");

        foreach (string i in URLTICTACTOE) {            
            UnityWebRequest  webtictactoe = UnityWebRequest.Get(i);
            yield return webtictactoe.SendWebRequest();
            
            if(!webtictactoe.isNetworkError && !webtictactoe.isHttpError) {
                Debug.Log("CONEXION CON ÉXITO JSON MENU PRINCIPAL");
                textJSON = webtictactoe.downloadHandler.text;
                break;
            } else
                Debug.Log("hubo un problema con la web");
        }
          
        foreach (string i in URLMENU) {

            UnityWebRequest webmenuprincipal = UnityWebRequest.Get(i);            
            yield return webmenuprincipal.SendWebRequest();            
           
            if(!webmenuprincipal.isNetworkError && !webmenuprincipal.isHttpError) {
                Debug.Log("CONEXION CON ÉXITO JSON MENU PRINCIPAL");
                textJSONMENU = webmenuprincipal.downloadHandler.text;            
                break;
            } else
                Debug.Log("hubo un problema con la web");
        }

        Empezar();        
    }
}    


