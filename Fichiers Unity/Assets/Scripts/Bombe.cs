using UnityEngine;

public class Bombe : MonoBehaviour
{
    [Header("Paramètres visuels")]
    public GameObject prefabExplosion;

    [Header("Paramètres de gameplay")]
    public float tempsAvantExplosion = 3f;
    public int porteeExplosion = 1; // La portée (sera modifiée par celui qui la pose !)
    public LayerMask calqueMurs;    

    // NOUVEAU : On cache cette variable dans l'Inspector car c'est le code qui va la remplir
    [HideInInspector] public Inventaire inventaireJoueur;

    private Collider monCollider;

    void Start()
    {
        // /!\ ATTENTION /!\ 
        // On ne cherche PLUS l'inventaire ici ! C'est le PoseurDeBombe qui va le transmettre.
        // Si c'est un monstre qui pose la bombe, "inventaireJoueur" restera vide (null), 
        // ce qui est parfait car le monstre n'a pas d'inventaire !

        monCollider = GetComponent<Collider>();
        if (monCollider != null)
        {
            monCollider.isTrigger = true; 
        }

        // Lance le compte à rebours
        Invoke("Exploser", tempsAvantExplosion);
    }

    void Exploser()
    {
        // Si la bombe appartient au joueur (donc si l'inventaire n'est pas vide), on lui rend sa bombe !
        if (inventaireJoueur != null)
        {
            inventaireJoueur.EnleverBombeDeCarte();
        }

        Vector3 positionExplosion = transform.position + new Vector3(0, 0.5f, 0);
        Instantiate(prefabExplosion, positionExplosion, Quaternion.identity);

        PropagerExplosion(Vector3.forward);
        PropagerExplosion(Vector3.back);
        PropagerExplosion(Vector3.left);
        PropagerExplosion(Vector3.right);

        Destroy(gameObject);
    }

    void OnTriggerExit(Collider autre)
    {
        if (autre.CompareTag("Player"))
        {
            if (monCollider != null)
            {
                monCollider.isTrigger = false;
            }
        }
    }

   void PropagerExplosion(Vector3 direction)
    {
        // On utilise la boucle avec notre variable porteeExplosion
        for (int i = 1; i <= porteeExplosion; i++)
        {
            Vector3 positionAVerifier = transform.position + (direction * i);
            Vector3 positionExplosion = positionAVerifier + new Vector3(0, 0.5f, 0);

            Collider[] objetsTouches = Physics.OverlapSphere(positionAVerifier, 0.4f, calqueMurs);

            if (objetsTouches.Length > 0)
            {
                Collider objet = objetsTouches[0];
                MurDestructible mur = objet.GetComponent<MurDestructible>();
                
                if (mur != null)
                {
                    mur.Detruire();
                    Instantiate(prefabExplosion, positionExplosion, Quaternion.identity);
                }
                
                break; 
            }
            else
            {
                Instantiate(prefabExplosion, positionExplosion, Quaternion.identity);
            }
        }
    }
}