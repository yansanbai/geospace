using UnityEngine;
using System.Collections.Generic;

/*
 * On Screen Keyboard
 * By Richard Taylor, Holopoint Interactive Pty. Ltd.
 * 
 * FEATURES:
 * - Fully configurable layout
 * - Fully skinnable
 * - Key select and press audio
 * - Configurable caps functionality
 * - Configurable key repeat settings
 * - Works with both joystick/gamepad and mouse/touchscreen input
 * - Simple integration
 * - Tested using Xbox 360 controller and iPad
 */


/*
 *  Time list:
 *      June于2020.04.17改
 *      
 */

public enum ShiftState { Off, Shift, CapsLock }

public class OnScreenKeyboard : MonoBehaviour
{

	// INSPECTOR VISIBLE PROPERTIES -------------------------------------------

	// Skinning
	public GUIStyle boardStyle;
	public GUIStyle keyStyle;
	public Texture2D selectionImage;

	// Board and button sizes
	public Rect screenRect = new Rect(0, 0, 0, 0);
	public Vector2 stdKeySize = new Vector2(32, 32);
	public Vector2 lgeKeySize = new Vector2(64, 32);

	// Key audio
	public AudioClip keySelectSound = null;
	public AudioClip keyPressSound = null;

	// Shift settings
	public bool shiftStateSwitchEnabled = true;
	public ShiftState shiftStateDefault = ShiftState.Off;

	// Joystick settings
	public bool joystickEnabled = true;
	public string joyPressButton = "Fire1";
	public string joyCapsButton = "Fire2";

	// Our keys. By default we'll include a simplified QWERTY keyboard handy 
	// for name entry, but this can literally be anything you want. Either the 
	// two arrays must be of matching length, or lowerKeys must be of size 0.
	public string[] upperKeys = { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P", "<<", "<row>",
									"A", "S", "D", "F", "G", "H", "J", "K", "L", "Done", "<row>",
									"Z", "X", "C", "V", "B", "N", "M", "Caps", "Space" };

	public string[] lowerKeys = { "q", "w", "e", "r", "t", "y", "u", "i", "o", "p", "<<", "<row>",
									"a", "s", "d", "f", "g", "h", "j", "k", "l", "Done", "<row>",
									"z", "x", "c", "v", "b", "n", "m", "Caps", "Space" };

	// The size must match the number of rows, or be 0
	public float[] rowIndents = { 0.0f, 0.2f, 0.5f };

	// Delays for repeated events
	public float initialRepeatDelay = 0.8f;
	public float continuedRepeatDelay = 0.2f;
	public float moveRepeatDelay = 0.3f;


	// INTERNAL DATA MEMBERS --------------------------------------------------
	private string keyPressed = "";
	private int pSelectedButton;

	private GUIStyle pressedStyle = null;

	private float keyRepeatTimer = 0;
	private bool keyDownPrevFrame = false;
	private bool keyReleased = false;
	private bool lastKeyWasShift = false;

	private float moveTimer = 0;

	private ShiftState shiftState;

	private bool[] keySizes;
	private Rect[] keyRects;
	private int[] rowMarkers;

	private int selectedButton;

	private AudioSource keySelectSource = null;
	private AudioSource keyPressSource = null;

	// Change this if it's conflicting with your own GUI's windows
	private int windowId = 0;


	/// <summary>
	/// 新增属性，控制虚拟键盘在屏幕中的位置
	/// </summary>
	[Header("June_Add_Attribute_Control_keyBoardTF---------------------------------------")]
	public float _keyBoardTF_X;
	public float _keyBoardTF_Y;



