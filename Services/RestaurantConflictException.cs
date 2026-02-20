namespace BoutiqueElegance.Services
{
    public class RestaurantConflictException : Exception
    {
        public int CurrentRestaurantId { get; set; }
        public int NewRestaurantId { get; set; }
        public string CurrentRestaurantName { get; set; }
        public string NewRestaurantName { get; set; }

        public RestaurantConflictException(
            int currentRestaurantId,
            string currentRestaurantName,
            int newRestaurantId,
            string newRestaurantName)
            : base($"Conflit de restaurant détecté")
        {
            CurrentRestaurantId = currentRestaurantId;
            CurrentRestaurantName = currentRestaurantName;
            NewRestaurantId = newRestaurantId;
            NewRestaurantName = newRestaurantName;
        }
    }
}

