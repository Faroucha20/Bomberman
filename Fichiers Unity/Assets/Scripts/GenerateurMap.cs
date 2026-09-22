using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ConfigMonstre
{
    public string nom;
    public GameObject prefab;
    public int quantite;
}

public class GenerateurMap : MonoBehaviour
{
    [Header("Dimensions de la Map")]
    public int largeur = 15;
    public int hauteur = 13;

    [Header("Éléments de la Map (Remplis par le niveau)")]
    public GameObject prefabMurIndestructible;
    public GameObject prefabMurDestructible;
    public GameObject prefabSol;
    public GameObject prefabBomberman;

    [Header("Paramètres de Génération")]
    public int nbMursDestructibles = 30;
    public ConfigMonstre[] monstresAGenerer;

    // On retire le Start() car la carte ne se génère plus toute seule au lancement !

    // --- NOUVELLE FONCTION : Appelée par le menu pour lancer un niveau ---
    public void LancerNouveauNiveau(ConfigNiveau nouvelleConfig)
    {
        NettoyerAncienneMap();

        // On copie les paramètres de la "fiche" choisie
        prefabSol = nouvelleConfig.prefabSol;
        prefabMurIndestructible = nouvelleConfig.prefabMurIndestructible;
        prefabMurDestructible = nouvelleConfig.prefabMurDestructible;
        nbMursDestructibles = nouvelleConfig.nbMursDestructibles;
        monstresAGenerer = nouvelleConfig.monstresAGenerer;

        // On lance la création !
        GenererLaMap();
    }

    // --- NOUVELLE FONCTION : Supprime tout avant de recréer ---
    public void NettoyerAncienneMap()
    {
        // On détruit tous les enfants de ce GameObject (les anciens murs, monstres, joueur...)
        foreach (Transform enfant in transform)
        {
            Destroy(enfant.gameObject);
        }
    }

    void GenererLaMap()
    {
        if (largeur % 2 == 0) largeur++;
        if (hauteur % 2 == 0) hauteur++;

        List<Vector3> casesLibres = new List<Vector3>();

        // ÉTAPE 1 : GÉNÉRATION DE LA STRUCTURE FIXE
        // --- MODIFICATION ICI : On étend la boucle de -3 à +3 pour créer la bordure ! ---
        for (int x = -3; x < largeur + 3; x++)
        {
            for (int z = -3; z < hauteur + 3; z++)
            {
                Vector3 position = new Vector3(x, 0f, z);

                // On génère le sol partout, même sous la nouvelle grosse bordure
                if (prefabSol != null)
                {
                    Instantiate(prefabSol, position - new Vector3(0, 0.5f, 0), Quaternion.identity, transform);
                }

                // --- NOUVEAU : Si la case est en dehors de la zone de jeu (les fameuses 3 cases) ---
                if (x < 0 || x >= largeur || z < 0 || z >= hauteur)
                {
                    Instantiate(prefabMurIndestructible, position, Quaternion.identity, transform);
                }
                // --- Le reste de ton code original reste intact ! ---
                else if (x == 0 || x == largeur - 1 || z == 0 || z == hauteur - 1)
                {
                    Instantiate(prefabMurIndestructible, position, Quaternion.identity, transform);
                }
                else if (x % 2 == 0 && z % 2 == 0)
                {
                    Instantiate(prefabMurIndestructible, position, Quaternion.identity, transform);
                }
                else
                {
                    bool estSafeZone = (x == 1 && z == hauteur - 2) || 
                                       (x == 2 && z == hauteur - 2) || 
                                       (x == 1 && z == hauteur - 3);   

                    if (!estSafeZone)
                    {
                        casesLibres.Add(position);
                    }
                }
            }
        }

        // ÉTAPE 2 : APPARITION DU JOUEUR (Intact)
        Vector3 positionJoueur = new Vector3(1, -0.5f, hauteur - 2);
        GameObject joueurCree = Instantiate(prefabBomberman, positionJoueur, Quaternion.identity, transform);

        Jeu interfaceUI = FindFirstObjectByType<Jeu>();
        if (interfaceUI != null)
        {
            interfaceUI.AssignerJoueur(joueurCree.GetComponent<Inventaire>());
            interfaceUI.DemarrerDetectionVictoire();
        }

        

        // ÉTAPE 3 : BARRICADES DE SÉCURITÉ (Intact)
        Vector3 barricadeDroite = new Vector3(3, 0f, hauteur - 2);
        Vector3 barricadeBas = new Vector3(1, 0f, hauteur - 4);

        Instantiate(prefabMurDestructible, barricadeDroite, Quaternion.identity, transform);
        Instantiate(prefabMurDestructible, barricadeBas, Quaternion.identity, transform);

        casesLibres.Remove(barricadeDroite);
        casesLibres.Remove(barricadeBas);

        int mursRestantsAPlacer = nbMursDestructibles - 2;

        // ÉTAPE 4 : PLACEMENT ALÉATOIRE DES MURS DESTRUCTIBLES (Intact)
        for (int i = 0; i < mursRestantsAPlacer; i++)
        {
            if (casesLibres.Count == 0) break; 

            int indexAleatoire = Random.Range(0, casesLibres.Count);
            Vector3 positionChoisie = casesLibres[indexAleatoire];

            Instantiate(prefabMurDestructible, positionChoisie, Quaternion.identity, transform);
            casesLibres.RemoveAt(indexAleatoire); 
        }

        // ÉTAPE 5 : PLACEMENT ALÉATOIRE DES MONSTRES (Intact)
        foreach (ConfigMonstre config in monstresAGenerer)
        {
            for (int i = 0; i < config.quantite; i++)
            {
                if (casesLibres.Count == 0) break;

                int indexAleatoire = Random.Range(0, casesLibres.Count);
                Vector3 positionChoisie = casesLibres[indexAleatoire];
                Vector3 positionMonstre = new Vector3(positionChoisie.x, positionChoisie.y - 0.5f, positionChoisie.z);

                Instantiate(config.prefab, positionMonstre, Quaternion.identity, transform);
                casesLibres.RemoveAt(indexAleatoire);
            }
        }
    }

    public void NettoyerPlateau()
    {
        // 1. On détruit tous les blocs (murs, sols)
        foreach (Transform enfant in transform)
        {
            Destroy(enfant.gameObject);
        }

        // 2. On nettoie les monstres
        GameObject[] monstres = GameObject.FindGameObjectsWithTag("Monstre");
        foreach (GameObject m in monstres) Destroy(m);

        // 3. On nettoie les explosions
        GameObject[] explosions = GameObject.FindGameObjectsWithTag("Explosion");
        foreach (GameObject e in explosions) Destroy(e);

        // --- NOUVEAU : On nettoie les bonus restés au sol ---
        GameObject[] bonus = GameObject.FindGameObjectsWithTag("Bonus");
        foreach (GameObject b in bonus) Destroy(b);
    }
}