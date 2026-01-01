using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [Header("Menu Settings")]
    [SerializeField] private GameObject menuPanel; 
    [SerializeField] private Button closeButton;   
    [SerializeField] private Button continueButton; 

    [Header("Player References")]
    [SerializeField] private PlayerMove playerMove; 
    [SerializeField] private Gun gun;               

    private bool isMenuOpen = false;

    private void Start()
    {
        if (menuPanel == null)
        {
            Debug.LogError("MenuPanel is not assigned in MenuController!");
        }
        if (playerMove == null)
        {
            Debug.LogError("PlayerMove is not assigned in MenuController!");
        }
        if (gun == null)
        {
            Debug.LogWarning("Gun is not assigned in MenuController. Shooting will not be disabled in menu.");
        }
        if (closeButton == null)
        {
            Debug.LogWarning("CloseButton is not assigned in MenuController!");
        }
        if (continueButton == null)
        {
            Debug.LogWarning("ContinueButton is not assigned in MenuController!");
        }

        
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }

        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseMenu);
        }
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(CloseMenu);
        }

        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isMenuOpen)
            {
                CloseMenu();
            }
            else
            {
                OpenMenu();
            }
        }
    }

    private void OpenMenu()
    {
        if (menuPanel == null)
        {
            Debug.LogError("Cannot open menu: MenuPanel is not assigned!");
            return;
        }
        if (playerMove == null)
        {
            Debug.LogError("Cannot open menu: PlayerMove is not assigned!");
            return;
        }

        
        menuPanel.SetActive(true);
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        Time.timeScale = 0f;
        
        playerMove.enabled = false;
        
        if (gun != null)
        {
            gun.enabled = false;
        }
        isMenuOpen = true;
    }

    private void CloseMenu()
    {
        if (menuPanel == null)
        {
            Debug.LogError("Cannot close menu: MenuPanel is not assigned!");
            return;
        }
        if (playerMove == null)
        {
            Debug.LogError("Cannot close menu: PlayerMove is not assigned!");
            return;
        }

        
        menuPanel.SetActive(false);
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        Time.timeScale = 1f;
        
        playerMove.enabled = true;
        
        if (gun != null)
        {
            gun.enabled = true;
        }
        isMenuOpen = false;
    }
}

