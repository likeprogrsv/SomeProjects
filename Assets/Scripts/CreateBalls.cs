using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateBalls : MonoBehaviour
{
    
    public GameObject prefab;
    public GameObject[] ballsArr;

    public int N = 100;                 //nUMBER OF PARTICLES

    public float h = 0.2f;              // Particle radius
    private float h2, h4, h8;           // Radius squared, radius to the 4, radius to the 8
       
    //Vector2d positions, velocities, accelerations of particles. X and Y coordinate
    private Vector2[] position; 
    private Vector2[] velocity;
    private Vector2[] velocityHalf;
    private Vector2[] acceleration;

    // Resistance to compression
    // speed of sound = sqrt(k / rho0)
    private int k = 30;                 // Bulk modulus (1000)

    private float gravity = -9.8f;
    private float mu = 3;               //viscosity (0.1)
    private float[] rho;                //Density
    private float rho0 = 1000;          // Reference density    
    private float rho02;                // Reference density times 2
    private float mass;                 //particle mass    
    private float dt = 18e-4f;  //18e-4f
    private float dt2;

    private float Cp;
    private float Cv;
    private float C0, C1, C2;

    private Camera cam;



    public void initialiseArray()
    {
        position = new Vector2[N];
        velocity = new Vector2[N];
        velocityHalf = new Vector2[N];
        acceleration = new Vector2[N];
        rho = new float[N];        
    }
    
    public void randomInit()
    {
        for (int i=0; i < N; i++)
        {
            //Initialize particle positions
            position[i].x = Random.Range(h, 0.25f);
            position[i].y = Random.Range(0.5f, 0.95f);
        }
    }

    public void particlesInMesh(float y1, float y2)
    {
        float yp = h * 0.5f + 0.01f;
        float xp = y1;
        float r = h;

        for (int i=0; i<N; i++)
        {
            //Initialize particle positions
            position[i].x = xp;
            position[i].y = yp;
            xp += r;
            Debug.Log(position[i].y);               ///////////////

            if (xp > y2)
            {
                xp = y1;
                yp += r;
            }

            velocity[i].x = Random.Range(-0.02f, 0.02f);
            velocity[i].y = Random.Range(-0.02f, 0.02f);
        }

        
        //Debug.Log(cam.pixelWidth);
    }

    public void computeDependentVariables()
    {
        h2 = h * h;          // Radius squared
        h4 = h2 * h2;        // Radius to the 4
        h8 = h4 * h4;        // Radius to the 8
        rho02 = rho0 * 2;    // Reference density times 2
        Cp = 15 * k;         // a part of acceleration formula calculation
        Cv = -40 * mu;       // a part of acceleration formula calculation   
        dt2 = dt / 2;
    }

    public void computeDensities()
    {
        // Find new densities        
        float dx, dy, r2, z, rho_ij;
        float C1 = 4 * mass / (Mathf.PI * h2);  
        float C2 = 4 * mass / (Mathf.PI * h8); 

        //Initialise densities
        for (int i = N-1; i >= 0; i--)      // N - number of particles, i - index of "rho" array (index begins from zero),    
        {                                   //  that's why i=N-1 
            rho[i] = C1;
        }

        //this forloop below iterate through whole set of particles. It checks the distance between i and j particle.
        //If the difference between radius squared (h2) and Hypotenuse squared? (r2) more than zero,
        //then calculates new density.
        for (int i = 0; i < N; i++)
        {
            for (int j = i + 1; j < N; j++)
            {
                dx = position[i].x - position[j].x;
                dy = position[i].y - position[j].y;
                r2 = dx * dx + dy * dy; //Hypotenuse squared?? r = position[i] - position[j]
                z = h2 - r2;

                if (z > 0)
                {
                    rho_ij = C2 * z * z * z;
                    rho[i] += rho_ij;
                    rho[j] += rho_ij;
                }
            }
        }

    }

    public void normalizeMass()
    {
        mass = 1;
        computeDensities();

        float rho2s = 0;
        float rhos = 0;
        for (int i = N - 1; i <= 0; i--)
        {
            rho2s += rho[i] * rho[i];
            rhos += rho[i];
        }

        mass = rho0 * rhos / rho2s;
        // Constants for interaction term
        C0 = mass / (Mathf.PI * h4);
        C1 = 4 * mass / (Mathf.PI * h2);
        C2 = 4 * mass / (Mathf.PI * h8);
    }

    public void computeAccelerations()
    {
        //Start with gravity and surface forces
        for (int i = N - 1; i >= 0; i--)
        {
            acceleration[i].x = 0;
            acceleration[i].y = gravity;                    
        }                                       

        //Find new densities
        float dx, dy, r2, rhoi, rhoj, q, u, w0, wp, wv, dvx, dvy;

        for (int i = N - 1; i >= 0; i--)
        {
            rhoi = rho[i];  
            for ( int j = i - 1; j >= 0; j--)
            {
                dx = position[i].x - position[j].x;
                dy = position[i].y - position[j].y;
                r2 = dx * dx + dy * dy;     

                if (r2 < h2)
                {
                    rhoj = rho[j];
                    q = Mathf.Sqrt(r2) / h;     //does q is the distance between i and j particles divided by the smoothing length h(or radius) ???
                    u = 1 - q;
                    w0 = C0 * u / (rhoi * rhoj);
                    wp = w0 * Cp * (rhoi + rhoj - rho02) * u / q;
                    wv = w0 * Cv;

                    dvx = velocity[i].x - velocity[j].x;
                    dvy = velocity[i].y - velocity[j].y;

                    acceleration[i].x += wp * dx + wv * dvx;
                    acceleration[i].y += wp * dy + wv * dvy;
                    acceleration[j].x -= wp * dx + wv * dvx;
                    acceleration[j].y -= wp * dy + wv * dvy;
                    
                }
            }
        }               
    }

    public void leapfrogInit()
    {
        for (int i = N - 1; i >= 0; i--)
        {
            //Update half step velocity
            velocityHalf[i].x = velocity[i].x + acceleration[i].x * dt2;
            velocityHalf[i].y = velocity[i].y + acceleration[i].y * dt2;

            //Update velocity
            velocity[i].x += acceleration[i].x * dt;
            velocity[i].y += acceleration[i].y * dt;

            //Update position
            position[i].x += velocityHalf[i].x * dt;
            position[i].y += velocityHalf[i].y * dt;

            Debug.Log(position[i].y);               ///////////////
            //Debug.Log(acceleration[i].y);
        }
    }


    public void createBalls()
    {
        ballsArr = new GameObject[N];
        for (int i=0; i<N; i++)
        {
            GameObject balls = Instantiate(prefab, new Vector3(position[i].x, position[i].y, 0), Quaternion.identity);  //as GameObject;
            //go.transform.localScale = Vector3.one;
            ballsArr[i] = balls;
        }
    }


    void Start()
    {
        /*  //преобразование координат
        cam = Camera.main;
        Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3((float)Screen.width, (float)Screen.height, 0));
        */

        initialiseArray();
        particlesInMesh(0.05f, h * 10f);  //worldPos.y/worldPos.x - 0.01f        
        computeDependentVariables();
        normalizeMass();
        computeAccelerations();
        leapfrogInit();
        createBalls();
        

        
    }

    // Update is called once per frame
    void Update()
    {
        leapfrogInit();                     // Temporarily
        //create balls by clicking mouse

        //Update balls positions
        int i = 0;
        foreach (GameObject ball in ballsArr)
        {
            ball.transform.position = position[i];
            i++;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

            if (hit.collider != null)
            {
                GameObject mouseBall = Instantiate(prefab, new Vector3(worldPoint.x, worldPoint.y, 0), Quaternion.identity);
               /*
                Debug.Log(hit.collider.name);
                Debug.Log("X " + Input.mousePosition.x + ";" + "Y " + Input.mousePosition.y);
                Debug.Log("X " + worldPoint.x + ";" + "Y " + worldPoint.y);
                */
            }
        }
        
    }
}
