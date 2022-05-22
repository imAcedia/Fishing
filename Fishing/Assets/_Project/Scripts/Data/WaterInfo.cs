using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fishing
{
    [CreateAssetMenu(menuName = "Game Data/Water Info", order = AssetMenu.order)]
    public class WaterInfo : ScriptableObject
    {
        [SerializeField]
        protected List<FishInfo> AvailableFishes = new();

        public int AvailableFishCount => AvailableFishes.Count;
        public FishInfo[] GetAvailableFishes() => AvailableFishes.ToArray();

        public FishInfo GetRandomFish()
        {
            return AvailableFishes[Random.Range(0, AvailableFishes.Count)];
        }
    }
}
