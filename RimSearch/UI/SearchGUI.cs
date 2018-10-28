using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimSearch.UI
{
    /// <summary>
    /// Custom IMGUI controls can be found here.
    /// </summary>
    public static class SearchGUI
    {
        public static Color cursorOverlayColor = new Color(1f, 1f, 0f, 0.5f);

        /// <summary>
        /// [Not currently used.] Formats text in a way that highlight special characters and words.
        /// </summary>
        /// <param name="rect">Text field area.</param>
        /// <param name="text">Already existing text.</param>
        /// <returns>New text from input.</returns>
        public static string FormattedTextField(Rect rect, string text)
        {
            //string controlName = text.GetHashCode() + "_FormattedTextField";
            //GUI.SetNextControlName(controlName);

            //Control ID
            int controlID = GUIUtility.GetControlID(FocusType.Keyboard) + 1;

            //Handle control.
            string outText = GUI.TextField(rect, text);

            //Overlay parts of text.
            TextEditor textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), controlID);
            //textEditor.controlID = controlID;
            textEditor.style.clipping.ToString();

            int cursorIndex = textEditor.cursorIndex;

            Rect cursorRect = new Rect(rect);
            cursorRect.width = 8f;
            cursorRect.x += Text.CalcSize(outText.Substring(0, cursorIndex)).x - 4;

            Widgets.DrawBoxSolid(cursorRect, cursorOverlayColor);

            //Return modifed text.
            return outText;
        }
    }
}
