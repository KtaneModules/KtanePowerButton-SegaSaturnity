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
}
