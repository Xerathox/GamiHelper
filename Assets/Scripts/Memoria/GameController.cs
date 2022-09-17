using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.Windows.Speech;
using UnityEngine.Networking;

public class GameController : MonoBehaviour {    
    private Tablero m_Tablero;
    private bool m_PuedeSeleccionarFicha = true;
    private Ficha m_UltimaSeleccion = null;

    //Reconocimiento de voz
    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, Action> actions = new Dictionary<string, Action>();
    private string TextoDicho;
    public MiDiccionario[] columnas;
    public MiDiccionario[] filas;

    public Ficha[] fichas = new Ficha[16];

    [Header("Particulas")]
    [SerializeField] EmisorDeParticulas emisorDeParticulas;

    [Header("Sonidos")]
    [SerializeField] AudioSource m_SoundFX;
    [SerializeField] AudioClip m_SonidoAcierto;
    [SerializeField] AudioClip m_SonidoError;

    [Header("Paneles")]
    [SerializeField] GameObject PanelVictoria1;
    [SerializeField] GameObject PanelVictoria2;
    [SerializeField] GameObject PanelEmpate;
    [SerializeField] GameObject PanelPausa;

    [Header("Textos en pantalla")]
    [SerializeField] TextMeshProUGUI texto_FichasRestantes;    
    [SerializeField] TextMeshProUGUI texto_puntajeJugador1;
    [SerializeField] TextMeshProUGUI texto_puntajeJugador2;
    private int sumarPuntaje1 = 1;
    private int sumarPuntaje2 = 1;

    //Variables para cambio de turno
    [Header("Conteo de turnos")]
    public string whoPlaysFirst;
    private string playerTurn;

    [Header("Referencias Titulo de barra")]
    public Image playerXIcon;
    public Image playerOIcon;

    [Header("Configuracion del juego")]
    public Color inactivePlayerColor;
    public Color activePlayerColor;   

    //Leer JSON
    public string textJSON;
    public string textJSONMENU;
 
    void Awake() {
        LlamadoApi();
        m_Tablero = GetComponent<Tablero>();        
    }

