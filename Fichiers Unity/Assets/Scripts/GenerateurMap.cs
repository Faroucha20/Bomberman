using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[System.Serializable]
public struct ConfigMonstre
{
    public string nom;
    public GameObject prefab;
    public int quantite;
}

public class GenerateurMap : NetworkBehaviour
{
    // --- NOUVEAU : Option pour basculer le comportement de la map ---
    [Header("Paramètres de Jeu")]
    public bool modeMultijoueur = false;

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

    private List<NetworkObject> objetsGeneresSurLeReseau = new List<NetworkObject>();

    public void LancerNouveauNiveau(ConfigNiveau nouvelleConfig)
    {
        if (!IsServer) return;

        NettoyerAncienneMap();

        prefabSol = nouvelleConfig.prefabSol;
        prefabMurIndestructible = nouvelleConfig.prefabMurIndestructible;
        prefabMurDestructible = nouvelleConfig.prefabMurDestructible;
        nbMursDestructibles = nouvelleConfig.nbMursDestructibles;
        monstresAGenerer = nouvelleConfig.monstresAGenerer;

        GenererLaMap();
    }

    public void NettoyerAncienneMap()
    {
        if (!IsServer) return;

        foreach (NetworkObject netObj in objetsGeneresSurLeReseau)
        {
            if (netObj != null && netObj.IsSpawned)
            {
                netObj.Despawn(); 
            }
        }
        objetsGeneresSurLeReseau.Clear();
        
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
        for (int x = -5; x < largeur + 5; x++)
        {
            for (int z = -3; z < hauteur + 3; z++)
            {
                Vector3 position = new Vector3(x, 0f, z);

                if (prefabSol != null)
                {
                    InstancierSurReseau(prefabSol, position - new Vector3(0, 0.5f, 0));
                }

                if (x < 0 || x >= largeur || z < 0 || z >= hauteur)
                {
                    InstancierSurReseau(prefabMurIndestructible, position);
                }
                else if (x == 0 || x == largeur - 1 || z == 0 || z == hauteur - 1)
                {
                    InstancierSurReseau(prefabMurIndestructible, position);
                }
                else if (x % 2 == 0 && z % 2 == 0)
                {
                    InstancierSurReseau(prefabMurIndestructible, position);
                }
                else
                {
                    // --- MODIFICATION : SAFE ZONES DYNAMIQUES ---
                    // Joueur 1 (Haut-Gauche) toujours protégé
                    bool estSafeZone = (x == 1 && z == hauteur - 2) || 
                                       (x == 2 && z == hauteur - 2) || 
                                       (x == 1 && z == hauteur - 3);   

                    // Si on est en multijoueur, on protège les 3 autres coins
                    if (modeMultijoueur)
                    {
                        estSafeZone = estSafeZone || 
                            (x == largeur - 2 && z == 1) || (x == largeur - 3 && z == 1) || (x == largeur - 2 && z == 2) || // Bas-Droite
                            (x == largeur - 2 && z == hauteur - 2) || (x == largeur - 3 && z == hauteur - 2) || (x == largeur - 2 && z == hauteur - 3) || // Haut-Droite
                            (x == 1 && z == 1) || (x == 2 && z == 1) || (x == 1 && z == 2); // Bas-Gauche
                    }

                    if (!estSafeZone)
                    {
                        casesLibres.Add(position);
                    }
                }
            }
        }

        // ÉTAPE 2 : APPARITION DU JOUEUR (Déléguée au NetworkManager)
        Jeu interfaceUI = FindFirstObjectByType<Jeu>();
        if (interfaceUI != null)
        {
            interfaceUI.DemarrerDetectionVictoire();
        }

        // --- MODIFICATION : ÉTAPE 3 : BARRICADES DE SÉCURITÉ ---
        List<Vector3> barricades = new List<Vector3>();
        
        // Joueur 1 (Haut-Gauche)
        barricades.Add(new Vector3(3, 0f, hauteur - 2));
        barricades.Add(new Vector3(1, 0f, hauteur - 4));

        if (modeMultijoueur)
        {
            // Joueur 2 (Bas-Droite)
            barricades.Add(new Vector3(largeur - 4, 0f, 1));
            barricades.Add(new Vector3(largeur - 2, 0f, 3));
            // Joueur 3 (Haut-Droite)
            barricades.Add(new Vector3(largeur - 4, 0f, hauteur - 2));
            barricades.Add(new Vector3(largeur - 2, 0f, hauteur - 4));
            // Joueur 4 (Bas-Gauche)
            barricades.Add(new Vector3(3, 0f, 1));
            barricades.Add(new Vector3(1, 0f, 3));
        }

        foreach (Vector3 pos in barricades)
        {
            InstancierSurReseau(prefabMurDestructible, pos);
            casesLibres.Remove(pos);
        }

        int mursRestantsAPlacer = nbMursDestructibles - barricades.Count;

        // ÉTAPE 4 : PLACEMENT ALÉATOIRE DES MURS DESTRUCTIBLES
        for (int i = 0; i < mursRestantsAPlacer; i++)
        {
            if (casesLibres.Count == 0) break; 

            int indexAleatoire = Random.Range(0, casesLibres.Count);
            Vector3 positionChoisie = casesLibres[indexAleatoire];

            InstancierSurReseau(prefabMurDestructible, positionChoisie);
            casesLibres.RemoveAt(indexAleatoire); 
        }

        // ÉTAPE 5 : PLACEMENT ALÉATOIRE DES MONSTRES
        if (!modeMultijoueur) {
            foreach (ConfigMonstre config in monstresAGenerer)
            {
                for (int i = 0; i < config.quantite; i++)
                {
                    if (casesLibres.Count == 0) break;

                    int indexAleatoire = Random.Range(0, casesLibres.Count);
                    Vector3 positionChoisie = casesLibres[indexAleatoire];
                    Vector3 positionMonstre = new Vector3(positionChoisie.x, positionChoisie.y - 0.5f, positionChoisie.z);

                    InstancierSurReseau(config.prefab, positionMonstre);
                    casesLibres.RemoveAt(indexAleatoire);
                }
            }
        }
    }

