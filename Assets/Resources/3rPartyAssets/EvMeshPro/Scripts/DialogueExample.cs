using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueExample : MonoBehaviour
{
	private void Start() {
		DialogueController.instance.NewDialogueInstance("Olá isso é uma pomba","character_leo");
		DialogueController.instance.NewDialogueInstance("This is an [NAMES]easy to use package[/NAMES] to give developers a good looking and simple dialogue system."); 
	}
}
