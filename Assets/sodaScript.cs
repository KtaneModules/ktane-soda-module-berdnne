using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class sodaScript : MonoBehaviour {

	public KMBombInfo bombInfo;
	public KMAudio Audio;

	// Components of module

	public KMSelectable can;
	public KMSelectable sipButton;
	public KMSelectable slurpButton;
	private int totalModuleNum;

	// Text meshes
	public TextMesh calorieText;
	public TextMesh fatText;
	public TextMesh cholesterolText;
	public TextMesh sodiumText;
	public TextMesh carbText;
	public TextMesh proteinText;
	public TextMesh servingText;

	// Logging
	static int moduleIdCounter = 1;
	int moduleId;
	private bool moduleSolved = false;

	// Module Variables
	private int calorieNum;
	private int fatNum;
	private int cholesterolNum;
	private int sodiumNum;
	private int carbNum;
	private int proteinNum;

	// Correct Answer
	private int correctSipNum = -1;
	private int correctSlurpNum = -1;

	// Inputted Answer

	private int enteredSipNum = 0;
	private int enteredSlurpNum = 0;

	void Awake () 
	{
		moduleId = moduleIdCounter++;

		can.OnInteract += delegate () { PressCan(); return false; };
		sipButton.OnInteract += delegate () { PressSipButton(); return false; };
		slurpButton.OnInteract += delegate () { PressSlurpButton(); return false; };
	}

	// Use this for initialization
	void Start ()
	{
		
		calorieNum = UnityEngine.Random.Range(10,31) * 10; // 100 to 300, always multiples of 10 (g)
		fatNum = UnityEngine.Random.Range(0,10) + 1; // 1 to 10 (g)
		cholesterolNum = UnityEngine.Random.Range(0,10) + 1; // 1 to 10 (mg)
		sodiumNum = (UnityEngine.Random.Range(0,20) + 1) * 5; // 5 to 100, always multiples of 5 (mg)
		carbNum = (UnityEngine.Random.Range(0,20) + 1) * 5; // 5 to 100, always multiples of 5 (g)
		proteinNum = UnityEngine.Random.Range(0,20) + 1; // 1 to 20 (g)

		calorieText.text = calorieNum + "";
		fatText.text = fatNum + "g";
		cholesterolText.text = cholesterolNum + "mg";
		sodiumText.text = sodiumNum + "mg";
		carbText.text = carbNum + "g";
		proteinText.text = proteinNum + "g";

		totalModuleNum = bombInfo.GetModuleNames().Count();

		servingText.text = "Serving Size 1 module\n\nServing Per Container " + totalModuleNum + "\n\n\n\n\nAmount Per Serving";

		determineSolution();

		Debug.Log("[Soda #" + moduleId + "] Calories: " + calorieNum + " Fat: " + fatNum + "g, Cholesterol: " + cholesterolNum + "mg,  Sodium: " + sodiumNum + "mg, Carbs: " + carbNum + "g, Protein: " + proteinNum + "g" + ", Solution: " + correctSipNum + " sips, " + correctSlurpNum + " slurps.");

	}

	void PressSipButton ()
	{
		enteredSipNum++;
		sipButton.AddInteractionPunch();
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, transform);
		GetComponent<KMAudio>().PlaySoundAtTransform("sip", transform);
		Debug.Log("Sip button pressed! Now at " + enteredSipNum + " sips.");
	}

	void PressSlurpButton ()
	{
		enteredSlurpNum++;
		slurpButton.AddInteractionPunch();
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, transform);
		GetComponent<KMAudio>().PlaySoundAtTransform("slurp", transform);
		Debug.Log("Slurp button pressed! Now at " + enteredSlurpNum + " slurps.");
	}

	void PressCan ()
	{
		Debug.Log("Can pressed!");

		if (moduleSolved){
			return;
		}

		if (enteredSipNum == correctSipNum && enteredSlurpNum == correctSlurpNum){
			// solve
			moduleSolved = true;
			GetComponent<KMBombModule>().HandlePass();
			GetComponent<KMAudio>().PlaySoundAtTransform("burp", transform);
			Debug.Log("[Soda #" + moduleId + "] Module solved.");
		}
		else {
			// strike
			GetComponent<KMBombModule>().HandleStrike();
			enteredSipNum = 0;
			enteredSlurpNum = 0;
			Debug.Log("[Soda #" + moduleId + "] Incorrect answer. Strike!");
		}
	}

	private void determineSolution()
	{
		// Determine number of sips

		if (calorieNum < 150){
			correctSipNum = 0;
		}
		else if (calorieNum >= 150 && calorieNum < 200){
			
			if (fatNum + carbNum + proteinNum < 10){
				correctSipNum = fatNum + carbNum + proteinNum;
			}
			else {
				correctSipNum = (fatNum + carbNum + proteinNum) % 10;
			}
		
		}
		else if (calorieNum >= 200 && calorieNum <= 250){
			
			if (cholesterolNum + sodiumNum < 10){
				correctSipNum = 0;
			}
			else if (cholesterolNum + sodiumNum < 100){
				correctSipNum = ((cholesterolNum + sodiumNum) / 10);
			}
			else {
				correctSipNum = ((cholesterolNum + sodiumNum) / 10) % 10;
			}

		}
		else if (calorieNum > 250){
			correctSipNum = proteinNum;
		}

		// Determine number of slurps

		if (calorieNum < 150){
			correctSlurpNum = fatNum;
		}
		else if (calorieNum >= 150 && calorieNum < 200){
			
			if (totalModuleNum < 10){
				correctSlurpNum = totalModuleNum;
			}
			else {
				correctSlurpNum = totalModuleNum % 10;
			}

		}
		else if (calorieNum >= 200 && calorieNum <= 250){
			
			if (proteinNum - fatNum < 0){
				correctSlurpNum = 0;
			}
			else {
				correctSlurpNum = proteinNum - fatNum;
			}

		}
		else if (calorieNum > 250){
			correctSlurpNum = 0;
		}
	}

