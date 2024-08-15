using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyDialog
{
    public static void Show(string title, string content, Chibi.Free.Dialog.ActionButton[] buttons)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            Chibi.Free.Dialog dialog = GameObject.Find("ChibiDialog").GetComponent<Chibi.Free.Dialog>();
            dialog.ShowDialog(title, content, buttons);
        });
    }

    public static void ShowMessage(string title, string content,Action ok_click=null)
    {
        Chibi.Free.Dialog.ActionButton ok = new Chibi.Free.Dialog.ActionButton("确定", () => { ok_click?.Invoke(); }, new Color(0f, 0.9f, 0.9f));
        Chibi.Free.Dialog.ActionButton[] buttons = { ok };
        MyDialog.Show(title, content, buttons);
    }
}
