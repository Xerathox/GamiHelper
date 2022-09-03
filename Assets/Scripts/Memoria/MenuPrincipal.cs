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
    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, Action> actions = new Dictionary<string, Action>();

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

    private void RecognizedSpeech(PhraseRecognizedEventArgs speech) {
        Debug.Log(speech.text);
        actions[speech.text].Invoke();
    }

    private void TicTacToe() {
        
        SceneManager.LoadScene(1);
    }
        
    private void Memoria() {
        SceneManager.LoadScene(2);        
    }

    private void Damas() {
        SceneManager.LoadScene(3);      
    } 
     
    public void SalirDeAplicación() {
        Application.Quit();
    }

    void LlamadoApi () {
        StartCoroutine(LlamadoApiCorrutina()); 
    }

    IEnumerator LlamadoApiCorrutina() {
        UnityWebRequest webmenuprincipal = UnityWebRequest.Get("https://raw.githubusercontent.com/Xerathox/JSONFiles/main/JSONMENUS.json");
        yield return webmenuprincipal.SendWebRequest();

        if(!webmenuprincipal.isNetworkError && !webmenuprincipal.isHttpError) {
            Debug.Log("CONEXION CON ÉXITO JSON MENU PRINCIPAL");
            textJSONMENU = webmenuprincipal.downloadHandler.text;            
        } else
            Debug.LogWarning("hubo un problema con la web");
        Empezar();        
    }
}


