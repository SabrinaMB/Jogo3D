using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Prime31;

public class PlayerController : MonoBehaviour
{
    public GameObject maincamera;
    public CharacterController2D.CharacterCollisionState2D flags;
    public float walkSpeed = 4.0f;     // Depois de incluido, alterar no Unity Editor
    public float jumpSpeed = 8.0f;     // Depois de incluido, alterar no Unity Editor
    public float doubleJumpSpeed = 6.0f; //Depois de incluido, alterar no Editor
    public float gravity = 9.8f;       // Depois de incluido, alterar no Unity Editor

    public bool doubleJumped; // informa se foi feito um pulo duplo

    public bool isDucking;
    public AudioClip coin;

    public LayerMask mask;  // para filtrar os layers a serem analisados

    public bool isGrounded;     // Se está no chão
    public bool isJumping;      // Se está pulando
    public bool isFalling;      // Se estiver caindo
    public bool isFacingRight;      // Se está olhando para a direita
    private Vector3 moveDirection = Vector3.zero; // direção que o personagem se move
    private CharacterController2D characterController;	//Componente do Char. Controller

    private BoxCollider2D boxCollider;
    private float colliderSizeY;
    private float colliderOffsetY;
    private Animator animator;

    void Start()
    {
        characterController = GetComponent<CharacterController2D>(); //identif. o componente
        animator = GetComponent<Animator>();
        isFacingRight = true;
        boxCollider = GetComponent<BoxCollider2D>();
        colliderSizeY = boxCollider.size.y;
        colliderOffsetY = boxCollider.offset.y;
    }

    void Update()
    {
        moveDirection.x = Input.GetAxis("Horizontal"); // recupera valor dos controles
        moveDirection.x *= walkSpeed;

        if (moveDirection.x < 0)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
            isFacingRight = false;
        }
        else
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
            isFacingRight = true;
        }



        moveDirection.y -= gravity * Time.deltaTime;    // aplica a gravidade
        characterController.move(moveDirection * Time.deltaTime);   // move personagem	
        flags = characterController.collisionState;     // recupera flags
        isGrounded = flags.below;               // define flag de chão

        
        if (isGrounded)
        {               // caso esteja no chão
            moveDirection.y = 0.0f;
            isJumping = false;
            doubleJumped = false; // se voltou ao chão pode faz pulo duplo

            if (Input.GetButtonUp("Jump") && !doubleJumped) // Segundo clique faz pulo duplo
            {
                isJumping = true;
                if (Input.GetButtonUp("Jump") && isJumping)
                {
                    moveDirection.y = doubleJumpSpeed;
                    doubleJumped = true;
                }
                
            }
            else
            {            // caso esteja pulando 
                if (Input.GetButtonUp("Jump") && moveDirection.y > 0) // Soltando botão diminui pulo
                                                                      //isJumping = true;
                    moveDirection.y *= 0.5f;
            }

        }
        

        if (Input.GetAxis("Vertical") < 0)// && moveDirection.x == 0)
        {
            if (!isDucking)
            {
                boxCollider.size = new Vector2(boxCollider.size.x, 2 * colliderSizeY / 3);
                boxCollider.offset = new Vector2(boxCollider.offset.x, colliderOffsetY - colliderSizeY / 6);
                characterController.recalculateDistanceBetweenRays();
            }
            isDucking = true;
        }
        else
        {
            if (isDucking)
            {
                boxCollider.size = new Vector2(boxCollider.size.x, colliderSizeY);
                boxCollider.offset = new Vector2(boxCollider.offset.x, colliderOffsetY);
                characterController.recalculateDistanceBetweenRays();
                isDucking = false;
            }
        }



        RaycastHit2D hit = Physics2D.Raycast(transform.position, -Vector2.up, 4f, mask);
        gameObject.transform.SetParent(hit.transform);
        if (hit.collider != null && isGrounded)
        {
            
            if (Input.GetAxis("Vertical") < 0 && Input.GetButtonDown("Jump"))
            {
                moveDirection.y = -jumpSpeed;
                StartCoroutine(PassPlatform(hit.transform.gameObject));
            }
            //else
            //{
              //  transform.SetParent(null);
            //}

        }


        if (moveDirection.y < -0f)
        {
            isFalling = true;
        }
        else
        {
            isFalling = false;
        }

        IEnumerator PassPlatform(GameObject platform)
        {
            platform.GetComponent<Collider2D>().enabled = false;
            yield return new WaitForSeconds(0.5f);
            platform.GetComponent<Collider2D>().enabled = true;
        }

        animator.SetFloat("movementX", Mathf.Abs(moveDirection.x / walkSpeed)); // +Normalizado
        animator.SetFloat("movementY", moveDirection.y);
        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isJumping", isJumping);
        animator.SetBool("isDucking", isDucking);
        animator.SetBool("isFalling", isFalling);
        animator.SetBool("doubleJumped", doubleJumped);




    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Diamonds"))
        {
            //if (other.gameObject.tag == "diamond")
            //{
            //  other.gameObject.GetComponent<Collider2D>().enabled = false;
            //other.gameObject.GetComponent<Renderer>().enabled = false;
            //}
            // Destroy(other.gameObject);
            other.gameObject.transform.position = new Vector3(-100,-100,0);
            AudioSource.PlayClipAtPoint(coin, this.gameObject.transform.position);
            
        }

    }

}