    void Empezar() {
        //Leer JSON
        JSONInitializer jsonInitializer = new JSONInitializer();   
        jsonInitializer = JsonUtility.FromJson<JSONInitializer>(textJSON);
        columnas = jsonInitializer.columna;
        filas = jsonInitializer.fila;

        JSONMenuInitializer jSONMenuInitializer = new JSONMenuInitializer();
        jSONMenuInitializer = JsonUtility.FromJson<JSONMenuInitializer>(textJSONMENU);        

        //TIC TAC TOE Setapea al primer jugador 
        playerTurn = whoPlaysFirst;

        if (playerTurn == "X")
            playerOIcon.color = inactivePlayerColor;        
        else
            playerXIcon.color = inactivePlayerColor;

        m_Tablero.InicializarTablero();
        ActualizarFichasRestantes();  

        //Reconocimiento de voz
        actions.Add(jSONMenuInitializer.pausa, MostrarMenuPausa);
        actions.Add(jSONMenuInitializer.reanudar, CerrarMenuPausa);
        actions.Add(jSONMenuInitializer.reiniciar, ReiniciarNivel);
        actions.Add(jSONMenuInitializer.cerrar, IrAMenuPrincipal);  

        //Comandos de voz  
        foreach (var columna in columnas) {
            foreach (var fila in filas) {
                actions.Add(columna.key + ' ' +  fila.key , VozManejarTurno);
            }   
        }

        keywordRecognizer = new KeywordRecognizer(actions.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += RecognizedSpeech;
        keywordRecognizer.Start();       
    }

    public void ProcesarClickEnFicha(Ficha ficha) {
        if (!m_PuedeSeleccionarFicha)
            return;
        if (!m_UltimaSeleccion) {
            //Significa que esta es la primera ficha que damos vuelta
            PrimeraFichaSeleccionada(ficha);
        }
        else {
            //Significa que es la segunda ficha que damos vuelta
            SegundaFichaSeleccionada(ficha);
        }        
    }
    private void PrimeraFichaSeleccionada(Ficha ficha) {
        m_UltimaSeleccion = ficha;
        ficha.MostrarFrente();
    }
    private void SegundaFichaSeleccionada(Ficha ficha) {
        if (ficha == m_UltimaSeleccion)
            return;        
        ficha.MostrarFrente();
        if (ficha.Id == m_UltimaSeleccion.Id)
            ParCorrecto(ficha,m_UltimaSeleccion);        
        else 
            ParIncorrecto(ficha, m_UltimaSeleccion);           
    }

    private void ParCorrecto(Ficha ficha, Ficha ultimaSeleccion){
        //Destruir ambas fichas
        Destroy(ficha.gameObject, 1.5f);
        Destroy(ultimaSeleccion.gameObject, 1.5f);

        //Emitir particulas de acierto
        emisorDeParticulas.EmitirParticulasDeAcierto(ficha.transform);
        emisorDeParticulas.EmitirParticulasDeAcierto(ultimaSeleccion.transform);

        //Emitir sonido acierto
        if (m_SonidoAcierto != null)
            m_SoundFX.PlayOneShot(m_SonidoAcierto);               
        //Resetear m_ultimaseleccion
        m_UltimaSeleccion = null;
        //Bloquear selección por cierto tiempo
        StartCoroutine(BloquearSeleccionPorTiempo(1.5f));
        //Restar 2 fichas o estas fichas a la cantidad de fichas restantes para ganar 
        m_Tablero.m_FichasRestantes -= 2;
        ActualizarFichasRestantes();

        if (playerTurn == "X") {            
            texto_puntajeJugador1.text = "Puntaje: " + sumarPuntaje1.ToString();
            sumarPuntaje1++;            
        } else {
            texto_puntajeJugador2.text = "Puntaje: " + sumarPuntaje2.ToString();
            sumarPuntaje2++;
        }

        //Comprobar si ganamos el juego
        if (m_Tablero.m_FichasRestantes == 0 && sumarPuntaje1 > sumarPuntaje2)            
            PanelVictoria1.SetActive(true);        
        if (m_Tablero.m_FichasRestantes == 0 && sumarPuntaje1 < sumarPuntaje2)            
            PanelVictoria2.SetActive(true);        
        if (m_Tablero.m_FichasRestantes == 0 && sumarPuntaje1 == sumarPuntaje2)            
            PanelEmpate.SetActive(true);        
    }

    private void ParIncorrecto(Ficha ficha, Ficha ultimaSeleccion){
        //Deshacer las selecciones 
        m_UltimaSeleccion = null;
        
        if (m_SonidoError != null)
            m_SoundFX.PlayOneShot(m_SonidoError);        
        ficha.Invoke("MostrarReverso", 1.5f);
        ultimaSeleccion.Invoke("MostrarReverso", 1.5f);
        StartCoroutine(BloquearSeleccionPorTiempo(1.5f)); 
          
        ChangeTurn();
    }

    IEnumerator BloquearSeleccionPorTiempo(float tiempo){
        m_PuedeSeleccionarFicha = false;
        yield return new WaitForSeconds(tiempo);
        m_PuedeSeleccionarFicha = true;
    }

    public void ActualizarFichasRestantes(){
        if (texto_FichasRestantes != null)
            texto_FichasRestantes.text = "Fichas Restantes:" + m_Tablero.m_FichasRestantes.ToString();        
    }

    //TIC TAC TOE Funciones para Menu Pausa
    public void MostrarMenuPausa() {
        PanelPausa.SetActive(true);
    }
    public void CerrarMenuPausa() {
        PanelPausa.SetActive(false);
    }
    public void ReiniciarNivel() {
        keywordRecognizer = null;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void IrAMenuPrincipal() {
        keywordRecognizer = null;
        SceneManager.LoadScene(ScreenIndices.MAINMENU);
    }

    //Funciones para cambio de turno
    public void ChangeTurn() {
        playerTurn = (playerTurn == "X") ? "O" : "X";
        if (playerTurn == "X") {
            playerXIcon.color = activePlayerColor;
            playerOIcon.color = inactivePlayerColor;            
        } else {
            playerXIcon.color = inactivePlayerColor;
            playerOIcon.color = activePlayerColor;
        }
    }

    public string GetPlayersTurn() {
        return playerTurn;
    }

    //Reconocimiento de voz
    private void RecognizedSpeech(PhraseRecognizedEventArgs speech) {        
        TextoDicho = speech.text;
        actions[speech.text].Invoke();
    }

    public void VozManejarTurno(){
        string[] words = TextoDicho.Split(' ');
        MiDiccionario columna = Array.Find(columnas, item => item.key == words[0]);
        MiDiccionario fila = Array.Find(filas, item => item.key == words[1]);
        int IdFicha = columna.value + fila.value;

        if (fichas[IdFicha])  ProcesarClickEnFicha(fichas[IdFicha]);
            else Debug.Log("Ficha eliminada");
    }

    void LlamadoApi() {
        StartCoroutine(LlamadoApiCorrutina()); 
    }

    IEnumerator LlamadoApiCorrutina() {
        UnityWebRequest webmemoria = UnityWebRequest.Get("https://raw.githubusercontent.com/Xerathox/JSONFiles/main/JSONMEMORIA.json");
        UnityWebRequest webmenu = UnityWebRequest.Get("https://raw.githubusercontent.com/Xerathox/JSONFiles/main/JSONMENUS.json");
        yield return webmemoria.SendWebRequest();
        yield return webmenu.SendWebRequest();

        if(!webmemoria.isNetworkError && !webmemoria.isHttpError) {
            Debug.Log("CONEXION CON ÉXITO JSON MEMORIA");
            textJSON = webmemoria.downloadHandler.text;           
        }
        else
            Debug.LogWarning("hubo un problema con la web");        
            
        if(!webmenu.isNetworkError && !webmenu.isHttpError) {
            Debug.Log("CONEXION CON ÉXITO JSON MENU MEMORIA");
            textJSONMENU = webmenu.downloadHandler.text;            
        }
        else
            Debug.LogWarning("hubo un problema con la web");   
        Empezar();
    }
}

 
