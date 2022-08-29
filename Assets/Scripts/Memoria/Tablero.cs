using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Tablero : MonoBehaviour {
    [Header("Valores")]
    [SerializeField] int m_AreaDeJuegoX = 4;
    [SerializeField] int m_AreaDeJuegoY = 4;
    [SerializeField] Vector2 m_SeparacionEntreFichas = Vector2.zero;

    [Header("Referencias")]
    [SerializeField] GameObject m_Ficha;    
    [SerializeField] Transform m_areaDeJuego;    

    public int m_FichasRestantes {get;set;}
    [SerializeField] private Sprite[] m_Imagenes;
    public GameController gameController;
    
    public void InicializarTablero() {

        if (m_AreaDeJuegoX * m_AreaDeJuegoY % 2 != 0)
            m_AreaDeJuegoY -=1;        

        Vector2 posicionInicialFicha = CalcularPosicionInicialDeFicha();
        int cantidadDeFichas = m_AreaDeJuegoX * m_AreaDeJuegoY;
        List<int> idsFichas = CrearListaDeIdsMezclada(cantidadDeFichas);

        int fichasCreadas = 0;

        for (int x = 0; x < m_AreaDeJuegoX; x++) {
            for (int y = 0; y < m_AreaDeJuegoY; y++) {
                m_Ficha.GetComponent<Ficha>().gameController = gameController;
                GameObject fichaGO = Instantiate(m_Ficha);

                //agregar ficha al arreglo gamecontroller
                gameController.fichas[fichasCreadas] = fichaGO.GetComponent<Ficha>();

                fichaGO.transform.SetParent(m_areaDeJuego);

                Ficha fichaActual = fichaGO.GetComponent<Ficha>();
                fichaActual.Id = idsFichas[fichasCreadas];
                fichaActual.SetearImagen(m_Imagenes[fichaActual.Id]);

                float posX = (x*m_SeparacionEntreFichas.x) - posicionInicialFicha.x;
                float posY = (y*m_SeparacionEntreFichas.y) - posicionInicialFicha.y;
                fichaGO.transform.localPosition = new Vector3(posX,0,posY);
                fichasCreadas++;
            }
        }
        m_FichasRestantes = fichasCreadas;
    }

    private Vector2 CalcularPosicionInicialDeFicha() {
        float posicionMaximaX = (m_AreaDeJuegoX - 1) * m_SeparacionEntreFichas.x;
        float posicionMaximaY = (m_AreaDeJuegoY - 1) * m_SeparacionEntreFichas.y;

        float mitadPosMaxX = posicionMaximaX/2;
        float mitadPosMaxY = posicionMaximaY/2;
        return new Vector2(mitadPosMaxX,mitadPosMaxY);
    }

    private List<int> CrearListaDeIdsMezclada(int cantidadDeFichas) {
        List<int> idsFichas = new List<int>();

        for (int i = 0; i < cantidadDeFichas; i++)
            idsFichas.Add(i/2);       
        idsFichas.Shuffle();
        return idsFichas;
    }
}
