using UnityEngine;
using System;

public class Inventaire : MonoBehaviour
{
    [Header("Réserves")]
    public int reserveVitesse = 0;
    public int reserveSac = 0;

    [Header("Statistiques Patins")]
    public float vitesseJoueur = 1f;
    public int patinsEnJeu = 1;
    public int maxPatins = 4;

    [Header("Statistiques Bombes")]
    public int bombesEnJeu = 1;       // Limite de bombes posables simultanément
    public int maxBombes = 4;         // Limite maximum de la capacité
    private int bombesActuellementSurCarte = 0; // Compteur invisible pour le jeu

    public int reserveHuile = 0;
    public int porteeExplosion = 1;
    public int huileEnJeu = 1;
    public int maxHuile = 4;



    // Événements pour l'UI
    public event Action<int> OnVitesseChanged;
    public event Action<int> OnSacChanged;
    public event Action<int, int> OnPatinsEnJeuChanged;
    public event Action<int, int> OnBombesEnJeuChanged; // Nouvel événement pour les bombes
    public event Action<int> OnHuileChanged;
    public event Action<int, int> OnHuileEnJeuChanged;

    public void AjouterBonusVitesse()
    {
        reserveVitesse++;
        OnVitesseChanged?.Invoke(reserveVitesse);
    }

    public void AjouterBonusSac()
    {
        reserveSac++;
        OnSacChanged?.Invoke(reserveSac);
    }

    public void AjouterBonusHuile()
    {
        reserveHuile++;
        OnHuileChanged?.Invoke(reserveHuile);
    }

    public void UtiliserPatin()
    {
        if (reserveVitesse > 0 && patinsEnJeu < maxPatins)
        {
            reserveVitesse--;
            patinsEnJeu++;
            vitesseJoueur += 1f;

            OnVitesseChanged?.Invoke(reserveVitesse);
            OnPatinsEnJeuChanged?.Invoke(patinsEnJeu, maxPatins);
        }
    }

    // --- NOUVEAU : UTILISER UN SAC ---
    public void UtiliserSac()
    {
        if (reserveSac > 0 && bombesEnJeu < maxBombes)
        {
            reserveSac--;
            bombesEnJeu++;

            OnSacChanged?.Invoke(reserveSac);
            OnBombesEnJeuChanged?.Invoke(bombesEnJeu, maxBombes);
        }
    }

    public void UtiliserHuile()
    {
        if (reserveHuile > 0 && huileEnJeu < maxHuile)
        {
            reserveHuile--;
            huileEnJeu++;
            porteeExplosion++;

            OnHuileChanged?.Invoke(reserveHuile);
            OnHuileEnJeuChanged?.Invoke(huileEnJeu, maxHuile);
        }
    }

    // --- NOUVEAU : GESTION DES BOMBES SUR LA CARTE ---
    
    // Le script qui pose les bombes appellera ça pour savoir s'il a le droit
    public bool PeutPoserBombe()
    {
        return bombesActuellementSurCarte < bombesEnJeu;
    }

    // A appeler juste après avoir fait apparaître une bombe
    public void AjouterBombeSurCarte()
    {
        bombesActuellementSurCarte++;
    }

    // A appeler quand une bombe explose pour rendre la place
    public void EnleverBombeDeCarte()
    {
        bombesActuellementSurCarte--;
        if (bombesActuellementSurCarte < 0) bombesActuellementSurCarte = 0; // Sécurité
    }
}