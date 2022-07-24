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

    private Piece selectedPiece;
    private List<Piece> forcedPieces;

    private Vector2 mouseOver; //función del mouse
    private Vector2 startDrag;
    private Vector2 endDrag;

    //Reconocimiento de voz
    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, Action> actions = new Dictionary<string, Action>();

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

        keywordRecognizer = new KeywordRecognizer(actions.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += RecognizedSpeech;
        keywordRecognizer.Start();   
    }
    private void Update() {
        UpdateMouseOver();

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
                if (forcedPieces.Find(fp => fp == p) == null) //no somos capaces de encontrar una ficha forzada
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
        Debug.Log(speech.text);
        actions[speech.text].Invoke();
    }
}