    public void NettoyerPlateau()
    {
        if (!IsServer) return;

        NettoyerAncienneMap();

        GameObject[] monstres = GameObject.FindGameObjectsWithTag("Monstre");
        foreach (GameObject m in monstres) 
        {
            NetworkObject netObj = m.GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsSpawned) netObj.Despawn();
            else Destroy(m);
        }

        GameObject[] explosions = GameObject.FindGameObjectsWithTag("Explosion");
        foreach (GameObject e in explosions) 
        {
            NetworkObject netObj = e.GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsSpawned) netObj.Despawn();
            else Destroy(e);
        }

        GameObject[] bonus = GameObject.FindGameObjectsWithTag("Bonus");
        foreach (GameObject b in bonus) 
        {
            NetworkObject netObj = b.GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsSpawned) netObj.Despawn();
            else Destroy(b);
        }
    }

    private void InstancierSurReseau(GameObject prefab, Vector3 position)
    {
        if (prefab == null) return;

        GameObject nouvelObjet = Instantiate(prefab, position, Quaternion.identity);

        NetworkObject netObj = nouvelObjet.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Spawn(); 
            objetsGeneresSurLeReseau.Add(netObj);
        }
        else
        {
            Debug.LogWarning($"Attention, le prefab {prefab.name} n'a pas de NetworkObject !");
        }
    }

    public Vector3 ObtenirPointDeDepart(ulong clientId)
    {
        Vector3 coinHautGauche = new Vector3(1, -0.5f, hauteur - 2);
        Vector3 coinBasDroite = new Vector3(largeur - 2, -0.5f, 1);
        Vector3 coinHautDroite = new Vector3(largeur - 2, -0.5f, hauteur - 2);
        Vector3 coinBasGauche = new Vector3(1, -0.5f, 1);

        switch (clientId % 4) 
        {
            case 0: return coinHautGauche;
            case 1: return coinBasDroite;
            case 2: return coinHautDroite;
            case 3: return coinBasGauche;
            default: return coinHautGauche;
        }
    }
}