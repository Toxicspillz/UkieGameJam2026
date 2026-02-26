using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterInput : MonoBehaviour
{

    private CharacterInputMap m_CharacterInputMap;
    [SerializeField] CharacterMovement CharacterMovement;

    private void Awake()
    {
        CharacterMovement = GetComponent<CharacterMovement>();
        m_CharacterInputMap = new CharacterInputMap();
    }



    private void OnEnable()
    {
        m_CharacterInputMap.Enable();
        m_CharacterInputMap.Player.Move.performed += Handle_MovePerformed;

        m_CharacterInputMap.Player.Move.canceled += Handle_MoveCancelled;



    }


    private void OnDisable()
    {
        m_CharacterInputMap.Disable();
        m_CharacterInputMap.Player.Move.performed -= Handle_MovePerformed;
        m_CharacterInputMap.Player.Move.canceled -= Handle_MoveCancelled;

    }


    private void Handle_MovePerformed(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        CharacterMovement.SetMove(input);
    }


    private void Handle_MoveCancelled(InputAction.CallbackContext context)
    {
        CharacterMovement.SetMove(Vector2.zero);

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
