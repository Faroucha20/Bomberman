using UnityEngine;

public class Bombe : MonoBehaviour
{
    [Header("Paramètres visuels")]
    public GameObject prefabExplosion;

    [Header("Paramètres de gameplay")]
    public float tempsAvantExplosion = 3f;
    public int porteeExplosion = 2; // Le nombre de cases que l'explosion va parcourir
    public LayerMask calqueMurs;    // Permet de cibler uniquement la couche "Murs"

    void Start()
    {
        // La méthode Invoke permet d'appeler une fonction après un délai (en secondes)
        Invoke("Exploser", tempsAvantExplosion);
    }

    void Exploser()
    {
        // 1. Créer l'explosion au centre (à l'emplacement exact de la bombe)
        Instantiate(prefabExplosion, transform.position, Quaternion.identity);

        // 2. Propager l'explosion dans les 4 directions cardinales (croix)
        PropagerExplosion(Vector3.forward); // Devant (Axe Z)
        PropagerExplosion(Vector3.back);    // Derrière (-Z)
        PropagerExplosion(Vector3.left);    // Gauche (-X)
        PropagerExplosion(Vector3.right);   // Droite (X)

        // 3. Détruire l'objet bombe (la sphère) de la scène
        Destroy(gameObject);
    }

    void PropagerExplosion(Vector3 direction)
    {
        // On fait une boucle pour avancer case par case selon la portée de la bombe
        for (int i = 1; i <= porteeExplosion; i++)
        {
            // On calcule la position de la case suivante à vérifier
            Vector3 positionAVerifier = transform.position + (direction * i);

            // On crée une petite sphère invisible pour détecter ce qui s'y trouve
            // On limite la recherche au calque "calqueMurs" pour ignorer le sol
            Collider[] objetsTouches = Physics.OverlapSphere(positionAVerifier, 0.4f, calqueMurs);

            // S'il y a quelque chose sur cette case...
            if (objetsTouches.Length > 0)
            {
                Collider objet = objetsTouches[0];

                // On vérifie si c'est un mur destructible grâce au script qu'on lui a attaché précédemment
                MurDestructible mur = objet.GetComponent<MurDestructible>();
                
                if (mur != null)
                {
                    // C'est un mur destructible ! On le détruit et on met une explosion à sa place
                    mur.Detruire();
                    Instantiate(prefabExplosion, positionAVerifier, Quaternion.identity);
                }
                
                // Que ce soit un mur destructible ou indestructible, l'explosion s'arrête ici (elle ne traverse pas les murs)
                break; 
            }
            else
            {
                // Si la case est complètement vide, on place une explosion et la boucle continue vers la case suivante
                Instantiate(prefabExplosion, positionAVerifier, Quaternion.identity);
            }
        }
    }
}