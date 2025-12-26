using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace SwyPhexLeague.UI
{
    public class CustomizationUI : MonoBehaviour
    {
        [System.Serializable]
        public class CarPreview
        {
            public string carId;
            public Sprite previewSprite;
            public GameObject modelPrefab;
            public string displayName;
            public int unlockLevel;
            public int cost;
        }
        
        [Header("Car Selection")]
        public Transform carGrid;
        public GameObject carButtonPrefab;
        public List<CarPreview> carPreviews;
        
        [Header("Color Customization")]
        public Color[] availableColors;
        public Transform colorGrid;
        public GameObject colorButtonPrefab;
        public Image currentColorDisplay;
        
        [Header("Decals")]
        public Sprite[] availableDecals;
        public Transform decalGrid;
        public GameObject decalButtonPrefab;
        public Image currentDecalDisplay;
        
        [Header("Stats Display")]
        public Text carNameText;
        public Text carDescriptionText;
        public Slider[] statSliders;
        public Text[] statValues;
        
        [Header("Purchase")]
        public GameObject purchasePanel;
        public Text purchaseText;
        public Button purchaseButton;
        public Text purchaseButtonText;
        
        private string selectedCarId;
        private Color selectedColor;
        private Sprite selectedDecal;
        
        private void Start()
        {
            LoadCarData();
            LoadCustomization();
        }
        
        public void LoadCarData()
        {
            ClearCarGrid();
            
            foreach (CarPreview preview in carPreviews)
            {
                GameObject buttonObj = Instantiate(carButtonPrefab, carGrid);
                CarButton button = buttonObj.GetComponent<CarButton>();
                
                if (button)
                {
                    button.Initialize(preview, OnCarSelected);
                }
            }
            
            if (carPreviews.Count > 0)
            {
                SelectCar(carPreviews[0].carId);
            }
        }
        
        private void ClearCarGrid()
        {
            foreach (Transform child in carGrid)
            {
                Destroy(child.gameObject);
            }
        }
        
        private void OnCarSelected(string carId)
        {
            SelectCar(carId);
        }
        
        public void SelectCar(string carId)
        {
            selectedCarId = carId;
            
            CarPreview preview = carPreviews.Find(p => p.carId == carId);
            if (preview == null) return;
            
            UpdateCarDisplay(preview);
            UpdatePurchaseInfo(preview);
            
            LoadColorOptions();
            LoadDecalOptions();
        }
        
        private void UpdateCarDisplay(CarPreview preview)
        {
            if (carNameText)
            {
                carNameText.text = preview.displayName;
            }
            
            if (carDescriptionText)
            {
                carDescriptionText.text = GetCarDescription(preview.carId);
            }
            
            UpdateCarStats(preview.carId);
        }
        
        private string GetCarDescription(string carId)
        {
            return carId switch
            {
                "PHEX-01" => "Balanced car with Pulse Dash ability",
                "NEON_WRAITH" => "Speed focused with Magnet Core",
                "GRAVRIDER" => "Controls gravity with Gravity Flip",
                "STREET_BRUISER" => "Defensive car with Shock Drop",
                _ => "Standard racing car"
            };
        }
        
        private void UpdateCarStats(string carId)
        {
            // Obtener estadísticas del auto
            float[] stats = GetCarStats(carId);
            
            for (int i = 0; i < statSliders.Length && i < stats.Length; i++)
            {
                statSliders[i].value = stats[i];
            }
            
            for (int i = 0; i < statValues.Length && i < stats.Length; i++)
            {
                statValues[i].text = $"{Mathf.RoundToInt(stats[i] * 100)}%";
            }
        }
        
        private float[] GetCarStats(string carId)
        {
            return carId switch
            {
                "PHEX-01" => new float[] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f },
                "NEON_WRAITH" => new float[] { 1.2f, 1.1f, 0.9f, 0.8f, 1.1f },
                "GRAVRIDER" => new float[] { 0.9f, 0.9f, 1.2f, 1.1f, 1.0f },
                "STREET_BRUISER" => new float[] { 0.8f, 1.1f, 0.9f, 1.2f, 0.9f },
                _ => new float[] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f }
            };
        }
        
        private void UpdatePurchaseInfo(CarPreview preview)
        {
            bool isUnlocked = Managers.SaveManager.Instance?.IsCarUnlocked(preview.carId) ?? true;
            int playerLevel = Managers.SaveManager.Instance?.PlayerLevel ?? 1;
            
            if (purchasePanel)
            {
                purchasePanel.SetActive(!isUnlocked);
            }
            
            if (purchaseText)
            {
                if (playerLevel < preview.unlockLevel)
                {
                    purchaseText.text = $"Unlocks at Level {preview.unlockLevel}";
                }
                else
                {
                    purchaseText.text = $"Cost: {preview.cost} Credits";
                }
            }
            
            if (purchaseButton)
            {
                bool canPurchase = playerLevel >= preview.unlockLevel && 
                    (Managers.SaveManager.Instance?.Credits ?? 0) >= preview.cost;
                    
                purchaseButton.interactable = canPurchase;
                
                if (purchaseButtonText)
                {
                    purchaseButtonText.text = canPurchase ? "Purchase" : "Cannot Purchase";
                }
            }
        }
        
        public void PurchaseSelectedCar()
        {
            CarPreview preview = carPreviews.Find(p => p.carId == selectedCarId);
            if (preview == null) return;
            
            bool purchased = Managers.SaveManager.Instance?.SpendCredits(preview.cost) ?? false;
            if (purchased)
            {
                Managers.SaveManager.Instance?.UnlockCar(selectedCarId);
                UpdatePurchaseInfo(preview);
            }
        }
        
        private void LoadColorOptions()
        {
            ClearColorGrid();
            
            foreach (Color color in availableColors)
            {
                GameObject buttonObj = Instantiate(colorButtonPrefab, colorGrid);
                Button button = buttonObj.GetComponent<Button>();
                
                if (button)
                {
                    Image colorImage = buttonObj.GetComponent<Image>();
                    if (colorImage)
                    {
                        colorImage.color = color;
                    }
                    
                    button.onClick.AddListener(() => SelectColor(color));
                }
            }
            
            if (availableColors.Length > 0)
            {
                SelectColor(availableColors[0]);
            }
        }
        
        private void ClearColorGrid()
        {
            foreach (Transform child in colorGrid)
            {
                Destroy(child.gameObject);
            }
        }
        
        public void SelectColor(Color color)
        {
            selectedColor = color;
            
            if (currentColorDisplay)
            {
                currentColorDisplay.color = color;
            }
            
            SaveColorSelection();
        }
        
        private void LoadDecalOptions()
        {
            ClearDecalGrid();
            
            foreach (Sprite decal in availableDecals)
            {
                GameObject buttonObj = Instantiate(decalButtonPrefab, decalGrid);
                Button button = buttonObj.GetComponent<Button>();
                
                if (button)
                {
                    Image decalImage = buttonObj.GetComponent<Image>();
                    if (decalImage)
                    {
                        decalImage.sprite = decal;
                    }
                    
                    button.onClick.AddListener(() => SelectDecal(decal));
                }
            }
            
            if (availableDecals.Length > 0)
            {
                SelectDecal(availableDecals[0]);
            }
        }
        
        private void ClearDecalGrid()
        {
            foreach (Transform child in decalGrid)
            {
                Destroy(child.gameObject);
            }
        }
        
        public void SelectDecal(Sprite decal)
        {
            selectedDecal = decal;
            
            if (currentDecalDisplay)
            {
                currentDecalDisplay.sprite = decal;
            }
            
            SaveDecalSelection();
        }
        
        private void LoadCustomization()
        {
            // Cargar selecciones guardadas
            string savedCar = PlayerPrefs.GetString("SelectedCar", "PHEX-01");
            string savedColor = PlayerPrefs.GetString("SelectedColor", "#FFFFFFFF");
            string savedDecal = PlayerPrefs.GetString("SelectedDecal", "");
            
            if (ColorUtility.TryParseHtmlString(savedColor, out Color color))
            {
                selectedColor = color;
            }
            
            // Aplicar selecciones
            SelectCar(savedCar);
        }
        
        private void SaveColorSelection()
        {
            string colorHex = ColorUtility.ToHtmlStringRGBA(selectedColor);
            PlayerPrefs.SetString("SelectedColor", "#" + colorHex);
            
            // Aplicar al auto actual
            ApplyCustomizationToCar();
        }
        
        private void SaveDecalSelection()
        {
            // Guardar selección de decal
            PlayerPrefs.SetString("SelectedDecal", selectedDecal ? selectedDecal.name : "");
            
            ApplyCustomizationToCar();
        }
        
        private void ApplyCustomizationToCar()
        {
            // Encontrar auto del jugador y aplicar personalización
            Core.CarController car = FindObjectOfType<Core.CarController>();
            if (car)
            {
                SpriteRenderer renderer = car.GetComponentInChildren<SpriteRenderer>();
                if (renderer)
                {
                    renderer.color = selectedColor;
                }
                
                Core.BoostSystem boost = car.GetComponent<Core.BoostSystem>();
                if (boost)
                {
                    boost.SetBoostColor(selectedColor);
                }
            }
        }
        
        public void SaveCarSelection()
        {
            PlayerPrefs.SetString("SelectedCar", selectedCarId);
            ApplyCustomizationToCar();
        }
        
        public class CarButton : MonoBehaviour
        {
            public Image carImage;
            public Text carNameText;
            public GameObject lockIcon;
            public Text unlockText;
            
            private string carId;
            private System.Action<string> onSelected;
            
            public void Initialize(CarPreview preview, System.Action<string> onSelectedCallback)
            {
                carId = preview.carId;
                onSelected = onSelectedCallback;
                
                if (carImage && preview.previewSprite)
                {
                    carImage.sprite = preview.previewSprite;
                }
                
                if (carNameText)
                {
                    carNameText.text = preview.displayName;
                }
                
                bool isUnlocked = Managers.SaveManager.Instance?.IsCarUnlocked(carId) ?? true;
                int playerLevel = Managers.SaveManager.Instance?.PlayerLevel ?? 1;
                
                if (lockIcon)
                {
                    lockIcon.SetActive(!isUnlocked);
                }
                
                if (unlockText)
                {
                    unlockText.gameObject.SetActive(!isUnlocked);
                    if (!isUnlocked)
                    {
                        unlockText.text = playerLevel >= preview.unlockLevel ? 
                            $"{preview.cost} Credits" : 
                            $"Lvl {preview.unlockLevel}";
                    }
                }
                
                GetComponent<Button>().onClick.AddListener(OnClick);
            }
            
            private void OnClick()
            {
                onSelected?.Invoke(carId);
            }
        }
    }
}
