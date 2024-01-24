using System;
using UnityEngine;
using UnityEngine.UIElements;

public class PopupWindow : VisualElement
{
    [UnityEngine.Scripting.Preserve]
    public new class UxmlFactory : UxmlFactory<PopupWindow> { }
    
    /// <summary>
    /// Get the stylesheet from the resources folder
    /// </summary>
    private const string ussPopupWindow = "popupWindow";
    private const string ussPopupWindowContainer = "popup_container";
    private const string ussHorizontalLayout = "horizontalLayout";
    private const string ussWarningMsg = "warning_message";
    private const string ussPopupText = "popup_text";
    private const string ussPopupButton = "popup_button";
    private const string ussConfirmButton = "button_confirm";
    private const string ussCancelButton = "button_cancel";

    public Label text; 
    public PopupWindow()
    {
        AddToClassList(ussPopupWindowContainer);
        VisualElement window = new VisualElement();
        window.AddToClassList(ussPopupWindow);
        hierarchy.Add(window);
        
        ////// TEXT SECTION //////
        VisualElement horizontalLayoutText = new VisualElement();
        horizontalLayoutText.AddToClassList(ussHorizontalLayout);
        window.Add(horizontalLayoutText);
        
        Label warning = new Label("WARNING");
        text = new Label("UWB has been launched before! Are you sure you want to launch it again?");
        
        warning.AddToClassList(ussWarningMsg);
        text.AddToClassList(ussPopupText);
        horizontalLayoutText.Add(warning);
        horizontalLayoutText.Add(text);
        
        ////// BUTTONS SECTION //////
        // Container for the buttons
        VisualElement horizontalLayoutButtons = new VisualElement();
        horizontalLayoutButtons.AddToClassList(ussHorizontalLayout);
        window.Add(horizontalLayoutButtons);
        
        Button confirmButton = new Button() {text = "Confirm"};
        Button cancelButton = new Button() {text = "Cancel"};
        
        confirmButton.AddToClassList(ussPopupButton);
        confirmButton.AddToClassList(ussConfirmButton);
        
        cancelButton.AddToClassList(ussPopupButton);
        cancelButton.AddToClassList(ussCancelButton);
        
        horizontalLayoutButtons.Add(cancelButton);
        horizontalLayoutButtons.Add(confirmButton);
        confirmButton.clicked += () => { OnConfirm(); };
        cancelButton.clicked += () => { OnCancel(); };
    }
    public event Action confirmed;
    public event Action canceled;
    
    private void OnConfirm()
    {
        confirmed?.Invoke();
    }
    
    private void OnCancel()
    {
        canceled?.Invoke();
    }
}
