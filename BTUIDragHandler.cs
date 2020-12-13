using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BTUIDragHandler : MonoBehaviour, IDragHandler, IPointerDownHandler{
	private RectTransform dragRectTransform = null;
	private Canvas canvas = null;
	
	public void Initialize(RectTransform dragRectTransform, Canvas canvas){
		this.dragRectTransform = dragRectTransform;
		this.canvas = canvas;
	}
	
	public void OnDrag(PointerEventData eventData){
		if(canvas == null || dragRectTransform == null){
			return;
		}
		
		dragRectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
	}
	
	public void OnPointerDown(PointerEventData eventData){
		if(canvas == null || dragRectTransform == null){
			return;
		}
		
		dragRectTransform.SetAsLastSibling();
	}
}