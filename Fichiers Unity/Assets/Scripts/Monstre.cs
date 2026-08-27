using UnityEngine;

public class Monstre : MonoBehaviour
{
    [Header("Paramètres du Monstre")]
    public float vitesse = 1f;
    
    // Le même calque (Layer) que tu as utilisé pour l'explosion de tes bombes !
    public LayerMask calqueMurs; 

    private Vector3 directionActuelle;

    void Start()
    {
        // Au démarrage, le monstre choisit une direction de départ
        ChoisirNouvelleDirection();
    }

    void Update()
    {
        // On regarde si un mur se trouve juste devant le monstre
        if (ObstacleDevant(directionActuelle))
        {
            // S'il y a un mur, on change de sens
            ChoisirNouvelleDirection();
        }
        else
        {
            // Sinon, on avance ! (Space.World permet de rester bien aligné sur la grille)
            transform.Translate(directionActuelle * vitesse * Time.deltaTime, Space.World);
            
            // On oriente le monstre pour qu'il regarde dans la direction où il marche
            if (directionActuelle != Vector3.zero)
            {
                transform.forward = directionActuelle;
            }
        }
    }

    bool ObstacleDevant(Vector3 directionAVerifier)
    {
        // On lance le laser depuis le centre du monstre. 
        // On remonte un peu (Vector3.up * 0.5f) pour que le laser ne frotte pas bêtement contre le sol.
        Vector3 origineLaser = transform.position + Vector3.up * 0.5f;

        // La distance d'anticipation (0.6 est parfait si tes murs font 1x1 mètre)
        float distanceVerif = 0.6f;

        // Physics.Raycast renvoie "true" s'il touche un mur, et "false" si la voie est libre
        return Physics.Raycast(origineLaser, directionAVerifier, distanceVerif, calqueMurs);
    }

    void ChoisirNouvelleDirection()
    {
        // 1. RECENTRAGE MAGIQUE : Avant de tourner, on replace le monstre EXACTEMENT 
        // au milieu de sa case (x et z arrondis). Ça évite qu'il se bloque dans les coins !
        float xArrondi = Mathf.Round(transform.position.x);
        float zArrondi = Mathf.Round(transform.position.z);
        transform.position = new Vector3(xArrondi, transform.position.y, zArrondi);

        // 2. Les 4 directions possibles dans Bomberman
        Vector3[] directionsPossibles = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
        
        bool directionTrouvee = false;
        int tentatives = 0; // Une sécurité pour ne pas faire bugger le jeu

        // On boucle jusqu'à trouver une direction sans mur (avec une limite de 10 essais)
        while (!directionTrouvee && tentatives < 10)
        {
            // On tire une direction au hasard dans notre liste
            int indexHasard = Random.Range(0, directionsPossibles.Length);
            Vector3 directionTest = directionsPossibles[indexHasard];

            // On lance un laser dans cette nouvelle direction. Si c'est libre...
            if (!ObstacleDevant(directionTest))
            {
                directionActuelle = directionTest; // On valide cette direction
                directionTrouvee = true;           // On arrête la boucle
            }
            
            tentatives++;
        }

        // Si après 10 essais on n'a rien trouvé (ex: le monstre est coincé entre 4 bombes), il s'arrête
        if (!directionTrouvee)
        {
            directionActuelle = Vector3.zero;
        }
    }
}