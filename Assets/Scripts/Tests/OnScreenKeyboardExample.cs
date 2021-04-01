using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 这是虚拟键盘插件脚本，June于2020.4.16改
/// </summary>


public class OnScreenKeyboardExample : MonoBehaviour
{
	public OnScreenKeyboard osk;
	/// <summary>
	/// 输入文字
	/// </summary>
	private string _inputString;
	/// <summary>
	/// 输入文本框
	/// </summary>
	public InputField _inputField;

	//每次激活清空文本框内容
	private void OnEnable()
	{
		_inputString = "";
		//Application.OpenURL(@"C:\Windows\System32\osk.exe");
	}

	void Update()
	{
		// You can use input from the OSK just by asking for the most recent 
		// pressed key, which will be returned to you as a string, or null if 
		// no key has been pressed since you last checked. Note that if more 
		// than one key has been pressed you will only be given the most recent.
		string keyPressed = osk.GetKeyPressed();

		if (keyPressed != "")
		{
			Debug.Log(keyPressed);

			// Take different action depending on what key was pressed
			if (keyPressed == "Backspace" || keyPressed == "<<")
			{
				// Remove a character
				if (_inputString.Length > 0)
					_inputString = _inputString.Substring(0, _inputString.Length - 1);
			}
			else if (keyPressed == "Space")
			{
				// Add a space
				_inputString += " ";
			}
			else if (keyPressed == "Enter" || keyPressed == "Done")
			{
				// Change screens, or do whatever you want to 
				// do when your user has finished typing :-)
			}
			else if (keyPressed == "Caps")
			{
				// Toggle the capslock state yourself
				osk.SetShiftState(osk.GetShiftState() == ShiftState.CapsLock ? ShiftState.Off : ShiftState.CapsLock);
			}
			else if (keyPressed == "Shift")
			{
				// Toggle shift state ourselves
				osk.SetShiftState(osk.GetShiftState() == ShiftState.Shift ? ShiftState.Off : ShiftState.Shift);
			}
			else
			{
				
				//限制输入
				//if (_inputField.text.Length >= _inputField.characterLimit) return;
				// Add a letter to the existing string
				_inputString += keyPressed;
			}
			//将文字赋值给文本框中的文本属性
			_inputField.text = _inputString;

		}
	}
}
