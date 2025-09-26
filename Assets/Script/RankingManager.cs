using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ScoreEntry
{
    public string nombre;
    public int puntaje;
}

[System.Serializable]
public class ScoreList
{
    public List<ScoreEntry> lista = new List<ScoreEntry>();
}

public static class RankingManager
{
    private static string key = "RankingData";
    private static int maxEntradas = 3;

    // Guarda el ranking en PlayerPrefs
    public static void SaveRanking(ScoreList scores)
    {
        string json = JsonUtility.ToJson(scores);
        PlayerPrefs.SetString(key, json);
        PlayerPrefs.Save();
    }

    // Carga el ranking desde PlayerPrefs
    public static ScoreList LoadRanking()
    {
        if (PlayerPrefs.HasKey(key))
        {
            string json = PlayerPrefs.GetString(key);
            return JsonUtility.FromJson<ScoreList>(json) ?? new ScoreList();
        }
        return new ScoreList();
    }

    // nuevo puntaje y mantener solo el Top 3
    public static void AddScore(string nombre, int puntaje)
    {
        ScoreList scores = LoadRanking();
        scores.lista.Add(new ScoreEntry { nombre = nombre, puntaje = puntaje });

        // Ordenar de mayor a menor y quedarnos solo con los mejores
        scores.lista = scores.lista
            .OrderByDescending(s => s.puntaje)
            .Take(maxEntradas)
            .ToList();

        SaveRanking(scores);
    }

    // Borrar todo el ranking
    public static void ClearRanking()
    {
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();
    }
}
