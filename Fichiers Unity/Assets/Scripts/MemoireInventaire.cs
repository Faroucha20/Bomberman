using UnityEngine;

public static class MemoireInventaire
{
    public static bool aUneSauvegarde = false;

    public static int memVitesse = 0;
    public static int memSac = 0;
    public static int memHuile = 0;
    public static float memVitesseJoueur = 1f;
    public static int memPatinsEnJeu = 1;
    public static int memBombesEnJeu = 1;
    public static int memPorteeExplosion = 1;
    public static int memHuileEnJeu = 1;

    public static void Sauvegarder(Inventaire inv)
    {
        memVitesse = inv.reserveVitesse;
        memSac = inv.reserveSac;
        memHuile = inv.reserveHuile;
        memVitesseJoueur = inv.vitesseJoueur;
        memPatinsEnJeu = inv.patinsEnJeu;
        memBombesEnJeu = inv.bombesEnJeu;
        memPorteeExplosion = inv.porteeExplosion;
        memHuileEnJeu = inv.huileEnJeu;
        aUneSauvegarde = true;
    }

    public static void Charger(Inventaire inv)
    {
        if (!aUneSauvegarde) return; 

        inv.reserveVitesse = memVitesse;
        inv.reserveSac = memSac;
        inv.reserveHuile = memHuile;
        inv.vitesseJoueur = memVitesseJoueur;
        inv.patinsEnJeu = memPatinsEnJeu;
        inv.bombesEnJeu = memBombesEnJeu;
        inv.porteeExplosion = memPorteeExplosion;
        inv.huileEnJeu = memHuileEnJeu;
    }

    public static void ViderMemoire()
    {
        aUneSauvegarde = false;
    }
    public static void PerdreBonusActifs()
    {
        // On remet uniquement les statistiques "en jeu" à leurs valeurs de départ
        // (⚠️ Modifie ces chiffres si tes valeurs de base sont différentes dans ton jeu)
        memVitesseJoueur = 5f; // Ta vitesse de base
        memPatinsEnJeu = 0;    // Les bonus de vitesse activés retombent à zéro
        
        memBombesEnJeu = 1;    // On redémarre avec 1 seule bombe posable
        memPorteeExplosion = 1; // La portée redescend à 1 case
        memHuileEnJeu = 0;     // Les bonus de flammes activés retombent à zéro

        // Surtout, on NE TOUCHE PAS à memVitesse, memSac, et memHuile !
        // Ils conservent le butin stocké par le joueur.
    }
}