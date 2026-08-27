using UnityEngine;

public class PoseurDeBombe : MonoBehaviour
{
    [Header("Paramètres")]
    public GameObject prefabBombe; // Le modèle de la bombe à faire apparaître

    // Référence vers l'inventaire du joueur
    private Inventaire monInventaire;

    void Start()
    {
        // Au démarrage, on récupère le script Inventaire qui est sur le même objet (le Bomberman)
        monInventaire = GetComponent<Inventaire>();
    }

    void Update()
    {
        // 1. On écoute la touche Espace
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 2. On demande la permission à l'inventaire avant de poser
            if (monInventaire != null && monInventaire.PeutPoserBombe())
            {
                PoserUneBombe();
            }
        }
    }

    void PoserUneBombe()
    {
        // 3. Récupérer la position exacte du joueur
        Vector3 positionJoueur = transform.position;

        // 4. Calculer le centre de la case (Arrondir les positions X et Z)
        float xArrondi = Mathf.Round(positionJoueur.x);
        float zArrondi = Mathf.Round(positionJoueur.z);

        // 5. Définir la position finale de la bombe
        Vector3 positionSurGrille = new Vector3(xArrondi, -0.75f, zArrondi); 

        // 6. Instancier (créer) la bombe dans le jeu
        Instantiate(prefabBombe, positionSurGrille, Quaternion.identity);

        // 7. Prévenir l'inventaire qu'une bombe vient d'être ajoutée sur la carte
        monInventaire.AjouterBombeSurCarte();
    }
}