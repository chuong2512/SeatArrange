using Assets.TJ.Scripts;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TJ.Scripts
{
    public class PowerUps : MonoBehaviour
    {
        public PowerUp currentPowerUp = PowerUp.None;
        public int shuffleCarCost;
        public int sortPlayerCost;

        [SerializeField] TextMeshProUGUI title;
        [SerializeField] TextMeshProUGUI info;
        [SerializeField] Image icon;
        [SerializeField] Sprite carShuffleSprite;
        [SerializeField] Sprite playerSortSprite;
        
        [SerializeField] Image bg;
        [SerializeField] Sprite carShuffleBG;
        [SerializeField] Sprite playerSortBG;
        [SerializeField] Sprite vipVehicleSprite;
        [SerializeField] GameObject panel;
        [SerializeField] GameObject background;
        [SerializeField] Button panelCloseButton;
        [SerializeField] Button useWithCoinsButton;
        [SerializeField] Button useWithAdsButton;
        public Button btn_ShuffleVehicles;
        public Button btn_ShufflePlayers;
        public GameObject notEnoughCoinsPopup;

        private bool isPanelClosed = false;
        private bool isInfoPlaying = false;

        private void Start()
        {
            InitializeUI();

            btn_ShuffleVehicles.onClick.AddListener(() =>
            {
                ShowCarShufflePanel();
                SoundController.Instance.PlayOneShot(SoundController.Instance.buttonSound, 0.5f);
                Vibration.Vibrate(30);
            });
            btn_ShufflePlayers.onClick.AddListener(() =>
            {
                ShowPlayerSortPanel();
                SoundController.Instance.PlayOneShot(SoundController.Instance.buttonSound, 0.5f);
                Vibration.Vibrate(30);
            });
            panelCloseButton.onClick.AddListener(() =>
            {
                ClosePanel();
                SoundController.Instance.PlayOneShot(SoundController.Instance.buttonSound, 0.5f);
                Vibration.Vibrate(30);
            });
        }

        private void InitializeUI()
        {
            panel.SetActive(false);
            background.SetActive(false);
            panel.transform.localScale = Vector3.zero;
            notEnoughCoinsPopup.transform.localScale = Vector3.zero;
        }

        private void ShowCarShufflePanel()
        {
            SetPowerUpPanel(PowerUp.ShuffleCar, "Shuffle", "Rearrange the <color=green>COLOR</color> of the Vehicles in the parking lot", carShuffleSprite, carShuffleBG);
            useWithCoinsButton.onClick.AddListener(() => UsePowerUpWithCoins(shuffleCarCost, VehicleController.instance.RandomVehicleColors));
            useWithAdsButton.onClick.AddListener(() =>
            {
                //call the ads here
                ISManager.instance.ShowRewardedVideo(AdState.CarShuffle);
                // call below lines after the ad
            });
        }

        public void CarShuffle_CallBack()
        {
            ClosePanel();
            VehicleController.instance.RandomVehicleColors();
            SoundController.Instance.PlayOneShot(SoundController.Instance.buttonSound, 0.5f);
            Vibration.Vibrate(30);
        }
        
        private void ShowPlayerSortPanel()
        {
            SetPowerUpPanel(PowerUp.SortPlayers, "Sort", "Sort the <color=green>PASSENGERS</color> according to Vehicle Colors", playerSortSprite, playerSortBG);
            useWithCoinsButton.onClick.AddListener(() => UsePowerUpWithCoins(sortPlayerCost, ShufflePlayersPowerUp));
            useWithAdsButton.onClick.AddListener(() =>
            {
                //call the ads
                ISManager.instance.ShowRewardedVideo(AdState.PlayerSort);
                //callback for he powerUp
              
            });
        }

        public void PlayerSort_CallBack()
        {
            ClosePanel();
            ShufflePlayersPowerUp();
            SoundController.Instance.PlayOneShot(SoundController.Instance.buttonSound, 0.5f);
            Vibration.Vibrate(30);
        }

        private void SetPowerUpPanel(PowerUp powerUp, string titleText, string infoText, Sprite iconSprite,  Sprite bgSprite)
        {
            currentPowerUp = powerUp;
            title.text = titleText;
            info.text = infoText;
            icon.sprite = iconSprite;
            icon.SetNativeSize();
            bg.sprite=bgSprite;
            OpenPanel();
        }

        private void OpenPanel()
        {
            openTween?.Kill();
            panel.SetActive(true);
            background.SetActive(true);
            closeTween = panel.transform.DOScale(Vector3.one, 0.3f);
        }
        Tween openTween;
        Tween closeTween;
        private void ClosePanel()
        {
            if (isPanelClosed)
                return;

            isPanelClosed = true;
            closeTween?.Kill();
            ResetButtonListeners();
            background.SetActive(false);
            openTween = panel.transform.DOScale(Vector3.zero, 0.3f).OnComplete(() =>
            {
                isPanelClosed = false;
                panel.SetActive(false);
            });
        }

        private void UsePowerUpWithCoins(int cost, System.Action powerUpAction)
        {
            int coins = CoinsManager.Instance.GetTotalCoins();
            if (coins >= cost)
            {
                CoinsManager.Instance.DeductCoins(cost);
                ClosePanel();
                powerUpAction.Invoke();
            }
            else
            {
                PlayInfoPopup("Not Enough Coins!");
                return;
            }
            SoundController.Instance.PlayOneShot(SoundController.Instance.buttonSound, 0.5f);
            Vibration.Vibrate(30);
        }

        private void ShufflePlayersPowerUp()
        {
            var cars = new List<Vehicle>(ParkingManager.instance.parkedVehicles);
            var players = PlayerManager.instance.playersInScene;
            int totalRemainingSeats = cars.Sum(car => car.SeatCount - car.playersInSeat);

            if (totalRemainingSeats < 24)
            {
                var additionalCars = VehicleController.instance.vehicles
                                    .Where(car => !car.CheckForObstacles())
                                    .ToList();

                foreach (var car in additionalCars)
                {
                    cars.Add(car);
                    totalRemainingSeats += car.SeatCount - car.playersInSeat;
                    if (totalRemainingSeats >= 24) break;
                }
            }

            int playersMatched = 0;
            for (int i = 0; i < cars.Count && playersMatched < 24; i++)
            {
                int remainingSeats = cars[i].SeatCount - cars[i].playersInSeat;
                for (int j = playersMatched; j < players.Count && remainingSeats > 0 && playersMatched < 24; j++)
                {
                    if (cars[i].vehicleColor == players[j].color)
                    {
                        SwapPlayerColors(playersMatched, j);
                        playersMatched++;
                        remainingSeats--;
                    }
                }
            }

            if (!PlayerManager.instance.isColormatched)
                EventManager.OnNewVehArrived?.Invoke();
        }


        private void SwapPlayerColors(int playerIndex1, int playerIndex2)
        {
            var players = PlayerManager.instance.playersInScene;
            var tempColor = players[playerIndex1].color;
            players[playerIndex1].ChangeColor(players[playerIndex2].color);
            players[playerIndex2].ChangeColor(tempColor);
        }

        private void PlayInfoPopup(string message)
        {
            if (isInfoPlaying)
                return;

            isInfoPlaying = true;
            var infoText = notEnoughCoinsPopup.GetComponent<TextMeshProUGUI>();
            infoText.text = message;
            notEnoughCoinsPopup.transform.DOScale(Vector3.one, 0.2f);
            DOVirtual.DelayedCall(2f, () =>
            {
                notEnoughCoinsPopup.transform.DOScale(Vector3.zero, 0.2f).OnComplete(() => isInfoPlaying = false);
            });
            SoundController.Instance.PlayOneShot(SoundController.Instance.nocoinPOP, 0.5f);
            Vibration.Vibrate(30);
        }
        private void ResetButtonListeners()
        {
            useWithCoinsButton.onClick.RemoveAllListeners();
            useWithAdsButton.onClick.RemoveAllListeners();
        }
    }

    public enum PowerUp
    {
        None,
        ShuffleCar,
        SortPlayers
    }
}
