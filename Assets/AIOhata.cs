using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using Panda;

public class AIOhata : MonoBehaviour
{
    //Transform do Player para saber a position dele
    public Transform player;
    //O transform da Bala para spawnar a bala
    public Transform bulletSpawn;
    //o Slider da vida para diminuir
    public Slider healthBar;
    //o gameobject da Bala
    public GameObject bulletPrefab;
    //O componete NaveMeshAgente para fazer a logica de andar e tudo mais
    NavMeshAgent agent;
    public Vector3 destination; // The movement destination.
    public Vector3 target; // The position to aim to.
    //A vida do NPC
    float health = 100.0f;
    //A velocidade de Rotation
    float rotSpeed = 5.0f;
    //A distancia que o NPC ver 
    float visibleRange = 80.0f;
    // A distancia do Tiro
    float shotRange = 40.0f;
    void Start()
    {
        //Pegando o componente
        agent = this.GetComponent<NavMeshAgent>();
        agent.stoppingDistance = shotRange - 5; //for a little buffer
        //ChAMA esse metodo a cada  5 
        InvokeRepeating("UpdateHealth", 5, 0.5f);
    }
    void Update()
    {
        //Pegando a minha posição com a camera e guardando na HealtBarPos
        Vector3 healthBarPos = Camera.main.WorldToScreenPoint(this.transform.position);
        //dando o valor da vida no healt bar (Slider)
        healthBar.value = (int)health;
        //A position do Healt sera a position da Camera do Transform desse Npc + um OffSet que seria para deixar em cima
        healthBar.transform.position = healthBarPos + new Vector3(0, 60, 0);
    }
    void UpdateHealth()
    {
        //Quando a vida estiver a baixo de 100, eu curo 
        if (health < 100)
            health++;
    }

    [Task]
    public void PickRandomDestination()
    {
        //Pego um vector aleatorio para ser o meu destino
        Vector3 dest = new Vector3(Random.Range(-100, 100), 0, Random.Range(-100, 100));
        //Seto a destino para o navmeshAgent
        agent.SetDestination(dest);
        //Falo para o Component do Panda que a tarefa foi executada
        Task.current.Succeed();
    }
    [Task]
    public void MoveToDestination()
    {
        //Caso seja para parecer no Inspected, mostro a duration do movimento no Formato correto
        if (Task.isInspected)
            Task.current.debugInfo = string.Format("t={0:0.00}", Time.time);
            //Verfico as variaves do Agent para concluir a task,que serias as variavels do Agent, a distancia e a onde esta 
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            Task.current.Succeed();
        }
    }

    [Task]
    public void PickDestination(int x, int z)
    {
        //Recebo distancia do Eixo X e Z para ir para proximo destino
        Vector3 dest = new Vector3(x, 0, z);
        agent.SetDestination(dest);
        Task.current.Succeed();
    }

    //Pego a position do meu Player
    [Task]
    public void TargetPlayer()
    {
        target = player.transform.position;
        Task.current.Succeed();
    }

    [Task]
    public bool Fire()
    {
        //Instancio a bala
        GameObject bullet = GameObject.Instantiate(bulletPrefab,
        bulletSpawn.transform.position, bulletSpawn.transform.rotation);
        //ela acaba sendo arremesada para uma direction que seria a frente dela
        bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * 2000);
        return true;
    }
    [Task]
    public void LookAtTarget()
    {
        //A direction sera o target menos a minha position que sera o local que irei olhar
        Vector3 direction = target - this.transform.position;
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation,
        Quaternion.LookRotation(direction), Time.deltaTime * rotSpeed);
        if (Task.isInspected)
            Task.current.debugInfo = string.Format("angle={0}",
            Vector3.Angle(this.transform.forward, direction));
        if (Vector3.Angle(this.transform.forward, direction) < 5.0f)
        {
            Task.current.Succeed();
        }
    }

//O Metodo para perguntar se estou vendo o Player e tambem verificar se tem parede na frente, para trazer realiadade ao NPC
    [Task]
    bool SeePlayer()
    {
        Vector3 distance = player.transform.position - this.transform.position;
        RaycastHit hit;
        bool seeWall = false;
        Debug.DrawRay(this.transform.position, distance, Color.red);
        if (Physics.Raycast(this.transform.position, distance, out hit))
        {
            if (hit.collider.gameObject.tag == "wall")
            {
                seeWall = true;
            }
        }
        if (Task.isInspected)
            Task.current.debugInfo = string.Format("wall={0}", seeWall);
        if (distance.magnitude < visibleRange && !seeWall)
            return true;
        else
            return false;
    }

    [Task]
    bool Turn(float angle)
    {
        //Vire no angulo recebido 
        var p = this.transform.position + Quaternion.AngleAxis(angle, Vector3.up) *
        this.transform.forward;
        target = p;
        return true;
    }
}