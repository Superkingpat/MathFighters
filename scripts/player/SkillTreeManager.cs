using Godot;
using System;
using System.Collections.Generic;

public partial class SkillTreeManager : Node {
	public static SkillTreeManager Instance;

	public int AscensionPoints { get; private set; } = 0;
	public Dictionary<string, bool> UnlockedSkills { get; private set; } = new Dictionary<string, bool>();

	private const string SavePath = "user://ascension_data.save";

	public override void _Ready() {
		if (Instance == null) {
			Instance = this;
			LoadAscensionData();
		} else {
			QueueFree(); // Ensure only one instance exists
		}
	}

	public void GainAscensionPoints(int amount) {
		AscensionPoints += amount;
		SaveAscensionData();
		// Emit a signal for UI update
	}

	public bool CanUnlockSkill(string skillId, Dictionary<string, string> skillRequirements) {
		if (UnlockedSkills.ContainsKey(skillId) && UnlockedSkills[skillId]) {
			return false; // Already unlocked
		}
		foreach (var requirement in skillRequirements) {
			if (requirement.Key == "prerequisite" && (!UnlockedSkills.ContainsKey(requirement.Value) || !UnlockedSkills[requirement.Value])) {
				return false; // Prerequisite not met
			}
		}
		return AscensionPoints > 0; // Can unlock if we have at least one point
	}

	public bool TryUnlockSkill(string skillId, int cost) {
		// In a real system, skill costs and requirements would be defined elsewhere
		Dictionary<string, string> skillRequirements = GetSkillRequirements(skillId); // Implement this

		if (CanUnlockSkill(skillId, skillRequirements) && AscensionPoints >= cost) {
			AscensionPoints -= cost;
			UnlockedSkills[skillId] = true;
			SaveAscensionData();
			// Emit a signal that a skill was unlocked
			return true;
		}
		return false;
	}

	public bool IsSkillUnlocked(string skillId) {
		return UnlockedSkills.ContainsKey(skillId) && UnlockedSkills[skillId];
	}

	private Dictionary<string, string> GetSkillRequirements(string skillId) {
		// This would fetch skill data from a more structured source (e.g., a Resource)
		if (skillId == "damage_up_1") {
			return new Dictionary<string, string>(); // No prerequisites
		} else if (skillId == "speed_up_1") {
			return new Dictionary<string, string>() { { "prerequisite", "damage_up_1" } };
		}
		return new Dictionary<string, string>();
	}

	private void SaveAscensionData() {
		var saveDict = new Godot.Collections.Dictionary();
		saveDict["ascension_points"] = AscensionPoints;
		var godotSkills  = new Godot.Collections.Dictionary();
		foreach (var kvp in UnlockedSkills)
			godotSkills[kvp.Key] = kvp.Value;
		saveDict["unlocked_skills"] = godotSkills;

		using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
		if (file != null) {
			var jsonString = Json.Stringify(saveDict);
			file.StoreLine(jsonString);
		} else {
			GD.PushError($"Could not save ascension data to {SavePath}");
		}
	}

	private void LoadAscensionData() {
		if (FileAccess.FileExists(SavePath)) {
			using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
			if (file != null) {
				var jsonString = file.GetLine();
				var saveDict = (Godot.Collections.Dictionary)Json.ParseString(jsonString);
				if (saveDict != null) {
					AscensionPoints = saveDict.ContainsKey("ascension_points") ? (int)saveDict["ascension_points"] : 0;
					if (saveDict.ContainsKey("unlocked_skills")) {
						var unlockedSkillsDict = (Godot.Collections.Dictionary)saveDict["unlocked_skills"];
						foreach (var key in unlockedSkillsDict.Keys) {
							UnlockedSkills[(string)key] = (bool)unlockedSkillsDict[key];
						}
					}
				} else {
					GD.PushError($"Could not load ascension data from {SavePath}");
				}
			} else {
				GD.PushError($"Could not load ascension data from {SavePath}");
			}
		}
	}
}
