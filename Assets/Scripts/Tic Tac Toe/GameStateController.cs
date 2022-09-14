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
    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, Action> actions = new Dictionary<string, Action>();
    private string TextoDicho;
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
        actions.Add(jSONMenuInitializer.pausa, MostrarMenuPausa);
        actions.Add(jSONMenuInitializer.reanudar, CerrarMenuPausa);
        actions.Add(jSONMenuInitializer.reiniciar, ReiniciarNivel);
        actions.Add(jSONMenuInitializer.cerrar, IrAMenuPrincipal);  

        //Comandos de voz               
        foreach (var columna in columnas) {
            foreach (var fila in filas) {
                actions.Add(columna.key + ' ' +  fila.key , VozMarcarCasilla);
            }   
        }
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
        SceneManager.LoadScene(ScreenIndices.MAINMENU);
    }

    //Reconocimiento de voz
    private void RecognizedSpeech(PhraseRecognizedEventArgs speech)
    {
        //Debug.Log(speech.text);
        TextoDicho = speech.text;
        actions[speech.text].Invoke();
    }

    public void VozMarcarCasilla() {
        string[] words = TextoDicho.Split(' ');
        MiDiccionario columna = Array.Find(columnas, item => item.key == words[0]);
        MiDiccionario fila = Array.Find(filas, item => item.key == words[1]);
        int IdFicha = columna.value + fila.value;

        tileList[IdFicha].gameObject.GetComponentInParent<TileController>().UpdateTile();
        Debug.Log(TextoDicho);
        
    }

    void LlamadoApi() {
        StartCoroutine(LlamadoApiCorrutina()); 
    }

    IEnumerator LlamadoApiCorrutina() {
        UnityWebRequest webtictactoe = UnityWebRequest.Get("https://raw.githubusercontent.com/Xerathox/JSONFiles/main/JSONTICTACTOE.json");
        UnityWebRequest webmenu = UnityWebRequest.Get("https://raw.githubusercontent.com/Xerathox/JSONFiles/main/JSONMENUS.json");
        yield return webtictactoe.SendWebRequest();
        yield return webmenu.SendWebRequest();

        if(!webtictactoe.isNetworkError && !webtictactoe.isHttpError){
            Debug.Log("CONEXION CON ÉXITO JSON MICHI");
            textJSON = webtictactoe.downloadHandler.text;            
        }
        else
            Debug.LogWarning("hubo un problema con la web");        
        
        if(!webmenu.isNetworkError && !webmenu.isHttpError){
            Debug.Log("CONEXION CON ÉXITO JSON MENU MICHI");
            textJSONMENU = webmenu.downloadHandler.text;            
        }
        else
            Debug.LogWarning("hubo un problema con la web");   
        Empezar();
    }
}    


