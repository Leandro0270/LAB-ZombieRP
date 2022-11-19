using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Esse comando faz com que seja necessario o objeto em que o script for aplicado tenha o componente RIGIDBODY
[RequireComponent(typeof(Rigidbody))]
public class PlayerRotation : MonoBehaviour
{    
    private Rigidbody _rb;
    //Variavel que vai armazenar onde o raycast está batendo
    private RaycastHit _hit;
    //Variavel que vai definir onde que o raycast vai poder bater, fazendo com que tudo que não esteja nessa layer seja ignorado
    public LayerMask ground;
    void Start()
    {
        //_rb está recebendo o componente Rigidbody de onde o script está sendo aplicado
        _rb = GetComponent<Rigidbody>();
    }
    
    //Para uso de componentes envolvendo fisicas (Nesse caso o RigidBody) é recomendado utilizar o fixed update
    void FixedUpdate()
    {
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out _hit, 100, ground))
        {
            //Nessa variavel está sendo feito a distância do player para onde o raycast está batendo
            Vector3 playerToMouse = _hit.point - (transform.position);
            playerToMouse.y = 0;
            //Nessa variavel está sendo feito o calculo da rotação necessária para o player utilizando o lerp para suavizar
            Quaternion newRotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(playerToMouse), 0.2f);
            _rb.MoveRotation(newRotation);
        }
    }
}
