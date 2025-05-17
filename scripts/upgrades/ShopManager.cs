// ShopManager.cs - Main script to handle shop functionality
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public partial class ShopManager : CanvasLayer
{
	[Export] public Control ShopPanel { get; set; }
	[Export] public Control UpgradeContainer { get; set; }
	[Export] public Label CurrencyLabel { get; set; }
	
	private bool _isShopOpen = false;
	private Player Player;
	private int _playerCurrency = 1000; // Starting currency
	private Dictionary<string, PlayerUpgrade> _availableUpgrades = new();
	private Dictionary<string, PlayerUpgrade> _purchasedUpgrades = new();
	
	// Path for saving game data
	private readonly string _savePath = "user://shop_data.json";
	
	public override void _Ready()
	{

		Player = GetTree().GetNodesInGroup("player")[0] as Player;

		// Hide shop on start
		if (ShopPanel != null)
			ShopPanel.Visible = false;
		
		// Initialize upgrades
		InitializeUpgrades();
		
		// Load saved data
		LoadShopData();
		
		// Update UI
		UpdateCurrencyDisplay();
		PopulateShop();
	}
	public override void _PhysicsProcess(double delta)
	{
		GD.Print($"HP: {Player.PlayerStats.BaseHealth}");
	}

	public override void _Input(InputEvent @event)
	{
		// Toggle shop with "o" key
		if (@event.IsActionPressed("toggle_shop") || (@event is InputEventKey key && key.Keycode == Key.O && key.Pressed && !key.Echo))
		{
			ToggleShop();
			GetViewport().SetInputAsHandled();
		}

		// Handle scrolling
		if (_isShopOpen && @event is InputEventMouseButton mouseEvent)
		{
			if (ShopPanel == null || !ShopPanel.Visible) return;

			if (mouseEvent.ButtonIndex == MouseButton.WheelUp || mouseEvent.ButtonIndex == MouseButton.WheelDown)
			{
				if (UpgradeContainer != null)
				{
					Vector2 newPosition = UpgradeContainer.Position;
					float scrollSpeed = 20.0f;

					if (mouseEvent.ButtonIndex == MouseButton.WheelUp)
						newPosition.Y += scrollSpeed;
					else
						newPosition.Y -= scrollSpeed;

					// Limit scrolling
					newPosition.Y = Mathf.Clamp(newPosition.Y, -UpgradeContainer.Size.Y + ShopPanel.Size.Y - 100, 0);
					UpgradeContainer.Position = newPosition;

					GetViewport().SetInputAsHandled();
				}
			}
		}
	}
	
	private void ToggleShop()
	{
		_isShopOpen = !_isShopOpen;
		
		if (ShopPanel != null)
			ShopPanel.Visible = _isShopOpen;
	}

	private void InitializeUpgrades()
	{
		// Define available upgrades
		_availableUpgrades.Add("speed", new PlayerUpgrade
		{
			Id = "speed",
			Name = "Speed Boost",
			Description = "Increases movement speed by 15%",
			BaseCost = 20,
			MaxLevel = 10,
			CurrentLevel = 0,
			Action=(lvl)=>{Player.PlayerStats.Speed*=1.15f;}
		});

		_availableUpgrades.Add("damage", new PlayerUpgrade
		{
			Id = "damage",
			Name = "Damage Upgrade",
			Description = "Increases attack damage by 15%",
			BaseCost = 30,
			MaxLevel = 10,
			CurrentLevel = 0,
			Action=(lvl)=>{Player.PlayerStats.Damage*=1.15f;}
		});

		_availableUpgrades.Add("health", new PlayerUpgrade
		{
			Id = "health",
			Name = "Health Boost",
			Description = "Increases max health by 25 points",
			BaseCost = 40,
			MaxLevel = 10,
			CurrentLevel = 0,
			Action=(lvl)=>{Player.PlayerStats.BaseHealth+=lvl*25;}
		});
	}
	
	private void PopulateShop()
	{
		// Clear existing items first
		foreach (Node child in UpgradeContainer.GetChildren())
		{
			child.QueueFree();
		}
		
		// Create a UI item for each upgrade
		PackedScene upgradeItemScene = GD.Load<PackedScene>("res://scenes/UpgradeItem.tscn");
		
		foreach (var upgrade in _availableUpgrades.Values)
		{
			var upgradeItem = (UpgradeItem)upgradeItemScene.Instantiate();
			UpgradeContainer.AddChild(upgradeItem);
			
			// Setup the item
			upgradeItem.SetUpgradeInfo(upgrade);
			upgradeItem.SetShopManager(this);
			
			// Update the display based on purchased status
			if (_purchasedUpgrades.ContainsKey(upgrade.Id))
			{
				upgradeItem.UpdateLevel(_purchasedUpgrades[upgrade.Id].CurrentLevel);

				//load in the upgrades to player
				_purchasedUpgrades[upgrade.Id].MakeActionInit();
			}
		}
	}
	
	public void PurchaseUpgrade(string upgradeId)
	{
		// Get the upgrade
		if (!_availableUpgrades.ContainsKey(upgradeId))
			return;
			
		PlayerUpgrade upgrade = _availableUpgrades[upgradeId];
		int currentLevel = 0;
		
		// Check if we already have this upgrade
		if (_purchasedUpgrades.ContainsKey(upgradeId))
		{
			currentLevel = _purchasedUpgrades[upgradeId].CurrentLevel;
			_purchasedUpgrades[upgradeId].MakeAction();
			
			// Check if max level reached
			if (currentLevel >= upgrade.MaxLevel)
			{
				GD.Print($"Upgrade {upgrade.Name} already at max level!");
				return;
			}
		}
		
		// Calculate cost for the next level
		int cost = upgrade.BaseCost * (currentLevel + 1);
		
		// Check if we have enough currency
		if (_playerCurrency < cost)
		{
			GD.Print("Not enough currency!");
			return;
		}
		
		// Purchase the upgrade
		_playerCurrency -= cost;
		
		// Add or update in purchased upgrades
		if (!_purchasedUpgrades.ContainsKey(upgradeId))
		{
			var purchasedUpgrade = new PlayerUpgrade
			{
				Id = upgrade.Id,
				Name = upgrade.Name,
				Description = upgrade.Description,
				BaseCost = upgrade.BaseCost,
				MaxLevel = upgrade.MaxLevel,
				CurrentLevel = 1
			};
			_purchasedUpgrades[upgradeId] = purchasedUpgrade;
		}
		else
		{
			_purchasedUpgrades[upgradeId].CurrentLevel++;
		}
		
		// Update UI
		UpdateCurrencyDisplay();
		UpdateShopItems();
				
		// Save data
		SaveShopData();
	}

	private void UpdateCurrencyDisplay()
	{
		if (CurrencyLabel != null)
			CurrencyLabel.Text = $"Currency: {_playerCurrency}";
		
	}
	
	private void UpdateShopItems()
	{
		foreach (Node child in UpgradeContainer.GetChildren())
		{
			if (child is UpgradeItem upgradeItem)
			{
				string id = upgradeItem.UpgradeId;
				if (_purchasedUpgrades.ContainsKey(id))
				{
					upgradeItem.UpdateLevel(_purchasedUpgrades[id].CurrentLevel);
				}
			}
		}
	}
	
	
	
	// Save shop data to disk
	private void SaveShopData()
	{
		ShopData data = new ShopData
		{
			PlayerCurrency = _playerCurrency,
			PurchasedUpgrades = _purchasedUpgrades
		};
		
		foreach (var kvp in data.PurchasedUpgrades)
		{
			kvp.Value.Action = null;

		}
		
		try
		{
			string jsonString = JsonSerializer.Serialize(data, new JsonSerializerOptions
			{
				WriteIndented = true
			});

			using Godot.FileAccess file = Godot.FileAccess.Open(_savePath, Godot.FileAccess.ModeFlags.Write);
			file.StoreString(jsonString);
			GD.Print("Shop data saved successfully.");
		}
		catch (Exception e)
		{
			GD.PrintErr($"Error saving shop data: {e.Message}");
		}
	}
	
	// Load shop data from disk
	private void LoadShopData()
	{
		if (!Godot.FileAccess.FileExists(_savePath))
		{
			GD.Print("No save file found. Starting with default values.");
			return;
		}
		
		try
		{
			using Godot.FileAccess file = Godot.FileAccess.Open(_savePath, Godot.FileAccess.ModeFlags.Read);
			string jsonString = file.GetAsText();
			
			ShopData data = JsonSerializer.Deserialize<ShopData>(jsonString);

			if (data != null)
			{
				_playerCurrency = data.PlayerCurrency;
				_purchasedUpgrades = data.PurchasedUpgrades;

				foreach (var kvp in _purchasedUpgrades)
					{
						if (_availableUpgrades.TryGetValue(kvp.Key, out var baseUpgrade))
						{
							kvp.Value.Action = baseUpgrade.Action;
						}
					}

				GD.Print("Shop data loaded successfully.");
			}
		}
		catch (Exception e)
		{
			GD.PrintErr($"Error loading shop data: {e.Message}");
		}
	}
	
	// Add currency (for testing)
	public void AddCurrency(int amount)
	{
		_playerCurrency += amount;
		UpdateCurrencyDisplay();
		SaveShopData();
	}
}

// Player Upgrade class to store upgrade info
[Serializable]
public class PlayerUpgrade
{
	public string Id { get; set; }
	public string Name { get; set; }
	public string Description { get; set; }
	public int BaseCost { get; set; }
	public int MaxLevel { get; set; }
	public int CurrentLevel { get; set; }
	public Action<int> Action { get; set; }

	public void MakeActionInit()
	{
		Action?.Invoke(CurrentLevel);
	}

	public void MakeAction()
	{
		Action?.Invoke(1);
	}
}
// Shop Data class for saving
[Serializable]
public class ShopData
{
	public int PlayerCurrency { get; set; }
	public Dictionary<string, PlayerUpgrade> PurchasedUpgrades { get; set; }
}