	// INITIALISATION ---------------------------------------------------------
	void Awake()
	{
		// Check that our key array sizes match
		if (upperKeys.Length != lowerKeys.Length && !(lowerKeys.Length == 0 && !shiftStateSwitchEnabled))
		{
			print("Error: OnScreenKeyboard needs the same number of upper and lower case keys, or there must be no lower keys and caps switch must be disabled");
			Destroy(this);
		}

		// Check for row markers and count row lengths
		List<int> rowMarkersTemp = new List<int>();
		for (int i = 0; i < upperKeys.Length; i++)
			if (upperKeys[i] == "<row>") rowMarkersTemp.Add(i);
		rowMarkers = rowMarkersTemp.ToArray();

		// Check row indents
		if (rowIndents.Length < rowMarkers.Length + 1)
		{
			float[] rowIndentsTemp = new float[rowMarkers.Length + 1];
			for (int i = 0; i < rowIndentsTemp.Length; i++)
			{
				if (i < rowIndents.Length) rowIndentsTemp[i] = rowIndents[i];
				else rowIndentsTemp[i] = 0;
			}
		}

		// Check button sizes - anything that's not a single character is a "large" key
		keySizes = new bool[upperKeys.Length];
		for (int i = 0; i < upperKeys.Length; i++) keySizes[i] = upperKeys[i].Length > 1;

		// Populate the array of key rectangles
		keyRects = new Rect[upperKeys.Length];
		int currentRow = 0;
		float xPos = (rowIndents.Length > 0 ? rowIndents[currentRow] : 0) + stdKeySize.x * 0.33f;
		float yPos = stdKeySize.y * 1.33f * currentRow + stdKeySize.y * 0.33f;
		for (int i = 0; i < upperKeys.Length; i++)
		{
			// On the start of a new line, position the new key accordingly
			if (IsRowMarker(i))
			{
				if (i != 0) currentRow++;
				xPos = (rowIndents.Length > 0 ? rowIndents[currentRow] : 0) + stdKeySize.x * 0.33f;
				yPos = stdKeySize.y * 1.33f * currentRow + stdKeySize.y * 0.33f;
			}
			else
			{
				// Draw the key, and set keyPressed accordingly
				keyRects[i] = new Rect(screenRect.x + xPos, screenRect.y + yPos, keySizes[i] ? lgeKeySize.x : stdKeySize.x, keySizes[i] ? lgeKeySize.y : stdKeySize.y);

				// Move over to the next key's position on this line
				xPos += keySizes[i] ? lgeKeySize.x + stdKeySize.x * 0.33f : stdKeySize.x * 1.33f;
			}
		}

		// Put ourselves in a default screen position if we haven't been explicitly placed yet
		if (screenRect.x == 0 && screenRect.y == 0 && screenRect.width == 0 && screenRect.height == 0)
		{
			// Figure out how big we need to be
			float maxWidth = 0;
			float maxHeight = 0;
			for (int i = 0; i < keyRects.Length; i++)
			{
				if (keyRects[i].xMax > maxWidth) maxWidth = keyRects[i].xMax;
				if (keyRects[i].yMax > maxHeight) maxHeight = keyRects[i].yMax;
			}
			maxWidth += stdKeySize.x * 0.33f;
			maxHeight += stdKeySize.y * 0.33f;

			screenRect = new Rect(_keyBoardTF_X, _keyBoardTF_Y, maxWidth, maxHeight);
		}

		// If we've got audio, create sources so we can play it
		if (keySelectSound != null)
		{
			keySelectSource = gameObject.AddComponent<AudioSource>() as AudioSource;
			keySelectSource.spatialBlend = 0;
			keySelectSource.clip = keySelectSound;
		}
		if (keyPressSound != null)
		{
			keyPressSource = gameObject.AddComponent<AudioSource>() as AudioSource;
			keyPressSource.spatialBlend = 0;
			keyPressSource.clip = keyPressSound;
		}

		// Set the initial shift state
		if (shiftStateSwitchEnabled) SetShiftState(shiftStateDefault);

		// Create a pressed button skin for joysticks
		pressedStyle = new GUIStyle();
		pressedStyle.normal.background = keyStyle.active.background;
		pressedStyle.border = keyStyle.border;
		pressedStyle.normal.textColor = keyStyle.active.textColor;
		pressedStyle.alignment = keyStyle.alignment;
		//新增字体样式------->按钮按下的时候调用
		pressedStyle.font = keyStyle.font;

	}


	// GAME LOOP --------------------------------------------------------------

	void Update()
	{
		// Handle keys being released
		if (!keyDownPrevFrame)
		{
			keyRepeatTimer = 0;
			if (!keyReleased) KeyReleased();
		}
		keyDownPrevFrame = false;

		// Check mouse input
		Vector3 guiMousePos = Input.mousePosition;
		guiMousePos.y = Screen.height - guiMousePos.y;
		for (int i = 0; i < keyRects.Length; i++)
		{
			Rect clickRect = keyRects[i];
			clickRect.x += screenRect.x; clickRect.y += screenRect.y;
			// Check for the click ourself, because we want to do it differently to usual
			if (clickRect.Contains(guiMousePos))
			{
				selectedButton = i;
				if (Input.GetMouseButtonDown(0)) KeyPressed();
				else if (Input.GetMouseButton(0)) KeyHeld();
				else if (Input.GetMouseButtonUp(0)) KeyReleased();
			}
		}

		// If the joystick is in use, update accordingly
		if (joystickEnabled) CheckJoystickInput();
	}

