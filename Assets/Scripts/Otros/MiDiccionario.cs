using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//JSON

[Serializable]
public struct MiDiccionario {
    public string key;
    public int value;
};

[System.Serializable]
public class JSONInitializer{
    public MiDiccionario[] columna;
    public MiDiccionario[] fila;
}