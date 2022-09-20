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
    //SpeechController objetoSpeechController;

    //Reconocimiento de Voz
    //public KeywordRecognizer keywordRecognizer;
    //public Dictionary<string, Action> actions = new Dictionary<string, Action>();

    void Awake() {
        LlamadoApi();
    }

    void Empezar() {
        JSONMenuInitializer jSONMenuInitializer = new JSONMenuInitializer();
        jSONMenuInitializer = JsonUtility.FromJson<JSONMenuInitializer>(textJSONMENU);
        
        //Reconocimiento de voz        
        SpeechController.instance.actions.Add(jSONMenuInitializer.michi, TicTacToe);
        SpeechController.instance.actions.Add(jSONMenuInitializer.memoria, Memoria);
        SpeechController.instance.actions.Add(jSONMenuInitializer.damas, Damas);
        SpeechController.instance.actions.Add(jSONMenuInitializer.salir, SalirDeAplicación); 

        /*objetoSpeechController = new SpeechController(actions);        
        SpeechController.instance.keywordRecognizer = new KeywordRecognizer(SpeechController.instance.actions.Keys.ToArray());
        SpeechController.instance.keywordRecognizer.OnPhraseRecognized += SpeechController.instance.RecognizedSpeech;
        SpeechController.instance.keywordRecognizer.Start();
        */
        SpeechController.instance.IniciarSpeech();
    }
/*
    public void RecognizedSpeech(PhraseRecognizedEventArgs speech) {
        Debug.Log(speech.text);
        SpeechController.instance.actions[speech.text].Invoke();
    }
*/

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