	private void CheckJoystickInput()
	{
		// KEY SELECTION		
		float horiz = Input.GetAxis("Horizontal");
		float vert = Input.GetAxis("Vertical");

		moveTimer -= Time.deltaTime;
		if (moveTimer < 0) moveTimer = 0;

		bool hadInput = false;
		bool moved = false;
		if (horiz > 0.5f)
		{
			if (moveTimer <= 0)
			{
				SelectRight();
				moved = true;
			}
			hadInput = true;
		}
		else if (horiz < -0.5f)
		{
			if (moveTimer <= 0)
			{
				SelectLeft();
				moved = true;
			}
			hadInput = true;
		}
		if (vert < -0.5f)
		{
			if (moveTimer <= 0)
			{
				SelectDown();
				moved = true;
			}
			hadInput = true;
		}
		else if (vert > 0.5f)
		{
			if (moveTimer <= 0)
			{
				SelectUp();
				moved = true;
			}
			hadInput = true;
		}
		if (!hadInput) moveTimer = 0;
		if (moved)
		{
			moveTimer += moveRepeatDelay;
			if (keySelectSource != null) keySelectSource.Play();
		}
		selectedButton = Mathf.Clamp(selectedButton, 0, upperKeys.Length - 1);

		// CAPITALS
		if (shiftStateSwitchEnabled &&
			(Input.GetKeyDown(KeyCode.LeftShift) ||
			Input.GetButtonDown(joyCapsButton)))
			shiftState = (shiftState == ShiftState.CapsLock ? ShiftState.Off : ShiftState.CapsLock);

		// TYPING
		if (Input.GetButtonDown(joyPressButton)) KeyPressed();
		else if (Input.GetButton(joyPressButton)) KeyHeld();
	}

	// Called on the first frame where a new key is pressed
	private void KeyPressed()
	{
		keyPressed = (shiftState != ShiftState.Off) ? upperKeys[selectedButton] : lowerKeys[selectedButton];
		pSelectedButton = selectedButton;
		keyRepeatTimer = initialRepeatDelay;

		keyDownPrevFrame = true;
		keyReleased = false;
		lastKeyWasShift = false;

		if (keyPressSource != null) keyPressSource.Play();
	}

	// Called for every frame AFTER the first while a key is being held
	private void KeyHeld()
	{
		// If the key being pressed has changed, revert to an initial press
		if (selectedButton != pSelectedButton)
		{
			KeyReleased();
			KeyPressed();
			return;
		}

		// Check if we're ready to report another press yet
		keyRepeatTimer -= Time.deltaTime;
		if (keyRepeatTimer < 0)
		{
			keyPressed = (shiftState != ShiftState.Off) ? upperKeys[selectedButton] : lowerKeys[selectedButton];
			keyRepeatTimer += continuedRepeatDelay;

			if (keyPressSource != null) keyPressSource.Play();
		}

		keyDownPrevFrame = true;
		keyReleased = false;
	}

	// Called the frame after a key is released
	private void KeyReleased()
	{
		keyDownPrevFrame = false;
		keyReleased = true;

		if (shiftState == ShiftState.Shift && !lastKeyWasShift)
			SetShiftState(ShiftState.Off);
	}

	// Selects the key to the left of the currently selected key
	private void SelectLeft()
	{
		selectedButton--;

		// If we've hit the start of a row, wrap to the end of it instead
		if (IsRowMarker(selectedButton) || selectedButton < 0)
		{
			selectedButton++;
			while (!IsRowMarker(selectedButton + 1) && selectedButton + 1 < upperKeys.Length) selectedButton++;
		}
	}

	// Selects the key to the right of the currently selected key
	private void SelectRight()
	{
		selectedButton++;

		// If we've hit the end of a row, wrap to the start of it instead
		if (IsRowMarker(selectedButton) || selectedButton >= upperKeys.Length)
		{
			selectedButton--;
			while (!IsRowMarker(selectedButton - 1) && selectedButton - 1 >= 0) selectedButton--;
		}
	}

