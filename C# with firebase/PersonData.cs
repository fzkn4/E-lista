using System; // Required for DateTime

namespace C__with_firebase.Models // Recommended to put data models in a 'Models' namespace
{
    public class PersonData
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; } // Made nullable in case it's not always provided
        public string LastName { get; set; }
        public string BirthPlace { get; set; }
        public string BarangayAddress { get; set; }
        public string CityAddress { get; set; }
        public string ProvinceAddress { get; set; }
        public string Gender { get; set; } // Will store "Male" or "Female"
        public string MaritalStatus { get; set; } // Will store "Single", "Married", etc.
        public long BirthDateTimestamp { get; set; } // Storing birth date as Unix timestamp (seconds since epoch)
        public string AddedByUserId { get; set; } // Optional: Store the ID of the user who added this record
        public long AddedTimestamp { get; set; } // Optional: Timestamp when the record was added
    }
}
