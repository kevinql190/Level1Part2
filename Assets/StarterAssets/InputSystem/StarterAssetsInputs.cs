using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Control Scheme")]
		public bool useArrowKeys = false; // NEW

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
		public void OnMove(InputValue value)
		{
			// Only use Input System movement if NOT using arrow keys
			if (!useArrowKeys)
			{
				MoveInput(value.Get<Vector2>());
			}
		}

		public void OnLook(InputValue value)
		{
			if (cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}
#endif

		private void Update()
		{
#if ENABLE_INPUT_SYSTEM
			if (useArrowKeys)
			{
				ReadArrowKeys();
			}
#endif
		}

#if ENABLE_INPUT_SYSTEM
		private void ReadArrowKeys()
		{
			if (Keyboard.current == null)
				return;

			Vector2 input = Vector2.zero;

			if (Keyboard.current.leftArrowKey.isPressed)
				input.x -= 1f;
			if (Keyboard.current.rightArrowKey.isPressed)
				input.x += 1f;
			if (Keyboard.current.upArrowKey.isPressed)
				input.y += 1f;
			if (Keyboard.current.downArrowKey.isPressed)
				input.y -= 1f;

			MoveInput(input.normalized);

      if(Keyboard.current.rightShiftKey.isPressed)
      {
          SprintInput(true);
      }
      else
      {
          SprintInput(false);
      }
		}
#endif

		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		}

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
}
