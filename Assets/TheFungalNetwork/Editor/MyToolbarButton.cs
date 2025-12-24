#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;

[EditorToolbarElement("Main Toolbar/MyBtn")]
public class MyToolbarButton : EditorToolbarButton
{
    public MyToolbarButton()
    {
        text = "MyBtn";
        tooltip = "My Button";
        // icon = EditorGUIUtility.IconContent("d_PlayButton On").image; // optional
        clicked += OnClicked;
    }

    void OnClicked()
    {
        Debug.Log("Toolbar button clicked!");
    }
}
#endif
