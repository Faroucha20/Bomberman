using UnityEngine;
using System.Collections;

public class Monstre : MonoBehaviour
{
    [Header("Paramètres du Monstre")]
    public float vitesse = 1f;
    public LayerMask calqueMurs; 
    
    [Header("Capacités Spéciales")]
    public bool peutDetruireMurs = false;
    public bool peutTraverserMursDestructibles = false; 
    
    public bool mouvementAleatoire = false; 
    public float tempsAvantChangement = 3f; 
    private float chronoChangement = 0f;    

    [Header("Capacité : Poseur de Bombes")]
    public bool peutPoserBombes = false;
    public GameObject prefabBombe; 
    public float tempsMiniEntreBombes = 4f; 
    public float tempsMaxiEntreBombes = 8f; 
    private float chronoBombe = 0f;
    private float prochainObjectifTemps = 0f;

    [Header("Paramètres d'Animation")]
    public float tempsAttaque = 2f; 

    private Vector3 directionActuelle;
    private Animator anim;
    private bool estEnTrainDAttaquer = false; 

    void Start()
    {
        anim = GetComponent<Animator>();
        ChoisirNouvelleDirection();

        if (peutPoserBombes)
        {
            prochainObjectifTemps = Random.Range(tempsMiniEntreBombes, tempsMaxiEntreBombes);
        }
    }

    void Update()
    {
        if (estEnTrainDAttaquer) return;
        
        // Si le monstre est complètement bloqué (direction zéro), il cherche une issue
        if (directionActuelle == Vector3.zero)
        {
            ChoisirNouvelleDirection();
            // S'il est toujours bloqué après avoir cherché, on arrête là pour cette frame
            if (directionActuelle == Vector3.zero) return; 
        }


        RaycastHit objetTouche; 

        if (ObstacleDevant(directionActuelle, out objetTouche))
        {
            MurDestructible mur = objetTouche.collider.GetComponent<MurDestructible>();

            if (mur != null && peutDetruireMurs)
            {
                StartCoroutine(SequenceAttaqueMur(mur));
            }
            else
            {
                ChoisirNouvelleDirection();
                chronoChangement = 0f; 
            }
        }
        else
        {
            transform.Translate(directionActuelle * vitesse * Time.deltaTime, Space.World);

            if (directionActuelle != Vector3.zero)
            {
                transform.forward = directionActuelle;
            }

            if (mouvementAleatoire)
            {
                chronoChangement += Time.deltaTime; 

                if (chronoChangement >= tempsAvantChangement)
                {
                    Vector3 centreCase = new Vector3(Mathf.Round(transform.position.x), transform.position.y, Mathf.Round(transform.position.z));
                    
                    if (Vector3.Distance(transform.position, centreCase) < 0.1f)
                    {
                        ChoisirNouvelleDirection();
                        chronoChangement = 0f; 
                    }
                }
            }

            // --- MISE À JOUR : LANCER LA BOMBE DEVANT ---
            if (peutPoserBombes)
            {
                chronoBombe += Time.deltaTime;

                if (chronoBombe >= prochainObjectifTemps)
                {
                    Vector3 centreCaseActuelle = new Vector3(Mathf.Round(transform.position.x), 0f, Mathf.Round(transform.position.z));
                    
                    Vector2 positionMonstre2D = new Vector2(transform.position.x, transform.position.z);
                    Vector2 centreCase2D = new Vector2(centreCaseActuelle.x, centreCaseActuelle.z);

                    if (Vector2.Distance(positionMonstre2D, centreCase2D) < 0.1f)
                    {
                        // 1. On calcule les coordonnées de la case juste devant le monstre
                        Vector3 caseDevant = centreCaseActuelle + directionActuelle;
                        
                        // 2. On vérifie s'il y a un obstacle (mur) sur cette case
                        // On rehausse le point de vérification de 0.5f pour être bien au milieu du bloc
                        bool caseDevantOccupee = Physics.CheckSphere(caseDevant + Vector3.up * 0.5f, 0.4f, calqueMurs);
                        
                        // 3. On choisit la destination finale de la bombe
                        Vector3 positionFinaleBombe;
                        if (caseDevantOccupee)
                        {
                            positionFinaleBombe = centreCaseActuelle; // La case devant est prise, on pose la bombe aux pieds
                        }
                        else
                        {
                            positionFinaleBombe = caseDevant; // La voie est libre, on la lance devant !
                        }

                        if (prefabBombe != null)
                        {
                            StartCoroutine(SequencePoserBombe(positionFinaleBombe));
                        }

                        chronoBombe = 0f;
                        prochainObjectifTemps = Random.Range(tempsMiniEntreBombes, tempsMaxiEntreBombes);
                    }
                }
            }
        }
    }

    bool ObstacleDevant(Vector3 directionAVerifier, out RaycastHit infosTouche)
    {
        Vector3 origineLaser = transform.position + Vector3.up * 0.5f;
        float distanceVerif = 0.6f;
        
        bool touche = Physics.Raycast(origineLaser, directionAVerifier, out infosTouche, distanceVerif, calqueMurs);
        
        if (touche && peutTraverserMursDestructibles)
        {
            MurDestructible mur = infosTouche.collider.GetComponent<MurDestructible>();
            if (mur != null)
            {
                return false; 
            }
        }
        
        return touche;
    }

    IEnumerator SequenceAttaqueMur(MurDestructible murADetruire)
    {
        estEnTrainDAttaquer = true; 

        if (anim != null) anim.SetTrigger("Attaque");

        yield return new WaitForSeconds(tempsAttaque);

        if (murADetruire != null)
        {
            murADetruire.Detruire(); 
        }

        ChoisirNouvelleDirection();
        estEnTrainDAttaquer = false;
    }

    IEnumerator SequencePoserBombe(Vector3 positionDeLaBombe)
    {
        estEnTrainDAttaquer = true; 

        if (anim != null) anim.SetTrigger("Attaque");

        yield return new WaitForSeconds(tempsAttaque);

        positionDeLaBombe.y = -0.75f;

        // 1. On mémorise la bombe qu'on vient de créer
        GameObject laBombe = Instantiate(prefabBombe, positionDeLaBombe, Quaternion.identity);

        // 2. On récupère son script et on force sa portée à 1 !
        Bombe scriptBombe = laBombe.GetComponent<Bombe>();
        if (scriptBombe != null)
        {
            // --- C'EST ICI LA CORRECTION ---
            // On utilise bien le nouveau nom : porteeExplosion
            scriptBombe.porteeExplosion = 1;
        }

        estEnTrainDAttaquer = false;
    }

    void ChoisirNouvelleDirection()
    {
        float xArrondi = Mathf.Round(transform.position.x);
        float zArrondi = Mathf.Round(transform.position.z);
        transform.position = new Vector3(xArrondi, transform.position.y, zArrondi);

        Vector3[] directionsPossibles = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };

        bool directionTrouvee = false;
        int tentatives = 0;

        while (!directionTrouvee && tentatives < 10)
        {
            int indexHasard = Random.Range(0, directionsPossibles.Length);
            Vector3 directionTest = directionsPossibles[indexHasard];

            RaycastHit dummyHit; 
            if (!ObstacleDevant(directionTest, out dummyHit))
            {
                directionActuelle = directionTest;
                directionTrouvee = true;
            }

            tentatives++;
        }

        if (!directionTrouvee)
        {
            directionActuelle = Vector3.zero;
        }
    }
}