#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} sip # sips that number of times. | !{0} slurp # slurps that number of times. | !{0} can submits the module.";
#pragma warning restore 414

	IEnumerator ProcessTwitchCommand (string command)
	{
		string[] split = command.ToUpperInvariant().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

		yield return null;

		if ("SIP".ContainsIgnoreCase(split[0]))
		{
			if (split.Length == 1)
			{
				yield return "sendtochaterror Please specify how many times to sip!";
				yield break;
			}
			if (split.Length > 2)
				yield break;

			foreach (var num in split[1])
				if (!"0123456789".Contains(num))
					yield break;

			int sipCount = 0;

			while (sipCount < int.Parse(split[1]))
			{
				sipButton.OnInteract();
				sipCount++;
				yield return new WaitForSeconds(0.1f);
			}
			yield break;
		}

		if ("SLURP".ContainsIgnoreCase(split[0]))
		{
			if (split.Length == 1)
			{
				yield return "sendtochaterror Please specify how many times to slurp!";
				yield break;
			}

			if (split.Length > 2)
				yield break;

			foreach (var num in split[1])
				if (!"01234456789".Contains(num))
					yield break;

			int slurpCount = 0;

			while (slurpCount < int.Parse(split[1]))
			{
				slurpButton.OnInteract();
				slurpCount++;
				yield return new WaitForSeconds(0.1f);
			}
			yield break;
		}

		if ("CAN".ContainsIgnoreCase(split[0]))
		{
			can.OnInteract();
			yield return new WaitForSeconds(0.1f);
		}
	}

	IEnumerator TwitchHandleForcedSolve()
	{
		yield return null;

		if (enteredSipNum > correctSipNum)
			enteredSipNum = 0;

		if (enteredSlurpNum > correctSlurpNum)
			enteredSlurpNum = 0;

		while (enteredSipNum != correctSipNum)
		{
			sipButton.OnInteract();
			yield return new WaitForSeconds(0.1f);
		}

		while (enteredSlurpNum != correctSlurpNum)
		{
			slurpButton.OnInteract();
			yield return new WaitForSeconds(0.1f);
		}

		can.OnInteract();
		yield return new WaitForSeconds(0.1f);
	}
}
