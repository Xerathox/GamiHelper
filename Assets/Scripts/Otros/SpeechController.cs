using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Windows.Speech;
using UnityEngine;

public class SpeechController {
    private KeywordRecognizer keywordRecognizer;
    public Dictionary<string, Action> actions = new Dictionary<string, Action>();
    
    public SpeechController(Dictionary<string, Action> actions) {
        keywordRecognizer = new KeywordRecognizer(actions.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += RecognizedSpeech;
        keywordRecognizer.Start();
        this.actions = actions;
    }

    public void RecognizedSpeech(PhraseRecognizedEventArgs speech) {      
        //Debug.Log(speech.text);
        //Debug.Log(actions);
        actions[speech.text].Invoke();        
    }
}
