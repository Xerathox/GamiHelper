using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Windows.Speech;
using UnityEngine;

public class SpeechController : MonoBehaviour {
    public KeywordRecognizer keywordRecognizer;
    public Dictionary<string, Action> actions = new Dictionary<string, Action>();
    public string TextoDicho = null;
    public static SpeechController instance;

    void Awake() {
        if (instance == null) {
            instance = this;
        }
        else {
            Debug.Log("instance isn't working");
        }
    }

    public void IniciarSpeech() {
        keywordRecognizer = new KeywordRecognizer(actions.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += RecognizedSpeech;
        keywordRecognizer.Start();
    }

    public void RecognizedSpeech(PhraseRecognizedEventArgs speech) {      
        TextoDicho = speech.text;      
        actions[speech.text].Invoke();        
    }
}