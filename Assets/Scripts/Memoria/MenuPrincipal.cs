using System;
using System.Linq;
using UnityEngine.Windows.Speech;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MenuPrincipal : MonoBehaviour{

    //Reconocimiento de Voz
    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, Action> actions = new Dictionary<string, Action>();

    void Start() {
        actions.Add("michi", TicTacToe);
        actions.Add("tic tac toe", TicTacToe);
        actions.Add("memoria", Memoria);
        actions.Add("damas", Damas);        
        actions.Add("salir", SalirDeAplicación);
        
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


