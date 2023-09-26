using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
    public class TransparentPanelClickHandler : MonoBehaviour
    {
        public static Vector2 LastClickPosition;

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                LastClickPosition = Input.mousePosition;
            }
        }
    }
 