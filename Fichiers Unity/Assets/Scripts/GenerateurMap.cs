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
        for (int x = 0; x < largeur; x++)
        {
            for (int z = 0; z < hauteur; z++)
            {
                Vector3 position = new Vector3(x, 0f, z);

                if (prefabSol != null)
                {
                    Instantiate(prefabSol, position - new Vector3(0, 0.5f, 0), Quaternion.identity, transform);
                }

                if (x == 0 || x == largeur - 1 || z == 0 || z == hauteur - 1)
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

        // ÉTAPE 2 : APPARITION DU JOUEUR (On ajoute 'transform' à la fin pour le ranger dans le dossier !)
        Vector3 positionJoueur = new Vector3(1, -0.5f, hauteur - 2);
        GameObject joueurCree = Instantiate(prefabBomberman, positionJoueur, Quaternion.identity, transform);

        UIManager interfaceUI = FindFirstObjectByType<UIManager>();
        if (interfaceUI != null)
        {
            interfaceUI.AssignerJoueur(joueurCree.GetComponent<Inventaire>());
        }

        // ÉTAPE 3 : BARRICADES DE SÉCURITÉ
        Vector3 barricadeDroite = new Vector3(3, 0f, hauteur - 2);
        Vector3 barricadeBas = new Vector3(1, 0f, hauteur - 4);

        Instantiate(prefabMurDestructible, barricadeDroite, Quaternion.identity, transform);
        Instantiate(prefabMurDestructible, barricadeBas, Quaternion.identity, transform);

        casesLibres.Remove(barricadeDroite);
        casesLibres.Remove(barricadeBas);

        int mursRestantsAPlacer = nbMursDestructibles - 2;

        // ÉTAPE 4 : PLACEMENT ALÉATOIRE DES MURS DESTRUCTIBLES
        for (int i = 0; i < mursRestantsAPlacer; i++)
        {
            if (casesLibres.Count == 0) break; 

            int indexAleatoire = Random.Range(0, casesLibres.Count);
            Vector3 positionChoisie = casesLibres[indexAleatoire];

            Instantiate(prefabMurDestructible, positionChoisie, Quaternion.identity, transform);
            casesLibres.RemoveAt(indexAleatoire); 
        }

        // ÉTAPE 5 : PLACEMENT ALÉATOIRE DES MONSTRES (On ajoute 'transform' à la fin aussi)
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
}