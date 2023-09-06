using UnityEngine;
using UnityEngine.EventSystems;

public class PanelControl : MonoBehaviour, IPointerDownHandler {
	public GameObject Panel;

	public void OnPointerDown(PointerEventData pointerEventData) {
		if (Panel != null) {
			Panel.SetActive(false);
		}
	}

	public void OpenPanel() {
		if (Panel != null) {
			Panel.SetActive(true);
		}
	}

	public void ClosePanel() {
		if (Panel != null) {
			Panel.SetActive(false);
		}
	}
}