using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.Linq;
using UnityEngine.Windows.Speech;
using UnityEngine.UI;

public class CheckersBoard : MonoBehaviour
{
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

    private Vector2 mouseOver; //función del mouse
    private Vector2 startDrag;
    private Vector2 endDrag;

    //Reconocimiento de voz
    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, Action> actions = new Dictionary<string, Action>();
    private string TextoDicho;

    [Header("Paneles")]    
    [SerializeField] GameObject PanelVictoria1;
    [SerializeField] GameObject PanelVictoria2;    
    [SerializeField] GameObject PanelPausa;

    private void Start(){
        isWhiteTurn = true;
        forcedPieces = new List<Piece>();

        GenerateBoard();

        //Reconocimiento de voz
        actions.Add("pausa", MostrarMenuPausa);
        actions.Add("reanudar", CerrarMenuPausa);
        actions.Add("reiniciar", ReiniciarNivel);
        actions.Add("cerrar", IrAMenuPrincipal);        

        //Comandos de voz       
        actions.Add("a uno", VozProcesar);
        actions.Add("b uno", VozProcesar);
        actions.Add("c uno", VozProcesar);
        actions.Add("de uno", VozProcesar);
        actions.Add("e uno", VozProcesar);
        actions.Add("f uno", VozProcesar);
        actions.Add("g uno", VozProcesar);
        actions.Add("h uno", VozProcesar);
        actions.Add("a dos", VozProcesar);
        actions.Add("b dos", VozProcesar);
        actions.Add("c dos", VozProcesar);
        actions.Add("de dos", VozProcesar);
        actions.Add("e dos", VozProcesar);
        actions.Add("f dos", VozProcesar);
        actions.Add("g dos", VozProcesar);
        actions.Add("h dos", VozProcesar);
        actions.Add("a tres", VozProcesar);
        actions.Add("b tres", VozProcesar);
        actions.Add("c tres", VozProcesar); 
        actions.Add("de tres", VozProcesar);
        actions.Add("e tres", VozProcesar);
        actions.Add("f tres", VozProcesar);
        actions.Add("g tres", VozProcesar); 
        actions.Add("h tres", VozProcesar);
        actions.Add("a cuatro", VozProcesar);
        actions.Add("b cuatro", VozProcesar);
        actions.Add("c cuatro", VozProcesar); 
        actions.Add("de cuatro", VozProcesar);  
        actions.Add("e cuatro", VozProcesar);
        actions.Add("f cuatro", VozProcesar);
        actions.Add("g cuatro", VozProcesar); 
        actions.Add("h cuatro", VozProcesar);  
        actions.Add("a cinco", VozProcesar);
        actions.Add("b cinco", VozProcesar);
        actions.Add("c cinco", VozProcesar); 
        actions.Add("de cinco", VozProcesar);  
        actions.Add("e cinco", VozProcesar);
        actions.Add("f cinco", VozProcesar);
        actions.Add("g cinco", VozProcesar); 
        actions.Add("h cinco", VozProcesar);  
        actions.Add("a seis", VozProcesar);
        actions.Add("b seis", VozProcesar);
        actions.Add("c seis", VozProcesar); 
        actions.Add("de seis", VozProcesar);  
        actions.Add("e seis", VozProcesar);
        actions.Add("f seis", VozProcesar);
        actions.Add("g seis", VozProcesar); 
        actions.Add("h seis", VozProcesar);  
        actions.Add("a siete", VozProcesar);
        actions.Add("b siete", VozProcesar);
        actions.Add("c siete", VozProcesar); 
        actions.Add("de siete", VozProcesar);  
        actions.Add("e siete", VozProcesar);
        actions.Add("f siete", VozProcesar);
        actions.Add("g siete", VozProcesar); 
        actions.Add("h siete", VozProcesar);  
        actions.Add("a ocho", VozProcesar);
        actions.Add("b ocho", VozProcesar);
        actions.Add("c ocho", VozProcesar); 
        actions.Add("de ocho", VozProcesar);  
        actions.Add("e ocho", VozProcesar);
        actions.Add("f ocho", VozProcesar);
        actions.Add("g ocho", VozProcesar); 
        actions.Add("h ocho", VozProcesar);  

        keywordRecognizer = new KeywordRecognizer(actions.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += RecognizedSpeech;
        keywordRecognizer.Start();   
    }
    private void Update() {
        UpdateMouseOver();

        if (Input.GetMouseButtonDown(0)) {
            Debug.Log((int)mouseOver.x + "," + (int)mouseOver.y);
        }

        if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
        {  
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

    private void UpdateMouseOver() {
        if (!Camera.main) {
            Debug.Log("No se pudo encontra la cámara principal");
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
        //conseguir la info que se obtuvo cuando se hizo el raycast
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
        //si estamos fuera del arreglo
        if(x < 0 || x >= 8 || y < 0 || y >= 8)
            return;
        
        Piece p = pieces[x,y];
        if (p != null && p.isWhite == isWhite) {

            if(forcedPieces.Count == 0) {
                selectedPiece = p;
                startDrag = mouseOver;

            }            
            else { 
                //Buscar por la pieza en nuestra lista de piezas forzadas 
                if (forcedPieces.Find(fp => fp == p) == null) //no somos capaces de encontrar una pieza forzada
                    return;
                selectedPiece = p;
                startDrag = mouseOver;
            }
        }
        
    }
    private void TryMove(int x1, int y1, int x2, int y2) {
        forcedPieces = ScanForPossibleMove();

        //Soporte multijugador
        startDrag = new Vector2(x1,y2);
        endDrag = new Vector2(x2,y2);
        selectedPiece = pieces[x1,y1];

        //Fuera de los límites
        if (x2 < 0 || x2 >= 8 || y2 < 0 || y2 >= 8) {            
            if (selectedPiece != null)
                MovePiece(selectedPiece,x1,y1);

            startDrag = Vector2.zero;
            selectedPiece = null;
            return;
        }
        if (selectedPiece != null) {
            //Si no se ha movido la ficha
            if(endDrag == startDrag) {
                MovePiece(selectedPiece, x1,y1);
                startDrag = Vector2.zero;
                selectedPiece = null;
                return;
            }

            //Verificar si es un movimiento válido
            if (selectedPiece.ValidMove(pieces,x1,y1,x2,y2)) {
                //Matamos a alguien?
                //Si es un salto
                if (Mathf.Abs(x2 - x1) == 2){
                    Piece p = pieces[(x1 + x2) / 2,( y1 + y2 ) / 2];
                    if(p!= null) {
                        pieces[(x1 + x2) / 2,( y1 + y2 ) / 2] = null;
                        DestroyImmediate(p.gameObject);
                        hasKilled = true;
                    }
                }

                //Se suponía que teníamos que matar algo?
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

        //Promoción
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

        if(ScanForPossibleMove(selectedPiece,x,y).Count != 0 && hasKilled)
            return;

        isWhiteTurn = !isWhiteTurn;
        isWhite = !isWhite;
        hasKilled = false;
        CheckVictory();
    }

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
            Victory(false); //TODO condición de victoria con menus
        if (!hasblack)
            Victory(true);
    }    
    private void Victory(bool isWhite) {
        if (isWhite)            
            PanelVictoria1.SetActive(true);
        else
            PanelVictoria2.SetActive(true);            
    }

    private List<Piece> ScanForPossibleMove(Piece p, int x, int y)   {
        forcedPieces = new List<Piece>();

        if(pieces[x,y].isForceToMove(pieces,x,y))
            forcedPieces.Add(pieces[x,y]);

        return forcedPieces;
    }
    private List<Piece> ScanForPossibleMove(){

        forcedPieces = new List<Piece>();

        //Chequear todas las piezas
        for (int i = 0; i < 8;i++)
            for (int j = 0; j < 8; j++)
                if (pieces[i,j] != null && pieces[i,j].isWhite == isWhiteTurn)
                    if(pieces[i,j].isForceToMove(pieces,i,j))
                        forcedPieces.Add(pieces[i,j]);
        return forcedPieces;
    }

    private void GenerateBoard() {
        //Generar al equipo blanco
        for (int y = 0; y < 3; y++) {
            bool oddRow = (y % 2 == 0);
            for (int x = 0; x < 8; x+=2) {
                //Generar la pieza                
                GeneratePiece((oddRow) ? x : x+1,y);
            }
        }
        
        //Generar al equipo negro
        for (int y = 7; y > 4; y--) {
            bool oddRow = (y % 2 == 0);
            for (int x = 0; x < 8; x+=2) {
                //Generar la pieza                
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

    //TIC TAC TOE Funciones para Menu Pausa
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
        SceneManager.LoadScene("MainMenu");
    }

    //Reconocimiento de voz
    private void RecognizedSpeech(PhraseRecognizedEventArgs speech)
    {
        //Debug.Log(speech.text);
        TextoDicho = speech.text;
        actions[speech.text].Invoke();
    }

    private void VozProcesar(){
        switch (TextoDicho)
        {
            case "a uno":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(0,7);   
                        startDrag = new Vector2(0,7);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,0,7);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "b uno":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(1,7);   
                        startDrag = new Vector2(1,7);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,1,7);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "c uno":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(2,7);   
                        startDrag = new Vector2(2,7);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,2,7);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "de uno": 
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(3,7);   
                        startDrag = new Vector2(3,7);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,3,7);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "e uno":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(4,7);   
                        startDrag = new Vector2(4,7);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,4,7);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "f uno":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(5,7);   
                        startDrag = new Vector2(5,7);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,5,7);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "g uno": 
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(6,7);   
                        startDrag = new Vector2(6,7);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,6,7);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "h uno":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(7,7);   
                        startDrag = new Vector2(7,7);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,7,7);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "a dos": 
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(0,6);   
                        startDrag = new Vector2(0,6);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,0,6);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "b dos":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(1,6);   
                        startDrag = new Vector2(1,6);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,1,6);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "c dos":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(2,6);   
                        startDrag = new Vector2(2,6);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,2,6);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "de dos": 
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(3,6);   
                        startDrag = new Vector2(3,6);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,3,6);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "e dos":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(4,6);   
                        startDrag = new Vector2(4,6);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,4,6);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "f dos":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(5,6);   
                        startDrag = new Vector2(5,6);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,5,6);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "g dos": 
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(6,6);   
                        startDrag = new Vector2(6,6);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,6,6);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "h dos":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(7,6);   
                        startDrag = new Vector2(7,6);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,7,6);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "a tres": 
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(0,5);   
                        startDrag = new Vector2(0,5);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,0,5);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "b tres":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(1,5);   
                        startDrag = new Vector2(1,5);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,1,5);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "c tres":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(2,5);   
                        startDrag = new Vector2(2,5);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,2,5);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "de tres": 
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(3,5);   
                        startDrag = new Vector2(3,5);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,3,5);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "e tres":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(4,5);   
                        startDrag = new Vector2(4,5);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,4,5);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "f tres":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(5,5);   
                        startDrag = new Vector2(5,5);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,5,5);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "g tres": 
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(6,5);   
                        startDrag = new Vector2(6,5);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,6,5);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "h tres":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(7,5);   
                        startDrag = new Vector2(7,5);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,7,5);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;       
            case "a cuatro": 
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(0,4);   
                        startDrag = new Vector2(0,4);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,0,4);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "b cuatro":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(1,4);   
                        startDrag = new Vector2(1,4);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,1,4);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "c cuatro":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(2,4);   
                        startDrag = new Vector2(2,4);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,2,4);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "de cuatro": 
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(3,4);   
                        startDrag = new Vector2(3,4);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,3,4);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "e cuatro":
                 if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(4,4);   
                        startDrag = new Vector2(4,4);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,4,4);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "f cuatro":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(5,4);   
                        startDrag = new Vector2(5,4);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,5,4);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "g cuatro": 
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(6,4);   
                        startDrag = new Vector2(6,4);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,6,4);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "h cuatro":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(7,4);   
                        startDrag = new Vector2(7,4);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,7,4);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;          
            case "a cinco": 
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(0,3);   
                        startDrag = new Vector2(0,3);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,0,3);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "b cinco":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(1,3);   
                        startDrag = new Vector2(1,3);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,1,3);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "c cinco":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(2,3);   
                        startDrag = new Vector2(2,3);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,2,3);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "de cinco": 
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(3,3);   
                        startDrag = new Vector2(3,3);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,3,3);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "e cinco":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(4,3);   
                        startDrag = new Vector2(4,3);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,4,3);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "f cinco":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(5,3);   
                        startDrag = new Vector2(5,3);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,5,3);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "g cinco": 
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(6,3);   
                        startDrag = new Vector2(6,3);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,6,3);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "h cinco":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(0,3);   
                        startDrag = new Vector2(0,3);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,0,3);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;          
            case "a seis": 
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(0,2);   
                        startDrag = new Vector2(0,2);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,0,2);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "b seis":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(1,2);   
                        startDrag = new Vector2(1,2);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,1,2);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "c seis":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(2,2);   
                        startDrag = new Vector2(2,2);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,2,2);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "de seis": 
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(3,2);   
                        startDrag = new Vector2(3,2);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,3,2);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "e seis":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(4,2);   
                        startDrag = new Vector2(4,2);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,4,2);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "f seis":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(5,2);   
                        startDrag = new Vector2(5,2);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,5,2);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "g seis": 
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(6,2);   
                        startDrag = new Vector2(6,2);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,6,2);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "h seis":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(7,2);   
                        startDrag = new Vector2(7,2);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,7,2);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;          
            case "a siete": 
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(0,1);   
                        startDrag = new Vector2(0,1);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,0,1);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "b siete":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(1,1);   
                        startDrag = new Vector2(1,1);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,1,1);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "c siete":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(2,1);   
                        startDrag = new Vector2(2,1);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,2,1);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "de siete": 
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(3,1);   
                        startDrag = new Vector2(3,1);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,3,1);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "e siete":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(4,1);   
                        startDrag = new Vector2(4,1);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,4,1);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "f siete":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(5,1);   
                        startDrag = new Vector2(5,1);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,5,1);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "g siete": 
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(6,1);   
                        startDrag = new Vector2(6,1);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,6,1);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "h siete":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(7,1);   
                        startDrag = new Vector2(7,1);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,7,1);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;           
            case "a ocho": 
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(0,0);   
                        startDrag = new Vector2(0,0);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,0,0);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "b ocho":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(1,0);   
                        startDrag = new Vector2(1,0);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,1,0);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "c ocho":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(2,0);   
                        startDrag = new Vector2(2,0);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,2,0);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "de ocho": 
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(3,0);   
                        startDrag = new Vector2(3,0);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,3,0);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "e ocho":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(4,0);   
                        startDrag = new Vector2(4,0);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,4,0);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "f ocho":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(5,0);   
                        startDrag = new Vector2(5,0);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,5,0);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "g ocho": 
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(6,0);   
                        startDrag = new Vector2(6,0);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,6,0);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;
            case "h ocho":
                if((isWhite?isWhiteTurn : !isWhiteTurn)) //logica de turnos
                {
                    if (selectedPiece == null) {
                        SelectPiece(7,0);   
                        startDrag = new Vector2(7,0);
                    } 
                    else {
                        TryMove((int)startDrag.x,(int)startDrag.y,7,0);
                    }                        
                    Debug.Log(TextoDicho);
                }
                break;                        
            default:
                break;
        }        
    }


}