using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;

public class KeyboardScript : MonoBehaviour
{

    public InputField TextField;
    public GameObject RusLayoutSml, RusLayoutBig, EngLayoutSml, EngLayoutBig, SymbLayout;
    private byte lastcode = 0;

    [DllImport("user32.dll", EntryPoint = "keybd_event")]
    public static extern void Keybd_event(
         byte bvk,//虚拟键值 ESC键对应的是27
         byte bScan,//0
         int dwFlags,//0为按下，1按住，2释放
         int dwExtraInfo//0
         );
    public void alphabetFunction(string alphabet)
    {
        int code = (int)alphabet[0];
        
        byte code1 = 0;
        if (code < 58)
        {
            code1 = (byte)(code);
        }
        else
        {
            code1 = (byte)(code - 97 + 65);
        }
        Keybd_event(lastcode, 0, 2, 0);
        Keybd_event(code1, 0, 0, 0);
        lastcode = code1;
        //TextField.text=TextField.text + alphabet;
    }

    public void BackSpace()
    {
        Keybd_event(8, 0, 0, 0);
        //Keybd_event(8, 0, 2, 0);
        //if(TextField.text.Length>0) TextField.text= TextField.text.Remove(TextField.text.Length-1);

    }

    public void CloseAllLayouts()
    {
        RusLayoutSml.SetActive(false);
        RusLayoutBig.SetActive(false);
        EngLayoutSml.SetActive(false);
        EngLayoutBig.SetActive(false);
        SymbLayout.SetActive(false);
    }

    public void ShowLayout(GameObject SetLayout)
    {

        CloseAllLayouts();
        SetLayout.SetActive(true);

    }

}
