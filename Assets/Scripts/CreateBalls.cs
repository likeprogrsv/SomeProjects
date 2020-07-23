using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateBalls : MonoBehaviour
{
    
    public GameObject prefab;
    public GameObject[] ballsArr;

    public int N = 100;                 //nUMBER OF PARTICLES
    public float ballRadius = 0.2f;

    public float h = 0.2f;              // Particle radius (or smoothing radius?)
    private float h2, h4, h8;           // Radius squared, radius to the 4, radius to the 8
       
    //Vector2d positions, velocities, accelerations of particles. X and Y coordinate
    private Vector2[] position; 
    private Vector2[] velocity;
    private Vector2[] velocityHalf;
    private Vector2[] acceleration;

    // Resistance to compression
    // speed of sound = sqrt(k / rho0)
    private int k = 30;                 // Bulk modulus (1000)   k = 30

    private float gravity = -9.8f;
    private float mu = 3;               //viscosity (0.1)
    private float[] rho;                //Density
    private float rho0 = 1000;          // Reference density    
    private float rho02;                // Reference density times 2
    private float mass;                 //particle mass    
    private float dt = 0.0008f;  //18e-4f
    private float dt2;
    public float restitution = 0.95f;  // Coefficient of restitution for boundaries

    private float Cp;
    private float Cv;
    private float C0, C1, C2;

    //Boundaries
    private float edge1;
    private float edge2;
    private float edge3;

    private Camera cam;



    public void initialiseArray()
    {
        position = new Vector2[N];
        velocity = new Vector2[N];
        velocityHalf = new Vector2[N];
        acceleration = new Vector2[N];
        rho = new float[N];        
    }

    public void computeDependentVariables()
    {
        h2 = h * h;             // Radius squared
        h4 = h2 * h2;           // Radius to the 4
        h8 = h4 * h4;           // Radius to the 8
        rho02 = rho0 * 2;       // Reference density times 2
        Cp = 15 * k;            // a part of acceleration formula calculation
        Cv = -40 * mu;          // a part of acceleration formula calculation   
        dt2 = dt / 2;
        edge1 = ballRadius/2 + (-4.77f);   //-+4.77 is a left and right wall coordinates of x-axis
        edge2 = 4.77f - ballRadius/2;
        edge3 = ballRadius/2 + (-3.48f);         // floor coordinate of y-axis
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
        float yp = ballRadius * 0.5f + 0.01f;
        float xp = y1;
        float r = ballRadius;

        for (int i=0; i<N; i++)
        {
            //Initialize particle positions
            position[i].x = xp;
            position[i].y = yp;
            xp += r;            

            if (xp > y2)
            {
                xp = y1;
                yp += r;
            }

            velocity[i].x = Random.Range(-0.02f, 0.02f);
            velocity[i].y = Random.Range(-0.02f, 0.02f);            
        }
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

        for (int i = N - 1; i >= 0; i--)
        {
            rho2s += rho[i] * rho[i];
            rhos += rho[i];
        }




        /*
        for (int i = N - 1; i <= 0; i--)
        {
            rho2s += rho[i] * rho[i];
            rhos += rho[i];
            Debug.Log(rho2s);
        }
            */
            mass = rho0 * rhos / rho2s;
        // Constants for interaction term
        C0 = mass / (Mathf.PI * h4);
        C1 = 4 * mass / (Mathf.PI * h2);
        C2 = 4 * mass / (Mathf.PI * h8);
        
    }

    public void computeAccelerations()
    {
        computeDensities();

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
                    /*
                    Debug.Log(rhoi);
                    Debug.Log(rhoj);
                    Debug.Log(C0);
                    Debug.Log(u);
                    Debug.Log(w0);
                    Debug.Log(acceleration[i]);
                    Debug.Log(acceleration[j]);
                    Debug.Log("------------------------");*/
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
            velocity[i].x = acceleration[i].x * dt2;        //velocity[i].x += acceleration[i].x * dt;
            velocity[i].y = acceleration[i].y * dt2;        //velocity[i].x += acceleration[i].x * dt;

            //Update position
            position[i].x += velocityHalf[i].x * dt;
            position[i].y += velocityHalf[i].y * dt;
        }
    }

    public void leapfrogStep()
    {
        for (int i = N - 1; i >= 0; i--)
        {
            // Update half step velocity
            velocityHalf[i].x += acceleration[i].x * dt;
            velocityHalf[i].y += acceleration[i].y * dt;

            // Update velocity
            velocity[i].x = velocityHalf[i].x + acceleration[i].x * dt2;
            velocity[i].y = velocityHalf[i].y + acceleration[i].y * dt2;

            // Update position
            position[i].x += velocityHalf[i].x * dt;
            position[i].y += velocityHalf[i].y * dt;

            //Debug.Log(acceleration[i]);                                                     ///////////////////////////////////

            //Handle boundaries
            if (position[i].x < edge1)
            {
                position[i].x = edge1;
                velocity[i].x *= -restitution;
                velocityHalf[i].x *= -restitution;                
            }
            else if (position[i].x > edge2)
            {
                position[i].x = edge2;
                velocity[i].x *= -restitution;
                velocityHalf[i].x *= -restitution;
            }
            if (position[i].y < edge3)
            {
                position[i].y = edge3 + Random.Range(0.0001f, 0.0005f);
                velocity[i].y *= -restitution;
                velocityHalf[i].y *= -restitution;
            }
        }
    }

    public void updateParticles()
    {
        //float[, , , ,] collisions = new float[N, N, N, N, N];
        List<float[]> collisionsList = new List<float[]>();
        float[][] collisions;
        float dx, dy, r2;

        //Reset properties and find collisions
        for (int i = N - 1; i >= 0; i--)
        {
            //Reset densities
            rho[i] = C1;

            //Reset accelerations
            acceleration[i].x = 0;
            acceleration[i].y = gravity;

            //Calculate which particles overlap
            for (int j = i - 1; j >= 0; j--)
            {
                dx = position[i].x - position[j].x;
                dy = position[i].y - position[j].y;
                r2 = dx * dx + dy * dy;
                if (r2 < h2)
                {
                    float[] temp = new float[5] { i, j, dx, dy, r2 };
                    collisionsList.Add(temp);
                }
            }
        }

        collisions = collisionsList.ToArray();              //Transform List to float array

        
        //Calculate densities
        float rho_ij, z;
        for (int i = collisions.Length - 1; i >= 0; i--)
        {
            z = h2 - collisions[i][4];
            rho_ij = C2 * z * z * z;
            rho[(int)collisions[i][0]] += rho_ij;
            rho[(int)collisions[i][1]] += rho_ij;
        }

        //TODO: Find max density

        //Calculate accelerations

        int pi, pj;
        float q, u, w0, wp, wv, dvx, dvy;

        for (int i = collisions.Length - 1; i >= 0; i--)
        {
            pi = (int)collisions[i][0];
            pj = (int)collisions[i][1];

            q = Mathf.Sqrt(collisions[i][4]) / h;
            u = 1 - q;
            w0 = C0 * u / (rho[pi] * rho[pj]);
            wp = w0 * Cp * (rho[pi] + rho[pj] - rho02) * u / q;
            wv = w0 * Cv;

            dvx = velocity[pi].x - velocity[pj].x;
            dvy = velocity[pi].y - velocity[pj].y;

            acceleration[pi].x += wp * collisions[i][2] + wv * dvx;
            acceleration[pi].y += wp * collisions[i][3] + wv * dvy;
            acceleration[pj].x -= wp * collisions[i][2] + wv * dvx;
            acceleration[pj].y -= wp * collisions[i][3] + wv * dvy;

        }


    }

    public void createBalls()
    {
        
        ballsArr = new GameObject[N];
        for (int i = N - 1; i >= 0; i--)
        {
            GameObject balls = Instantiate(prefab, new Vector3(position[i].x, position[i].y, 0), Quaternion.identity) as GameObject;
            ballSize(balls);    
            //go.transform.localScale = Vector3.one;
            ballsArr[i] = balls;
        }
    }

    
    public void ballSize(GameObject currentBall)
    {
        RectTransform rt = currentBall.GetComponent<RectTransform>();
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ballRadius);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ballRadius);
    }

    public void initialiseSystem()
    {
        initialiseArray();
        particlesInMesh(-4.2f, ballRadius);  //worldPos.y/worldPos.x - 0.01f     
        createBalls();
        computeDependentVariables();
        normalizeMass();
        computeAccelerations();
        leapfrogInit();

        /*  //преобразование координат
        cam = Camera.main;
        Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3((float)Screen.width, (float)Screen.height, 0));
        */
    }

    public void Clear()
    {
        foreach (GameObject ball in ballsArr)
        {
            GameObject.Destroy(ball.gameObject);
        }
        initialiseSystem();

    }


    void Start()
    {
        initialiseSystem();
    }

    // Update is called once per frame
    void Update()
    {
        updateParticles();
        leapfrogStep();
        



        //Update balls positions 
        for (int i = N - 1; i >=0; i--)
        {
            ballsArr[i].transform.position = position[i];
        }
        
        //create balls by clicking mouse
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
