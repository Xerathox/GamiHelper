using System;
using System.Linq;
using UnityEngine.Windows.Speech;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour{

    public TextAsset textJSONMENU;

    //Reconocimiento de Voz
    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, Action> actions = new Dictionary<string, Action>();

    void Start() {

        JSONMenuInitializer jSONMenuInitializer = new JSONMenuInitializer();
        jSONMenuInitializer = JsonUtility.FromJson<JSONMenuInitializer>(textJSONMENU.text);
        
        //Reconocimiento de voz
        actions.Add(jSONMenuInitializer.michi, TicTacToe);
        actions.Add(jSONMenuInitializer.memoria, Memoria);
        actions.Add(jSONMenuInitializer.damas, Damas);
        actions.Add(jSONMenuInitializer.salir, SalirDeAplicación); 
        
        keywordRecognizer = new KeywordRecognizer(actions.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += RecognizedSpeech;
        keywordRecognizer.Start();          
    }

    private void RecognizedSpeech(PhraseRecognizedEventArgs speech)
    {
        Debug.Log(speech.text);
        actions[speech.text].Invoke();
    }

    private void TicTacToe(){
        SceneManager.LoadScene(1);
    }
        
    private void Memoria(){
        SceneManager.LoadScene(2);        
    }

    private void Damas(){
        SceneManager.LoadScene(3);      
    } 
     
    public void SalirDeAplicación() {
        Application.Quit();
    }
}


