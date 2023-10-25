using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class PowerButtonScript : MonoBehaviour {

	public KMBombInfo bomb;
	public KMBombModule module;
	public KMAudio audio;
	public KMBossModule boss;
	
	public KMSelectable button;
	public GameObject ringLight;
	
	private static int moduleIdCounter = 1;
	private int moduleId;
	
	private int count = 0;
	public static string[] ignoredModules = null;
	
	private float startBombTime;
	private bool isOn;
	private bool isSolved;

	void Start () {
		Debug.LogFormat("[Power Button #{0}] Starting up module", moduleId);
		
		startBombTime = bomb.GetTime();
		isOn = UnityEngine.Random.value > 0.5f;
		
		ringLight.GetComponent<Renderer>().enabled = isOn;
		
		if (isOn) {
			Debug.LogFormat("[Power Button #{0}] Light is on", moduleId);
		} else {
			Debug.LogFormat("[Power Button #{0}] Light is off", moduleId);
		}
		
		if (ignoredModules == null) {
			ignoredModules = boss.GetIgnoredModules("Power Buttton", new string[]{
				"14",
				"42",
				"501",
				"Forget Enigma",
				"Forget Everything",
				"Forget It Not",
				"Forget Me Later",
				"Forget Me Not",
				"Forget Perspective",
				"Forget The Colors",
				"Forget Them All",
				"Forget This",
				"Forget Us Not",
				"OmegaForget",
				"Organization",
				"Power Button",
				"Purgatory",
				"Simon Forgets",
				"Simon's Stages",
				"Souvenir",
				"Whiteout",
				"Übermodule",
			});
		};
		
		count = bomb.GetSolvableModuleNames().Where(x => !ignoredModules.Contains(x)).ToList().Count;
	}
	
	void Awake () {
		moduleId = moduleIdCounter++;
		
		button.OnInteract += delegate () { checkPress(); return false; };
	}
	
	void checkPress() {
		if (isSolved) {	return;	}
		button.AddInteractionPunch();
		audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		var solveCount = bomb.GetSolvedModuleNames().Count;
		if (isOn == false){
			if (solveCount >= count) {
				Debug.LogFormat("[Power Button #{0}] It's the last module, solving", moduleId);
				solve();
			} else if (bomb.GetTime() > startBombTime / 2) {
				Debug.LogFormat("[Power Button #{0}] Rule D, more than half original time.", moduleId);
				Debug.LogFormat("[Power Button #{0}] Pressed at {1} solves. Expected multiple of 5", moduleId, solveCount.ToString());
				if ( mod(solveCount, 5) == 0) {
					solve();
				} else {
					module.HandleStrike();
				}
			} else {
				Debug.LogFormat("[Power Button #{0}] Rule E, otherwise.", moduleId);
				Debug.LogFormat("[Power Button #{0}] Pressed at {1} unsolved. Expected multiple of 5", moduleId, (bomb.GetSolvableModuleNames().Count - solveCount).ToString());
				if ( mod(bomb.GetSolvableModuleNames().Count - solveCount, 5)  == 0) {
					solve();
				} else {
					module.HandleStrike();
				}
			}
		} else {
			if (bomb.GetBatteryCount() > 2 && bomb.GetOnIndicators().Count() > 0){
				Debug.LogFormat("[Power Button #{0}] Rule A, +2 batteries and at least 1 lit indicator", moduleId);
				int target = mod(bomb.GetSerialNumberNumbers().Sum(), 9) == 0 ? 9 : mod(bomb.GetSerialNumberNumbers().Sum(), 9); 
				Debug.LogFormat("[Power Button #{0}] Pressed at {1} seconds. Expected time is at {2}", moduleId, mod((int)bomb.GetTime(), 10),target.ToString());
				if (mod((int)bomb.GetTime(), 10) == target) {
					solve();
				} else {
					module.HandleStrike();
				}
			} else if ( (bomb.IsIndicatorOn("CAR") || bomb.IsIndicatorOff("CAR")) && bomb.GetPortCount(Port.RJ45) >= 1) {
				Debug.LogFormat("[Power Button #{0}] Rule B, CAR indicator and at least 1 RJ45", moduleId);
				int target = bomb.GetSolvableModuleNames().Count - solveCount;
				while (target > 59)	target -= 20;
				Debug.LogFormat("[Power Button #{0}] Pressed at {1} seconds. Expected is {2}", moduleId, mod((int)bomb.GetTime(), 60), target.ToString().PadLeft(2, '0'));;
				if (mod((int)bomb.GetTime(), 60) == target)	solve();
				else module.HandleStrike();
			} else {
				Debug.LogFormat("[Power Button #{0}] Rule C, otherwise", moduleId);
				int target = mod((int)bomb.GetTime(), 60)/10 + mod((int)bomb.GetTime(), 10);
				Debug.LogFormat("[Power Button #{0}] Pressed at {1} seconds, seconds sum: {2}. Expected is prime", moduleId, mod((int)bomb.GetTime(), 60), target);
				if (target == 2 || target == 3 || target == 5 || target == 7 || target == 11 || target == 13){
					solve();
				} else {
					module.HandleStrike();
				}
			}
		}
	}
	
	int mod(int a, int b) {  return ((a %= b) < 0) ? a+b : a;  }
	
	void solve() {
		Debug.LogFormat("[Power Button #{0}] Module Solved!", moduleId);
		module.HandlePass();
		isSolved = true;
		ringLight.GetComponent<Renderer>().enabled = !ringLight.GetComponent<Renderer>().enabled;
	}

	//twitch plays
	#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} press (##/#) [Presses the power button (optionally when the last two digits of the bomb's timer are '##' or the last digit is '#')]";
	#pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string command)
	{
		string[] parameters = command.Split(' ');
		if (parameters[0].EqualsIgnoreCase("press"))
		{
			if (parameters.Length > 2)
				yield return "sendtochaterror Too many parameters!";
			else if (parameters.Length == 2)
			{
				int temp = -1;
				if (!int.TryParse(parameters[1], out temp))
				{
					yield return "sendtochaterror!f The specified number '" + parameters[1] + "' is invalid!";
					yield break;
				}
				if (temp < 0 || temp > 59)
				{
					yield return "sendtochaterror The specified number '" + parameters[1] + "' is invalid!";
					yield break;
				}
				yield return null;
				if (parameters[1].Length == 2)
					while (temp != mod((int)bomb.GetTime(), 60)) yield return "trycancel Halted waiting to press the button due to a cancel request!";
				else
					while (temp != mod((int)bomb.GetTime(), 10)) yield return "trycancel Halted waiting to press the button due to a cancel request!";
				button.OnInteract();
			}
			else if (parameters.Length == 1)
			{
				yield return null;
				button.OnInteract();
			}
		}
	}

	IEnumerator TwitchHandleForcedSolve()
    {
		if (!isOn)
        {
			if (bomb.GetTime() > startBombTime / 2)
            {
				while (mod(bomb.GetSolvedModuleNames().Count, 5) != 0 && bomb.GetSolvedModuleNames().Count < count) yield return true;
				button.OnInteract();
			}
			else
			{
				while (mod(bomb.GetSolvableModuleNames().Count - bomb.GetSolvedModuleNames().Count, 5) != 0 && bomb.GetSolvedModuleNames().Count < count) yield return true;
				button.OnInteract();
			}
		}
		else
        {
			if (bomb.GetBatteryCount() > 2 && bomb.GetOnIndicators().Count() > 0)
            {
				while (mod((int)bomb.GetTime(), 10) != (mod(bomb.GetSerialNumberNumbers().Sum(), 9) == 0 ? 9 : mod(bomb.GetSerialNumberNumbers().Sum(), 9))) yield return true;
				button.OnInteract();
			}
			else if ((bomb.IsIndicatorOn("CAR") || bomb.IsIndicatorOff("CAR")) && bomb.GetPortCount(Port.RJ45) >= 1)
			{
				int target = bomb.GetSolvableModuleNames().Count - bomb.GetSolvedModuleNames().Count;
				while (target > 59) target -= 20;
				while (mod((int)bomb.GetTime(), 60) != target)
                {
					yield return true;
					target = bomb.GetSolvableModuleNames().Count - bomb.GetSolvedModuleNames().Count;
					while (target > 59) target -= 20;
				}
				button.OnInteract();
			}
            else
            {
				while (!(mod((int)bomb.GetTime(), 60) / 10 + mod((int)bomb.GetTime(), 10)).EqualsAny(2, 3, 5, 7, 11, 13)) yield return true;
				button.OnInteract();
			}
		}
    }
}
