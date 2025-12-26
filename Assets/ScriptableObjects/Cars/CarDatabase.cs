using UnityEngine;

namespace SwyPhexLeague.ScriptableObjects.Cars
{
    [CreateAssetMenu(fileName = "CarDatabase", menuName = "SwyPhex/Car Database")]
    public class CarDatabase : ScriptableObject
    {
        public CarData[] cars;
        
        public CarData GetCarById(string carId)
        {
            foreach (CarData car in cars)
            {
                if (car.carId == carId)
                {
                    return car;
                }
            }
            return null;
        }
        
        public CarData GetCarByIndex(int index)
        {
            if (index >= 0 && index < cars.Length)
            {
                return cars[index];
            }
            return null;
        }
        
        public int GetCarIndex(string carId)
        {
            for (int i = 0; i < cars.Length; i++)
            {
                if (cars[i].carId == carId)
                {
                    return i;
                }
            }
            return -1;
        }
        
        public string[] GetAllCarIds()
        {
            string[] ids = new string[cars.Length];
            for (int i = 0; i < cars.Length; i++)
            {
                ids[i] = cars[i].carId;
            }
            return ids;
        }
        
        public string[] GetAllCarNames()
        {
            string[] names = new string[cars.Length];
            for (int i = 0; i < cars.Length; i++)
            {
                names[i] = cars[i].displayName;
            }
            return names;
        }
    }
}
