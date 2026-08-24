using UnityEngine;

public class PoseurDeBombe : MonoBehaviour
{
    [Header("Paramètres")]
    public GameObject prefabBombe; // Le modèle de la bombe à faire apparaître

    void Update()
    {
        // 1. On écoute la touche Espace
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PoserUneBombe();
        }
    }

    void PoserUneBombe()
    {
        // 2. Récupérer la position exacte du joueur
        Vector3 positionJoueur = transform.position;

        // 3. Calculer le centre de la case (Arrondir les positions X et Z)
        float xArrondi = Mathf.Round(positionJoueur.x);
        float zArrondi = Mathf.Round(positionJoueur.z);

        // 4. Définir la position finale de la bombe
        // Note : Mettez "0f" pour le Y si votre sol est à 0. 
        // Si la bombe est à moitié dans le sol, mettez par exemple "0.5f" à la place.
        Vector3 positionSurGrille = new Vector3(xArrondi, 0f, zArrondi); 

        // 5. Instancier (créer) la bombe dans le jeu
        Instantiate(prefabBombe, positionSurGrille, Quaternion.identity);
    }
}