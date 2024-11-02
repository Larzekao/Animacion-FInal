using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class BicolaConClave<TKey, TValue>
{
    private LinkedList<KeyValuePair<TKey, TValue>> listaEnlazada;
    private Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> diccionario;

    public BicolaConClave()
    {
        listaEnlazada = new LinkedList<KeyValuePair<TKey, TValue>>();
        diccionario = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>();
    }
    [JsonIgnore]
    public IEnumerable<TValue> Valores
    {
        get
        {
            foreach (var nodo in listaEnlazada)
            {
                yield return nodo.Value;
            }
        }
    }

    public void AgregarAlInicio(TKey clave, TValue valor)
    {
        if (diccionario.ContainsKey(clave))
        {
            throw new ArgumentException("La clave ya existe en la bicola.");
        }

        var nodo = new KeyValuePair<TKey, TValue>(clave, valor);
        var nodoEnlazado = listaEnlazada.AddFirst(nodo);
        diccionario[clave] = nodoEnlazado;
    }

    public void AgregarAlFinal(TKey clave, TValue valor)
    {
        if (diccionario.ContainsKey(clave))
        {
            throw new ArgumentException("La clave ya existe en la bicola.");
        }

        var nodo = new KeyValuePair<TKey, TValue>(clave, valor);
        var nodoEnlazado = listaEnlazada.AddLast(nodo);
        diccionario[clave] = nodoEnlazado;
    }

    public TValue Obtener(TKey clave)
    {
        if (diccionario.TryGetValue(clave, out var nodo))
        {
            return nodo.Value.Value;
        }

        throw new KeyNotFoundException("La clave no existe en la bicola.");
    }

    public bool Eliminar(TKey clave)
    {
        if (diccionario.TryGetValue(clave, out var nodo))
        {
            listaEnlazada.Remove(nodo);
            diccionario.Remove(clave);
            return true;
        }

        return false;
    }

    public bool ContieneClave(TKey clave)
    {
        return diccionario.ContainsKey(clave);
    }

    public void Limpiar()
    {
        listaEnlazada.Clear();
        diccionario.Clear();
    }

    public int Conteo => listaEnlazada.Count;
}
