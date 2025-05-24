using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Database; // Add this using directive for Firebase Realtime Database
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration; // For configuration
using System;
using System.IO; // For File operations
using System.Threading.Tasks;

namespace C__with_firebase
{
    public sealed class FirebaseAuthenticationService
    {
        private static readonly FirebaseAuthenticationService instance = new FirebaseAuthenticationService();

        private readonly FirebaseAuthClient _firebaseAuthClient;
        private FirebaseClient _firebaseDatabaseClient;

        // Firebase Config - these will be loaded from JSON
        private readonly string FirebaseApiKey;
        private readonly string FirebaseDatabaseUrl;
        private readonly string FirebaseAuthDomain;

        private FirebaseAuthenticationService()
        {
            // Load configuration from firebaseConfig.json
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // Current folder
                .AddJsonFile("firebaseConfig.json", optional: false, reloadOnChange: true);

            var configuration = builder.Build();

            // Bind Firebase section to a local config object
            var firebaseConfigSection = configuration.GetSection("Firebase");
            FirebaseApiKey = firebaseConfigSection["ApiKey"];
            FirebaseDatabaseUrl = firebaseConfigSection["DatabaseUrl"];
            FirebaseAuthDomain = firebaseConfigSection["AuthDomain"];

            // Initialize Firebase Auth Client
            var config = new FirebaseAuthConfig
            {
                ApiKey = FirebaseApiKey,
                AuthDomain = FirebaseAuthDomain,
                Providers = new FirebaseAuthProvider[]
                {
                    new EmailProvider()
                },
            };

            _firebaseAuthClient = new FirebaseAuthClient(config);
        }

        public static FirebaseAuthenticationService Instance => instance;

        public async Task<UserCredential> SignInAsync(string email, string password)
        {
            var userCredential = await _firebaseAuthClient.SignInWithEmailAndPasswordAsync(email, password);
            return userCredential;
        }

        public async Task<UserCredential> RegisterAsync(string email, string password)
        {
            var userCredential = await _firebaseAuthClient.CreateUserWithEmailAndPasswordAsync(email, password);
            return userCredential;
        }

        public void SignOut()
        {
            _firebaseAuthClient.SignOut();
            _firebaseDatabaseClient = null;
        }

        public User GetCurrentUser()
        {
            return _firebaseAuthClient.User;
        }

        public bool IsUserLoggedIn()
        {
            return GetCurrentUser() != null;
        }

        public FirebaseClient GetFirebaseDatabaseClient()
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                throw new InvalidOperationException("User is not logged in. Cannot access Firebase Database.");
            }

            if (_firebaseDatabaseClient == null)
            {
                _firebaseDatabaseClient = new FirebaseClient(
                    FirebaseDatabaseUrl,
                    new FirebaseOptions
                    {
                        AuthTokenAsyncFactory = async () =>
                        {
                            var token = await currentUser.GetIdTokenAsync();
                            return token;
                        }
                    });
            }

            return _firebaseDatabaseClient;
        }
    }
}
