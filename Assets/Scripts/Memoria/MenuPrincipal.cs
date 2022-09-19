 using System;
using System.Linq;
using UnityEngine.Windows.Speech;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class MenuPrincipal : MonoBehaviour {

    public string textJSONMENU;

    //Reconocimiento de Voz
    public KeywordRecognizer keywordRecognizer;
    public Dictionary<string, Action> actions = new Dictionary<string, Action>();

    void Awake() {
        LlamadoApi();
    }

    void Empezar() {
        JSONMenuInitializer jSONMenuInitializer = new JSONMenuInitializer();
        jSONMenuInitializer = JsonUtility.FromJson<JSONMenuInitializer>(textJSONMENU);
        
        //Reconocimiento de voz
        actions.Add(jSONMenuInitializer.michi, TicTacToe);
        actions.Add(jSONMenuInitializer.memoria, Memoria);
        actions.Add(jSONMenuInitializer.damas, Damas);
        actions.Add(jSONMenuInitializer.salir, SalirDeAplicación); 
        
        keywordRecognizer = new KeywordRecognizer(actions.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += RecognizedSpeech;
        keywordRecognizer.Start();          
    }

    public void RecognizedSpeech(PhraseRecognizedEventArgs speech) {
        Debug.Log(speech.text);
        actions[speech.text].Invoke();
    }

    public void TicTacToe() {                
        LoadingManager.NextScene(ScreenIndices.TICTACTOE);        
    }
        
    public void Memoria() {
        LoadingManager.NextScene(ScreenIndices.MEMORIA);       
    }

    public void Damas() {
        LoadingManager.NextScene(ScreenIndices.DAMAS);     
    } 
     
    public void SalirDeAplicación() {
        Application.Quit();
    }

    void LlamadoApi () {
        StartCoroutine(LlamadoApiCorrutina()); 
    }

    IEnumerator LlamadoApiCorrutina() {
        List<string> URL = new List<string>();
        URL.Add("https://raw.githubusercontent.com/Xerathox/JSONFiles/main/JSONMENUS.jsona");
        URL.Add("https://raw.githubusercontent.com/Xerathox/JSONFiles2/main/JSONMENUS.json");

        foreach (string i in URL) {
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



