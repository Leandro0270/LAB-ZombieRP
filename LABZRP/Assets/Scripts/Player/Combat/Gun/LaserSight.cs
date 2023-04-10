using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserSight : MonoBehaviour
{
    
    public Color corLaser = Color.red;
    public int DistanciaDoLaser = 100;
    public float LarguraInicial = 0.02f, LarguraFinal = 0.1f;
    private GameObject luzColisao;
    public Material materialLaser;
    private LineRenderer lineRenderer;

    private Vector3 posicLuz;
    // Start is called before the first frame update
    void Start () {
        luzColisao = new GameObject ();
        luzColisao.AddComponent<Light> ();
        luzColisao.GetComponent<Light> ().intensity = 8;
        luzColisao.GetComponent<Light> ().bounceIntensity = 8;
        luzColisao.GetComponent<Light> ().range = LarguraFinal * 2;
        luzColisao.GetComponent<Light> ().color = corLaser;
        //
        lineRenderer = gameObject.AddComponent<LineRenderer> ();
        lineRenderer.material = materialLaser;
        lineRenderer.SetColors (corLaser, corLaser);
        lineRenderer.SetWidth (LarguraInicial, LarguraFinal);
        lineRenderer.SetVertexCount (2);
        posicLuz = new Vector3(0, 0, LarguraFinal);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 PontoFinalDoLaser = transform.position + (transform.forward * DistanciaDoLaser);
        RaycastHit PontoDeColisao;
        LayerMask layer = ~(1 << 2);

        if (Physics.Raycast(transform.position, transform.forward, out PontoDeColisao, DistanciaDoLaser, layer))
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, PontoDeColisao.point);
            luzColisao.transform.position = (PontoDeColisao.point - posicLuz);
        }
        else
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, PontoFinalDoLaser);
            luzColisao.transform.position = PontoFinalDoLaser;
        }
    }
}
