using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateBalls : MonoBehaviour
{
    
    public GameObject prefab;

    public int N = 100; //nUMBER OF PARTICLES
    public float h = 0.2f; // Particle Radius

    private float x; //x coordinate
    private float y; //y coordinate
    private Vector2[] position; //vector2d positions of particles
    private Camera cam;

    public void initialiseArray()
    {
        position = new Vector2[N];
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

            if (xp > y2)
            {
                xp = y1;
                yp += r;
            }
        }
        //Debug.Log(cam.pixelHeight);
        //Debug.Log(cam.pixelWidth);
    }


    public void createBalls()
    {
        for (int i=0; i<N; i++)
        {
            GameObject balls = Instantiate(prefab, new Vector3(position[i].x, position[i].y, 0), Quaternion.identity);
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
        createBalls();
        
    }

    // Update is called once per frame
    void Update()
    {






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