	// Selects the key above the currently selected key
	private void SelectUp()
	{
		// Find the center of the currently selected button
		float selCenter = keyRects[selectedButton].x + keyRects[selectedButton].width / 2;

		// Find the start of the next button;
		int tgtButton = selectedButton;
		while (!IsRowMarker(tgtButton) && tgtButton >= 0) tgtButton--;
		if (IsRowMarker(tgtButton)) tgtButton--;
		if (tgtButton < 0) tgtButton = upperKeys.Length - 1;

		// Find the button with the closest center on that line
		float nDist = float.MaxValue;
		while (!IsRowMarker(tgtButton) && tgtButton >= 0)
		{
			float tgtCenter = keyRects[tgtButton].x + keyRects[tgtButton].width / 2;
			float tDist = Mathf.Abs(tgtCenter - selCenter);
			if (tDist < nDist)
			{
				nDist = tDist;
			}
			else
			{
				selectedButton = tgtButton + 1;
				return;
			}
			tgtButton--;
		}
		selectedButton = tgtButton + 1;
	}

	// Selects the key below the currently selected key
	private void SelectDown()
	{
		// Find the center of the currently selected button
		float selCenter = keyRects[selectedButton].x + keyRects[selectedButton].width / 2;

		// Find the start of the next button;
		int tgtButton = selectedButton;
		while (!IsRowMarker(tgtButton) && tgtButton < upperKeys.Length) tgtButton++;
		if (IsRowMarker(tgtButton)) tgtButton++;
		if (tgtButton >= upperKeys.Length) tgtButton = 0;

		// Find the button with the closest center on that line
		float nDist = float.MaxValue;
		while (!IsRowMarker(tgtButton) && tgtButton < upperKeys.Length)
		{
			float tgtCenter = keyRects[tgtButton].x + keyRects[tgtButton].width / 2;
			float tDist = Mathf.Abs(tgtCenter - selCenter);
			if (tDist < nDist)
			{
				nDist = tDist;
			}
			else
			{
				selectedButton = tgtButton - 1;
				return;
			}
			tgtButton++;
		}
		selectedButton = tgtButton - 1;
	}

	// Returns the row number of a specified button
	private int ButtonRow(int buttonIndex)
	{
		for (int i = 0; i < rowMarkers.Length; i++)
			if (buttonIndex < rowMarkers[i]) return i;

		return rowMarkers.Length;
	}


	// GUI FUNCTIONALITY ------------------------------------------------------

	void OnGUI()
	{
		GUI.Window(windowId, screenRect, WindowFunc, "", boardStyle);
	}

	private void WindowFunc(int id)
	{
		for (int i = 0; i < upperKeys.Length; i++)
		{
			if (!IsRowMarker(i))
			{
				// Draw a glow behind the selected button
				if (i == selectedButton)
					GUI.DrawTexture(new Rect(keyRects[i].x - 5, keyRects[i].y - 5, keyRects[i].width + 10, keyRects[i].height + 10), selectionImage);
				// Draw the key
				// Note that we don't do click detection here, we do it in update
				GUI.Button(keyRects[i], (shiftState != ShiftState.Off) ? upperKeys[i] : lowerKeys[i],
						   (joystickEnabled && selectedButton == i && Input.GetButton(joyPressButton) ? pressedStyle : keyStyle));
			}
		}
	}

	// Returns true if they item at a specified index is a row end marker
	private bool IsRowMarker(int currentKeyIndex)
	{
		for (int i = 0; i < rowMarkers.Length; i++) if (rowMarkers[i] == currentKeyIndex) return true;
		return false;
	}


	// CONTROL INTERFACE ------------------------------------------------------

	// Returns the latest key to be pressed, or null if no new key was pressed 
	// since last time you checked. This means that you can only grab a single 
	// keypress once, as it's cleared once you've read it. It also means that 
	// if you let the user press multiple keys between checks only the most 
	// recent one will be picked up each time.
	public string GetKeyPressed()
	{
		if (keyPressed == null) keyPressed = "";

		string key = keyPressed;
		keyPressed = "";
		return key;
	}

	// Toggle the caps state from elsewhere
	public void SetShiftState(ShiftState newShiftState)
	{
		if (!shiftStateSwitchEnabled) return;

		shiftState = newShiftState;
		if (shiftState == ShiftState.Shift) lastKeyWasShift = true;
	}

	public ShiftState GetShiftState() { return shiftState; }
}