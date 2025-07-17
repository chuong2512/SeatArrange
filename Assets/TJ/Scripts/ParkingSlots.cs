using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace TJ.Scripts
{
    public class ParkingSlots : MonoBehaviour
    {
        public Transform enterPoint;
        public Transform stopPoint;

        public bool isOccupied;
        [SerializeField] private GameObject normal;
        [SerializeField] private GameObject locked;

        // Start is called before the first frame update
        void Start()
        {
            enterPoint = transform.GetChild(0).transform;
            stopPoint = transform.GetChild(1).transform;
        }

        /*private void OnTriggerStay(Collider other)
        {
            isOccupied = true;
        }*/
        private void OnMouseDown()
        {
            if (GameManager.instance.gameOver || EventSystem.current.IsPointerOverGameObject()) return;
            if (locked.activeInHierarchy)
            {
                CheckLockStatus();
                return;
            }

            UnlockSlot_Callback();
        }

        private void CheckLockStatus()
        {
            ParkingManager.instance.parkingSlot_Rv = this;

            if(GameDataManager.Instance.playerData.intDiamond>=100)
            {
                GameDataManager.Instance.playerData.SubDiamond(100);

                UnlockSlot_Callback();
            }
        }

        public void UnlockSlot_Callback()
        {
            var slots = ParkingManager.instance.slots;
            if (!slots.Contains(this))
            {
                slots.Add(this);
                locked.SetActive(false);
                normal.SetActive(true);
            }
            Debug.Log("Added Slot");
        }
    }
}