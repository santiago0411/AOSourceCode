using AOClient.Core;
using AOClient.Network;
using UnityEngine;
using AOClient.UI;
#if AO_DEBUG
using IngameDebugConsole;
#endif
namespace AOClient.Player
{
    public class PlayerController : MonoBehaviour
    {
        private float clickTime;
        private GameObject chatbox;
        private Camera mainCamera;
        
        private const float DOUBLE_CLICK_TIME = 0.3f;

        private void Start()
        {
            chatbox = UIManager.GameUI.Console.Chatbox;
            mainCamera = Camera.main;
        }

        private void Update()
        {
            #if AO_DEBUG
            if (DebugLogManager.Instance.PopupEnabled)
                return;
            #endif
            
            if (UIManager.GameUI.MailWindow.IsOpen)
                return;
            
            // TODO REMOVE TEMP
            if (Input.GetKeyDown(KeyCode.F10))
                UIManager.GameUI.MailWindow.Show();
            
            UseItem();
            Chat();
            LeftClick();
            GrabItem();
            DropItem();
            EquipItem();
            Attack();
            TameNpc();
            ChangePetTarget();
            FKeys();
        }

        private void FixedUpdate()
        {   
            MovementInput();
        }

        private void TameNpc()
        {
            if (!chatbox.activeSelf && Input.GetKeyDown(KeyCode.D))
                GameManager.Instance.LocalPlayer.SetClickRequest(ClickRequest.TameAnimal);
        }

        private void ChangePetTarget()
        {
            if (!chatbox.activeSelf && Input.GetKeyDown(KeyCode.H))
                GameManager.Instance.LocalPlayer.SetClickRequest(ClickRequest.PetChangeTarget);
        }

        private static void FKeys()
        {
            if (Input.GetKeyDown(KeyCode.F4))
                PacketSender.PlayerInput(PlayerInput.Exit);

            if (Input.GetKeyDown(KeyCode.F6))
                PacketSender.PlayerInput(PlayerInput.Meditate);
            
            if (Input.GetKeyDown(KeyCode.F7))
                UIManager.GameUI.PartyWindow.ShowWindow();
        }

        private void UseItem()
        {
            if (!chatbox.activeSelf && Input.GetKeyDown(KeyCode.U))
            {
                byte selectedSlotId = UIManager.GameUI.InventoryUI.SelectedInventorySlot.SlotId;
                PacketSender.PlayerItemAction(selectedSlotId, true);
            }
        }

        private void Chat()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (chatbox.activeSelf)
                {
                    string message = UIManager.GameUI.Console.ChatboxField.text;
                    //Check for empty message to not send an empty packet
                    if (!string.IsNullOrEmpty(message))
                    {
                        PacketSender.PlayerChat(message);
                    }
                    UIManager.GameUI.Console.Chatbox.SetActive(false);
                    UIManager.GameUI.Console.ChatboxField.text = string.Empty;
                }
                else
                {
                    UIManager.GameUI.Console.Chatbox.SetActive(true);
                    UIManager.GameUI.Console.ChatboxField.Select();
                    UIManager.GameUI.Console.ChatboxField.ActivateInputField();
                }
            }
        }

        private void LeftClick()
        {
            if (Input.GetMouseButtonUp(0))
            {
                bool doubleClick = (Time.realtimeSinceStartup - clickTime) < DOUBLE_CLICK_TIME;

                if (RectTransformUtility.RectangleContainsScreenPoint(UIManager.GameUI.CameraTransform, Input.mousePosition)) // Check that it's within the game area
                {
                    Vector2 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition); // Convert the click position to world position if it is
                    var localPlayer = Core.GameManager.Instance.LocalPlayer;

                    if (localPlayer.ClickRequest != ClickRequest.NoRequest) // Whether to send a click request packet or normal click
                        PacketSender.PlayerLeftClickRequest(worldPos, localPlayer.ClickRequest);
                    else
                        PacketSender.PlayerLeftClick(worldPos, doubleClick);

                    UIManager.ChangeCursor(null); // Set cursor back to default
                    localPlayer.ClickRequest = ClickRequest.NoRequest;
                }

                clickTime = Time.realtimeSinceStartup; // Save last click time to check for double click
            }
        }

        private void GrabItem()
        {
            if (!chatbox.activeSelf && Input.GetKeyDown(KeyCode.A))
                PacketSender.PlayerInput(PlayerInput.GrabItem);
        }

        private void DropItem()
        {
            if (!chatbox.activeSelf && Input.GetKeyDown(KeyCode.T))
            {
                UIManager.GameUI.DropItems.DragAndDrop = false;
                UIManager.GameUI.DropItems.Show();
            }
        }

        private void EquipItem()
        {
            if (!chatbox.activeSelf && Input.GetKeyDown(KeyCode.E))
            {
                byte selectedSlotId = UIManager.GameUI.InventoryUI.SelectedInventorySlot.SlotId;
                PacketSender.PlayerItemAction(selectedSlotId, false);
            }
        }

        private static void Attack()
        {
            if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
                PacketSender.PlayerInput(PlayerInput.Attack);
        }

        private static void MovementInput()
        {
            var inputs = PlayerMovementInputs.Empty;

            if (Input.GetKey(KeyCode.UpArrow))
                inputs |= PlayerMovementInputs.MoveUp;

            if (Input.GetKey(KeyCode.DownArrow))
                inputs |= PlayerMovementInputs.MoveDown;

            if (Input.GetKey(KeyCode.LeftArrow))
                inputs |= PlayerMovementInputs.MoveLeft;

            if (Input.GetKey(KeyCode.RightArrow))
                inputs |= PlayerMovementInputs.MoveRight;

            if (inputs != PlayerMovementInputs.Empty)
                PacketSender.PlayerMovementInputs(inputs);
        }
    }
}
