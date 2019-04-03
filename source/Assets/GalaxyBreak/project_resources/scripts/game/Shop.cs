using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Shop : MonoBehaviour 
{
	#region Inspector Members
	[Header("Shop")]
	[Tooltip("Game coins displayed in shop interface")]
	[SerializeField] private Text coinsText;

	[Tooltip("Selected ship sprite game object")]
	[SerializeField] private GameObject selectedObject;

	[Header("Elements")]
	[Tooltip("Product informations as type, price and useful references")]
	[SerializeField] private ShopElement[] elements;

	[Tooltip("Product informations as type, price and useful references")]
	[SerializeField] private InAppProduct[] inAppProducts;

	[Header("References")]
	[SerializeField] private GameplayController gameplayController;
	#endregion

	#region Private Members
	private GameManager gameManager;	// Game manager reference
	#endregion

	#region Main Methods
	private void Awake()
	{
		// Initialize values
		gameManager = GameManager.Instance;

		InitProducts();

		// Move selected game object to currently selected ship
		selectedObject.transform.position = elements[gameManager.CurrentShip].PriceText.transform.parent.position;
	}

	private void Update()
	{
		// Update shop interface information (coins)
		coinsText.text = gameManager.Coins.ToString();
	}
	#endregion

	#region Shop Methods
	public void BuyProduct(int index)
	{
		if (gameManager.Ships[index] == 0)	// Check if product is not purchased yet
		{
			if (elements[index].Price <= gameManager.Coins)		// Check if player has enough coins to buy the product
			{
				// Subtract coins from game manager and set product as bought
				gameManager.Coins -= (int)elements[index].Price;
				gameManager.Ships[index] = 1;

				// Disable price game object
				elements[index].PriceText.gameObject.SetActive(false);

				// Update game manager current ship and save data to disk
				gameManager.CurrentShip = index;
				gameManager.SaveData();

				// Update current instanced ship model
				if (gameplayController.CurrentPlayer) gameplayController.CurrentPlayer.SetModel();

				#if !UNITY_EDITOR
				if (!gameManager.BuyShipAchievement)
				{
					gameManager.BuyShipAchievement = true;
					gameManager.UploadAchievement(ProjectManager.buyShipAchievement);
					gameManager.SaveData();
				}
				#endif

				// Update selected game object to currently selected ship
				selectedObject.transform.position = elements[gameManager.CurrentShip].PriceText.transform.parent.position;
			}
			#if !UNITY_EDITOR
			else gameManager.ShowPopUp(ProjectManager.shopTitle, ProjectManager.shopError);
			#elif DEBUG_INFO
			else Debug.Log("Shop: you don't have enought coins to buy this product.");
			#endif
		}	
		else	// Select ship if it is already purchased
		{
			// Update game manager current ship and save data to disk
			gameManager.CurrentShip = index;
			gameManager.SaveData();

			// Update current instanced ship model
			if (gameplayController.CurrentPlayer) gameplayController.CurrentPlayer.SetModel();

			// Update selected game object to currently selected ship
			selectedObject.transform.position = elements[gameManager.CurrentShip].PriceText.transform.parent.position;
		}
	}

	public void InAppPurchase(int index)
	{
		#if !UNITY_EDITOR
		// Start Android purchasing flow
		gameManager.PurchaseProduct(ProjectManager.coinsPackIds[index]);
		#elif DEBUG_INFO
		Debug.Log("Shop: attempting to buy an Android product");
		#endif
	}

	#region Shop Internal Methods
	private void InitProducts()
	{
		// Initialize products displaying
		for (int i = 0; i < elements.Length; i++)
		{
			// Disable price game object if product is already bought
			if (gameManager.Ships[i] == 1) elements[i].PriceText.gameObject.SetActive(false);
			else elements[i].PriceText.text = elements[i].Price.ToString();
		}

		for (int i = 0; i < inAppProducts.Length; i++)
		{
			inAppProducts[i].PriceText.text = inAppProducts[i].Price.ToString() + "$";
			inAppProducts[i].RewardText.text = inAppProducts[i].Reward.ToString() + " COINS";
		}
	}
	#endregion
	#endregion

	#region Serializable
	[System.Serializable]
	public class ShopElement
	{
		#region Public Members
		[Header("Units")]
		[Tooltip("In game product price in coins")]
		[SerializeField] private int price;

		[Header("References")]
		[Tooltip("Product price text reference")]
		[SerializeField] private Text priceText;
		#endregion

		#region Properties
		public int Price
		{
			get { return price; }
		}

		public Text PriceText
		{
			get { return priceText; }
		}
		#endregion
	}

	[System.Serializable]
	public class InAppProduct
	{
		#region Public Members
		[Header("Units")]
		[Tooltip("In game product price in US dollars")]
		[SerializeField] private float price;

		[Tooltip("In game product reward in coins")]
		[SerializeField] private int reward;

		[Header("References")]
		[Tooltip("Product price text reference")]
		[SerializeField] private Text priceText;

		[Tooltip("Product coins reward text reference")]
		[SerializeField] private Text rewardText;
		#endregion

		#region Properties
		public float Price
		{
			get { return price; }
		}

		public int Reward
		{
			get { return reward; }
		}

		public Text PriceText
		{
			get { return priceText; }
		}

		public Text RewardText
		{
			get { return rewardText; }
		}
		#endregion
	}
	#endregion
}
