using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.Linq;
using UnityEngine.Windows.Speech;
using UnityEngine.UI;

public class CheckersBoard : MonoBehaviour {
    public Piece[,] pieces = new Piece[8,8];
    public GameObject whitePiecePrefab;
    public GameObject blackPiecePrefab;

    private Vector3 boardOffset = new Vector3(-4.0f,0,-4.0f);
    private Vector3 pieceOffset = new Vector3(0.5f,0,0.5f);

    public bool isWhite;
    private bool isWhiteTurn;
    private bool hasKilled;

    [SerializeField] public Piece selectedPiece;
    private List<Piece> forcedPieces;
    
    private Vector2 mouseOver;
    private Vector2 startDrag;
    private Vector2 endDrag;
    
    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, Action> actions = new Dictionary<string, Action>();
    private string textsaid;
    public string column_origin;
    public string row_origin;
    public MiDiccionario[] columns;
    public MiDiccionario[] rows;

    [Header("Paneles")]    
    [SerializeField] GameObject victoryPanel1;
    [SerializeField] GameObject victoryPanel2;    
    [SerializeField] GameObject pausePanel;

    public TextAsset textJSON;
    public TextAsset textJSONMENU;
    
    private void Start() {
        //Read JSON
        JSONInitializer jsonInitializer = new JSONInitializer();   
        jsonInitializer = JsonUtility.FromJson<JSONInitializer>(textJSON.text);
        columns = jsonInitializer.columna;
        rows = jsonInitializer.fila;

        JSONMenuInitializer jSONMenuInitializer = new JSONMenuInitializer();
        jSONMenuInitializer = JsonUtility.FromJson<JSONMenuInitializer>(textJSONMENU.text);        

        isWhiteTurn = true;
        forcedPieces = new List<Piece>();

        GenerateBoard();

        //Voice Recognizer
        actions.Add(jSONMenuInitializer.pausa, ShowMenuPause);
        actions.Add(jSONMenuInitializer.reanudar, CloseMenuPause);
        actions.Add(jSONMenuInitializer.reiniciar, RestartLevel);
        actions.Add(jSONMenuInitializer.cerrar, GoMainMenu);  
     
        //Comand voice
        foreach (var column in columns) 
            foreach (var row in rows)
                actions.Add(column.key + ' ' +  row.key , VoiceProcessor);        

        keywordRecognizer = new KeywordRecognizer(actions.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += RecognizedSpeech;
        keywordRecognizer.Start();   
    }
    private void Update() {
        UpdateMouseOver();

        if (Input.GetMouseButtonDown(0)) {
            Debug.Log((int)mouseOver.x + "," + (int)mouseOver.y);
        }

        //Turns logic
        if ((isWhite?isWhiteTurn : !isWhiteTurn)) {  
            int x = (int)mouseOver.x;
            int y = (int)mouseOver.y;

            if (selectedPiece != null)
                UpdatePieceDrag(selectedPiece);
            if (Input.GetMouseButtonDown(0))
                SelectPiece(x,y);
            if(Input.GetMouseButtonUp(0))
                TryMove((int)startDrag.x,(int)startDrag.y,x,y);
        }
    }        

    //Movement Logic
    private void UpdateMouseOver() {
        if (!Camera.main) {
            Debug.Log("No se pudo encontrar la cámara principal");
            return;
        }
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),out hit, 25.0f,LayerMask.GetMask("Board"))) {
            mouseOver.x = (int)(hit.point.x - boardOffset.x);
            mouseOver.y = (int)(hit.point.z - boardOffset.z);
        }
        else {
            mouseOver.x = -1;
            mouseOver.y = -1;
        }
    }
    private void UpdatePieceDrag(Piece p) {
        //get the info that was obtained when the raycast was made
        if (!Camera.main) {
            Debug.Log("No se pudo encontra la cámara principal");
            return;
        }
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),out hit, 25.0f, LayerMask.GetMask("Board"))) {
            p.transform.position = hit.point + Vector3.up;
        }
    }
    private void SelectPiece (int x, int y) {
        //if we are ouy of the array
        if (x < 0 || x >= 8 || y < 0 || y >= 8)
            return;
        
        Piece p = pieces[x,y];
        if (p != null && p.isWhite == isWhite) {
            if (forcedPieces.Count == 0) {
                selectedPiece = p;
                startDrag = mouseOver;
            }            
            else { 
                // Search for the piece in our forced piece list
                if (forcedPieces.Find(fp => fp == p) == null) //we aren't able to find a forced piece
                    return;
                selectedPiece = p;
                startDrag = mouseOver;
            }
        }        
    }
    private void TryMove(int x1, int y1, int x2, int y2) {
        forcedPieces = ScanForPossibleMove();

        startDrag = new Vector2(x1,y2);
        endDrag = new Vector2(x2,y2);
        selectedPiece = pieces[x1,y1];

        //Out of bounds
        if (x2 < 0 || x2 >= 8 || y2 < 0 || y2 >= 8) {            
            if (selectedPiece != null)
                MovePiece(selectedPiece,x1,y1);
            startDrag = Vector2.zero;
            selectedPiece = null;
            return;
        }
        if (selectedPiece != null) {
            //If a piece hasn't moved
            if (endDrag == startDrag) {
                MovePiece(selectedPiece, x1,y1);
                startDrag = Vector2.zero;
                selectedPiece = null;
                return;
            }

            //Verify if it's a valid move
            if (selectedPiece.ValidMove(pieces,x1,y1,x2,y2)) {
                //Do we kill a piece?                
                if (Mathf.Abs(x2 - x1) == 2){
                    Piece p = pieces[(x1 + x2) / 2,( y1 + y2 ) / 2];
                    if(p!= null) {
                        pieces[(x1 + x2) / 2,( y1 + y2 ) / 2] = null;
                        DestroyImmediate(p.gameObject);
                        hasKilled = true;
                    }
                }

                //It's supposed to kill a piece?
                if (forcedPieces.Count != 0 && !hasKilled) {
                    MovePiece(selectedPiece, x1,y1);
                    startDrag = Vector2.zero;
                    selectedPiece = null;
                    return;
                }
                pieces[x2,y2] = selectedPiece;
                pieces[x1,y1] = null;
                MovePiece(selectedPiece, x2,y2);

                EndTurn();
            }
            else {
                MovePiece(selectedPiece, x1,y1);
                startDrag = Vector2.zero;
                selectedPiece = null;
                return;
            }
        }
    }
    private void EndTurn() {
        int x = (int)endDrag.x;
        int y = (int)endDrag.y;

        //King Piece promotion
        if(selectedPiece != null){
            if(selectedPiece.isWhite && !selectedPiece.isKing && y == 7) {
                selectedPiece.isKing = true;
                selectedPiece.transform.Rotate(Vector3.right * 180);
            }
            else if(!selectedPiece.isWhite && !selectedPiece.isKing && y == 0) {
                selectedPiece.isKing = true;
                selectedPiece.transform.Rotate(Vector3.right * 180);
            }
        }

        selectedPiece = null;
        startDrag = Vector2.zero;

        if (ScanForPossibleMove(selectedPiece,x,y).Count != 0 && hasKilled)
            return;

        isWhiteTurn = !isWhiteTurn;
        isWhite = !isWhite;
        hasKilled = false;
        CheckVictory();
    }
    private List<Piece> ScanForPossibleMove(Piece p, int x, int y) {
        forcedPieces = new List<Piece>();

        if(pieces[x,y].isForceToMove(pieces,x,y))
            forcedPieces.Add(pieces[x,y]);

        return forcedPieces;
    }
    private List<Piece> ScanForPossibleMove(){
        forcedPieces = new List<Piece>();
        //Check all pieces
        for (int i = 0; i < 8;i++)
            for (int j = 0; j < 8; j++)
                if (pieces[i,j] != null && pieces[i,j].isWhite == isWhiteTurn)
                    if(pieces[i,j].isForceToMove(pieces,i,j))
                        forcedPieces.Add(pieces[i,j]);
        return forcedPieces;
    }

    //Win Condition
    private void CheckVictory() {
        var ps = FindObjectsOfType<Piece>();
        bool hasWhite = false, hasblack = false;
        for (int i = 0; i < ps.Length; i++) {
            if(ps[i].isWhite)
                hasWhite = true;
            else
                hasblack = true;            
        }

        if (!hasWhite)
            Victory(false);
        if (!hasblack)
            Victory(true);
    }    
    private void Victory(bool isWhite) {
        if (isWhite)            
            victoryPanel1.SetActive(true);
        else
            victoryPanel2.SetActive(true);            
    }

    //Pieces Generator
    private void GenerateBoard() {
        //Generate white team
        for (int y = 0; y < 3; y++) {
            bool oddRow = (y % 2 == 0);
            for (int x = 0; x < 8; x+=2) {
                //Generate the piece               
                GeneratePiece((oddRow) ? x : x+1,y);
            }
        }
        
        //Generate black team
        for (int y = 7; y > 4; y--) {
            bool oddRow = (y % 2 == 0);
            for (int x = 0; x < 8; x+=2) {
                //Generate the piece                
                GeneratePiece((oddRow) ? x : x+1,y);
            }
        }        
    }
    private void GeneratePiece(int x, int y) {
        bool isPieceWhite = (y > 3) ? false : true;
        GameObject go = Instantiate((isPieceWhite) ? whitePiecePrefab : blackPiecePrefab) as GameObject;
        go.transform.SetParent(transform);
        Piece p = go.GetComponent<Piece>();
        pieces[x,y] = p;
        MovePiece(p,x,y);
    }
    private void MovePiece(Piece p, int x, int y) {
        p.transform.position = (Vector3.right * x) + (Vector3.forward * y) + boardOffset + pieceOffset;
    }

    //Menu Pause System
    public void ShowMenuPause() {
        pausePanel.SetActive(true);
    }
    public void CloseMenuPause() {
        pausePanel.SetActive(false);
    }
    public void RestartLevel() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void GoMainMenu() {
        SceneManager.LoadScene("MainMenu");
    }

    //Voice Recognizer
    private void RecognizedSpeech(PhraseRecognizedEventArgs speech) {        
        textsaid = speech.text;
        actions[speech.text].Invoke();
    }
    private void VoiceProcessor() {
        string[] words = textsaid.Split(' ');

        MiDiccionario column = Array.Find(columns, item => item.key == words[0]);
        MiDiccionario row = Array.Find(rows, item => item.key == words[1]);

        // Turns logic
        if ((isWhite?isWhiteTurn : !isWhiteTurn)) {
            if (selectedPiece == null) {
                SelectPiece(column.value,row.value);                
                startDrag = new Vector2(column.value,row.value);
                if (selectedPiece != null) {
                    column_origin = column.key;
                    row_origin = row.key;
                }
            } 
            else
                TryMove((int)startDrag.x,(int)startDrag.y,column.value,row.value);                   
            Debug.Log(textsaid);
        }
    }
}